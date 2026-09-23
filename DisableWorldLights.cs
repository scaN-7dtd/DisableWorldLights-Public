using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace DisableWorldLights
{
    public class ModApi : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            ConfigLoader.Load(_modInstance.Path);

            var harmony = new Harmony("scan_disableworldlights");
            harmony.PatchAll();

            Debug.Log("[DisableWorldLights] Loaded.");
        }
    }

    internal static class ConfigLoader
    {
        public static Dictionary<string, bool> EmissionTargets = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, bool> BlockLightTargets = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, bool> POILightExceptions = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, bool> LightLODTargets = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public static bool DebugLogs = false;
        public static bool ExcludeTraderAreas = false;

        public static void Load(string modPath)
        {
            string configPath = Path.Combine(modPath, "config.xml");

            if (!File.Exists(configPath))
            {
                Debug.LogWarning("[DisableWorldLights] Config file not found at: " + configPath);
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(configPath);

                if (doc.Root.Attribute("debugLogs") != null)
                    bool.TryParse(doc.Root.Attribute("debugLogs").Value, out DebugLogs);

                LoadTargetSection(doc, "Category", EmissionTargets);
                LoadTargetSection(doc, "LightCategory", BlockLightTargets);
                LoadTargetSection(doc, "LightLODCategory", LightLODTargets);

                XElement exceptionsRoot = doc.Root.Element("POILightExceptions");
                if (exceptionsRoot != null)
                {
                    foreach (XElement block in exceptionsRoot.Elements("Block"))
                    {
                        string name = block.Attribute("name")?.Value;
                        if (string.IsNullOrEmpty(name)) continue;

                        bool exclude = true;
                        if (block.Attribute("exclude") != null)
                            bool.TryParse(block.Attribute("exclude").Value, out exclude);

                        POILightExceptions[name] = exclude;
                    }
                }

                XElement traderAreaEl = doc.Root.Element("ExcludeTraderAreas");
                if (traderAreaEl != null && traderAreaEl.Attribute("enabled") != null)
                    bool.TryParse(traderAreaEl.Attribute("enabled").Value, out ExcludeTraderAreas);

                Debug.Log($"[DisableWorldLights] Config loaded: {EmissionTargets.Count} emissive targets, {BlockLightTargets.Count} light targets, {LightLODTargets.Count} special targets, {POILightExceptions.Count} POI light exceptions. Debug logs: {DebugLogs}. Exclude trader areas: {ExcludeTraderAreas}.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[DisableWorldLights] Error reading config file: " + ex.Message);
            }
        }

        private static void LoadTargetSection(XDocument doc, string categoryTag, Dictionary<string, bool> target)
        {
            foreach (XElement category in doc.Root.Elements(categoryTag))
            {
                bool categoryTurnOff = true;
                if (category.Attribute("turnOff") != null)
                    bool.TryParse(category.Attribute("turnOff").Value, out categoryTurnOff);

                foreach (XElement block in category.Elements("Block"))
                {
                    string name = block.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;

                    bool blockTurnOff = true;
                    if (block.Attribute("turnOff") != null)
                        bool.TryParse(block.Attribute("turnOff").Value, out blockTurnOff);

                    target[name] = categoryTurnOff && blockTurnOff;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Block), "OnBlockEntityTransformAfterActivated")]
    internal static class DisableEmissiveGlowsPatch
    {
        private static void Postfix(Block __instance, WorldBase _world, Vector3i _blockPos, BlockEntityData _ebcd)
        {
            if (_ebcd == null || !_ebcd.bHasTransform || _ebcd.transform == null) return;

            if (ConfigLoader.ExcludeTraderAreas && _world is World w && w.IsWithinTraderArea(_blockPos)) return;

            string blockName = __instance.GetBlockName();
            if (!ConfigLoader.EmissionTargets.TryGetValue(blockName, out bool enabled) || !enabled) return;

            foreach (Renderer r in _ebcd.transform.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_EmissionColor"))
                        m.SetColor("_EmissionColor", Color.black);
                    if (m.HasProperty("_EmissionMultiply"))
                        m.SetFloat("_EmissionMultiply", 0f);
                    m.DisableKeyword("_EMISSION");
                }
            }

            foreach (Light lightComp in _ebcd.transform.GetComponentsInChildren<Light>(true))
            {
                LightLOD lod = lightComp.GetComponent<LightLOD>();
                if (lod != null)
                    lod.SwitchOnOff(false);
                else
                    lightComp.enabled = false;
            }

            if (ConfigLoader.DebugLogs)
                Debug.Log("[DisableWorldLights] emissive-block turned off: " + blockName);
        }
    }

    [HarmonyPatch(typeof(BlockLight), "updateLightState")]
    internal static class ForceBlockLightOffPatch
    {
        private static void Prefix(BlockLight __instance, WorldBase _world, Vector3i _blockPos, ref BlockValue _blockValue)
        {
            if (ConfigLoader.ExcludeTraderAreas && _world is World w && w.IsWithinTraderArea(_blockPos)) return;

            string blockName = __instance.GetBlockName();
            if (!ConfigLoader.BlockLightTargets.TryGetValue(blockName, out bool enabled) || !enabled) return;

            if ((_blockValue.meta & 2) != 0)
            {
                _blockValue.meta = (byte)(_blockValue.meta & ~2);
                _world.SetBlockRPC(_blockPos, _blockValue);

                if (ConfigLoader.DebugLogs)
                    Debug.Log("[DisableWorldLights] light-block turned off: " + blockName);
            }
        }
    }

    [HarmonyPatch(typeof(BlockLight), "OnBlockEntityTransformAfterActivated")]
    internal static class ForcePoiLightsOffPatch
    {
        private static void Postfix(BlockLight __instance, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
        {
            if (ConfigLoader.ExcludeTraderAreas && _world is World w && w.IsWithinTraderArea(_blockPos)) return;

            var props = __instance.Properties.Values;

            if (!props.ContainsKey("IndexName") || props["IndexName"] != "POILight") return;

            string blockName = __instance.GetBlockName();

            if (ConfigLoader.POILightExceptions.TryGetValue(blockName, out bool excluded) && excluded)
            {
                if (ConfigLoader.DebugLogs)
                    Debug.Log("[DisableWorldLights] poilight-block exempted, left untouched: " + blockName);
                return;
            }

            if ((_blockValue.meta & 2) == 0) return;

            BlockValue offValue = _blockValue;
            offValue.meta = (byte)(offValue.meta & ~2);
            _world.SetBlockRPC(_blockPos, offValue);

            if (ConfigLoader.DebugLogs)
                Debug.Log("[DisableWorldLights] poilight-block turned off: " + blockName);
        }
    }

    [HarmonyPatch(typeof(LightLOD), "FrameUpdate")]
    internal static class ForceLightLODOffPatch
    {
        private static readonly ConditionalWeakTable<LightLOD, object> loggedInstances = new ConditionalWeakTable<LightLOD, object>();

        private static void Postfix(LightLOD __instance, BlockEntityData ___bed)
        {
            if (___bed == null || !___bed.bHasTransform || ___bed.transform == null) return;

            if (ConfigLoader.ExcludeTraderAreas && GameManager.Instance.World.IsWithinTraderArea(___bed.pos)) return;

            string blockName = ___bed.blockValue.Block.GetBlockName();
            if (!ConfigLoader.LightLODTargets.TryGetValue(blockName, out bool enabled) || !enabled) return;

            __instance.SetCulled(true);
            __instance.SetEmissiveColor(false);

            if ((bool)__instance.LitRootObject)
                __instance.LitRootObject.SetActive(false);

            foreach (Renderer r in ___bed.transform.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_EmissionColor"))
                        m.SetColor("_EmissionColor", Color.black);
                    if (m.HasProperty("_EmissionMultiply"))
                        m.SetFloat("_EmissionMultiply", 0f);
                    m.DisableKeyword("_EMISSION");
                }
            }

            if (ConfigLoader.DebugLogs && !loggedInstances.TryGetValue(__instance, out _))
            {
                loggedInstances.Add(__instance, null);
                Debug.Log("[DisableWorldLights] lodlight-block turned off: " + blockName);
            }
        }
    }

    [HarmonyPatch(typeof(World), "SetBlocksRPC", new Type[] { typeof(List<BlockChangeInfo>) })]
    internal static class ForceBlockLightOffOnWritePatch
    {
        private static void Prefix(World __instance, List<BlockChangeInfo> _blockChangeInfo)
        {
            for (int i = 0; i < _blockChangeInfo.Count; i++)
            {
                BlockChangeInfo info = _blockChangeInfo[i];
                if (!info.bChangeBlockValue) continue;

                Block block = info.blockValue.Block;
                if (block == null) continue;

                if (ConfigLoader.ExcludeTraderAreas && __instance.IsWithinTraderArea(info.blockValueRef.BlockPosition)) continue;

                string blockName = block.GetBlockName();
                if (!ConfigLoader.BlockLightTargets.TryGetValue(blockName, out bool enabled) || !enabled) continue;

                if ((info.blockValue.meta & 2) != 0)
                {
                    BlockValue bv = info.blockValue;
                    bv.meta = (byte)(bv.meta & ~2);
                    info.blockValue = bv;

                    if (ConfigLoader.DebugLogs)
                        Debug.Log("[DisableWorldLights][Light Block] Blocked external re-enable attempt (batch): " + blockName);
                }
            }
        }
    }
}