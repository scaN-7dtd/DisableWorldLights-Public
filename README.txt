
[Disable World Lights by scaN] 

*This mod was made with the assistance of the free (web) versions of ChatGPT, DeepSeek, and Claude.


/// WARNING: this mod could break the "Restore Power" quest. I couldn't test it because a confirmed 
bug prevents testing the quest in the prefab editor.

/// Attention: If you visit a trader while the option is set to "false" and then change it to "true", 
the LIGHT BLOCKS will not turn on again. This is due to how the code handles these blocks.
If you used this mod before the update, you will need to restore the backup of your save that you 
made in order to fully exclude traders.



 TABLE OF CONTENTS
===================

1. Mod Information
   1.1 Features
   1.2 Notes
   1.3 Default Settings
   1.4 Untouched Blocks
     
2. General Information  
   2.1 Requirements
   2.2 Installation
   2.3 Compatibility
   
3. Mod Configuration
   3.1 How to exclude Trader Areas
   3.2 How to exclude Emissive-Special blocks
   3.3 How to exclude Light blocks
   3.4 How to exclude Sign Shop blocks

4. Links - Bug Reports


====================================================================================================

1. MOD INFORMATION

This mod is the result of a shift in direction from what I was originally trying to achieve: 
disabling all lights and fires in the world for a specific "overhaul" I'm doing for myself. 
In the process, I was forced to specify certain block prefab names, so I changed it to use the 
block name instead, and moved that to a config file. With that possibility now open, this ended 
up including an option to disable or exclude almost every single block from this change.
So you shouldn't see this as a mod by design, but rather as a mod by consequence. The default 
config is just a logical choice aligned with the mod's title.

----------------------------------------------------------------------------------------------------

1.1 FEATURES

- Disables the light/emission emited by blocks present in the world, including POIs. 
- Blocks that emmit "fire" (burning barrel, ember piles, etc) can also be disabled.
- Option to exclude Trader Areas (on by default).

----------------------------------------------------------------------------------------------------

1.2 NOTES

- Player placed blocks (electric system) are fully functional.
- Every block can be excluded from this change in the configuration file.
- Works in already created saves.

----------------------------------------------------------------------------------------------------

1.2 DEFAULT CONFIGURATION

- Now Trader Areas are excluded by default.

- These blocks are EXCLUDED in the config.xml file:

- Trader signs
- Vending machines
- Fire blocks
- Landmines

----------------------------------------------------------------------------------------------------

1.3 UNTOUCHED BLOCKS

- These two blocks are still "on":

- Broken Pipe Fire Hazards - This block uses another system, to disable them use my mod "DisableBrokenPipeFireHazards"

- Quest Generators - WARNING: this mod could break the "Restore Power" quest. I couldn't test it 
because a confirmed bug prevents testing the quest in the prefab editor.
https://community.thefunpimps.com/threads/regression-playtest-no-longer-creates-start-rally-marker-in-prefab-editor.48620/

====================================================================================================

2. GENERAL INFORMATION

- A new savegame is NOT NEEDED.

- The mod was developed and tested in single-player on game version 3.2.0 (b10) on Windows 10.

- This mod improves performance by disabling lights, and the methods used to disable them 
have no performance impact.

----------------------------------------------------------------------------------------------------

2.1 REQUIREMENTS

- Easy AntiCheat disabled.

- Game version: Should work in any 3.X version.

- Dedicated Servers: I have no clue about what information the server stores and updates, but in 
case of working should be installed in both server and clients, because of the code.

----------------------------------------------------------------------------------------------------

2.2 INSTALLATION

1) Make a backup of your game save file.

2) Unzip the downloaded file and put the folder "DisableWorldLights" inside your "mods" folder.

----------------------------------------------------------------------------------------------------

2.3 COMPATIBILITY

- The mod should be INCOMPATBILE with any mod that changes the light of world placed blocks.

- The mod should be COMPATIBLE with any mod that adds light blocks that uses the electricity 
system, also any emissive/special block thats not listed in the config.xml file should work 
(its automatically excluded).


====================================================================================================

3. MOD CONFIGURATION

- You can exclude any block in the game from being disabled, leaving him in his original state, 
dont get confused, a block can be already turned off in the world, for example candle-walls,
trader signs that turn off in the night, etc.

- The configuration depends on the type of block you want to exclude, there is two main types: 
"Emissive-Special" and "Light" blocks.

- Remember to learn how to comment/uncomment code in xml files: 
https://www.tutorialspoint.com/xml/xml_comments.htm

----------------------------------------------------------------------------------------------------

3.1 HOW TO EXCLUDE TRADER AREAS

/// Attention: If you visit a trader while the option is set to "false" and then change it to "true", 
the LIGHT BLOCKS will not turn on again. This is due to how the code handles these blocks.
If you used this mod before the update, you will need to restore the backup of your save that you 
made in order to fully exclude traders.



true = default lights

false = all lights off (trader signs still on with default config)


<ExcludeTraderAreas enabled="true" />


----------------------------------------------------------------------------------------------------

3.2 HOW TO EXCLUDE EMISSIVE-SPECIAL BLOCKS
 
This category has the blocks separated in groups, you can exclude an entire group or each block 
individually.



- EXCLUDE A BLOCK GROUP - put "false" in the first line of a group, for example:

	<!-- *** TORCH-CANDLE -->
	<LightCategory name="Torchs" turnOff="false">
		<Block name="wallTorchLight" turnOff="true" />
		<Block name="candleWallLight" turnOff="true" />
		<Block name="candleTableLight" turnOff="true" />	
	</LightCategory>



- DISABLE A BLOCK GROUP - put "true" in the first lien of a group, for example:

	<!-- *** RECESSED LIGHTS -->
	<Category name="RecessedLights" turnOff="true">
		<Block name="recessedLight" turnOff="true" />
		<Block name="recessedLightOffset" turnOff="true" />
	</Category>



- EXCLUDE AN INDIVIDUAL BLOCK - make sure the GROUP is on "true", and then put "false" in the line 
of the block you want to exclude, for example:
	
	<!-- *** MOTION SENSOR -->
	<Category name="MotionSensor" turnOff="true">
		<Block name="motionSensorPOI" turnOff="true" />
		<Block name="motionSensorLeftPOI" turnOff="false" />
		<Block name="motionSensorRightPOI" turnOff="true" />
	</Category>


----------------------------------------------------------------------------------------------------

3.3 HOW TO EXCLUDE LIGHT BLOCKS


1) First of all you need to verify the block is not an Emissive-Special Block, to do that open the 
file "BlocksList.txt" located inside the folder "DisableWorldLights", and search for his name with 
CTRL + F or CTRL + B, if the block isnt there, then is a Light Block.

2) Open the file "config.xml" located inside the folder "DisableWorldLights", scroll all the way 
down and in the "EXCLUDE LIGHT BLOCKS" uncomment the structure and add a line for your block.

"exclude="true" = block original state	
	
	
For example, to exclude all the "Gooseneck blocks":
	
	<POILightExceptions>
		<Block name="lightWallGooseneckWhite" exclude="true" />
		<Block name="lightWallGooseneckBrown" exclude="true" />
		<Block name="lightWallGooseneckRed" exclude="true" />
		<Block name="lightWallGooseneckOrange" exclude="true" />
		<Block name="lightWallGooseneckYellow" exclude="true" />
		<Block name="lightWallGooseneckGreen" exclude="true" />
		<Block name="lightWallGooseneckBlue" exclude="true" />
		<Block name="lightWallGooseneckPurple" exclude="true" />
		<Block name="lightWallGooseneckGrey" exclude="true" />
		<Block name="lightWallGooseneckBlack" exclude="true" />
		<Block name="lightWallGooseneckPink" exclude="true" />
		<Block name="lightWallGooseneckArmyGreen" exclude="true" />
	</POILightExceptions>


----------------------------------------------------------------------------------------------------

3.4 HOW TO EXCLUDE SIGN SHOP BLOCKS

The Sign Shop blocks are the ones outside the different shops in the game (Working Stiff´s, 
Crack a book, Pass n´Gass, etc.) These blocks cannot be toggled on or off, but they have separate 
on and off models. We will therefore replace them with their off-state model.

To "exclude" them just go to "DisableWorldLights\Config\blocks.xml" and comment them, or remove 
the file and folder completely.


- Exclude an entire group:

	<!-- *** GAS 
	<set xpath="/blocks/block[@name='signShopGasLit']/property[@name='Model']/@value">@:Entities/Signs/gasSignPrefab.prefab</set>
	<set xpath="/blocks/block[@name='signShopGasWallLit']/property[@name='Model']/@value">@:Entities/Signs/gasSignWallPrefab.prefab</set>
	<set xpath="/blocks/block[@name='signShopGasLargeLit']/property[@name='Model']/@value">@:Entities/Signs/gasSign_LargePrefab.prefab</set>
	<set xpath="/blocks/block[@name='signShopGasLargeWallLit']/property[@name='Model']/@value">@:Entities/Signs/gasSignWall_LargePrefab.prefab</set> -->
	
	
- Exclude an individual sign:

	<!-- *** GAS -->
	<set xpath="/blocks/block[@name='signShopGasLit']/property[@name='Model']/@value">@:Entities/Signs/gasSignPrefab.prefab</set>
	<!-- <set xpath="/blocks/block[@name='signShopGasWallLit']/property[@name='Model']/@value">@:Entities/Signs/gasSignWallPrefab.prefab</set> -->
	<set xpath="/blocks/block[@name='signShopGasLargeLit']/property[@name='Model']/@value">@:Entities/Signs/gasSign_LargePrefab.prefab</set>
	<set xpath="/blocks/block[@name='signShopGasLargeWallLit']/property[@name='Model']/@value">@:Entities/Signs/gasSignWall_LargePrefab.prefab</set> 


====================================================================================================

4.0 HELP - BUG REPORTS


# Links:

- NexusMods profile: https://www.nexusmods.com/profile/scaN7dtd

- 7daystodiemods profile: https://7daystodiemods.com/profiles/scan

- Github: https://github.com/scaN-7dtd


# Bug Reports: use the dedicated bug report section on the respective mod page or dm me 
in discord: scan88


====================================================================================================