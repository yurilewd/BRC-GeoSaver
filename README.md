## GeoSaver

GeoSaver is an in-game phone app for saving, organizing and loading player positions.

You can name your save positions and organize them into folders for better organization, or share your saved positions with others.

GeoSaver supports an effectively endless amount of saved positions so feel free to go wild and make as many as you want.

By default GeoSaver comes with some positions already set up.

 - All taxi positions
 - All entrances for each map
 - Each of the movestyles in the Hideout

When loading a position outside of your current stage you will be taken to that stage and moved to your selected position, so no need to deal with making sure you're in the correct stage, just select where you want to go and you'll go.

GeoSaver does fully support Mapstation stages too so feel free to make new saves in all of your favorite maps.

## Other Things To Note
When making a save it will be given a default name which is the name of the map followed by the coordinates, changing the name of saved positions or managing the folder structure needs to be done through your file explorer.
If you use r2modman you can find this folder by:

 - Opening the Settings menu
 - Click the "Browse profile folder" option
 - Open the "BepInEx" folder
 - Open the "config" folder
 - Open the "Locations" folder
 
This is where you will find all of your saved positions, the folders you see in game are found here. These folders are not restricted at all, feel free to add new ones, delete any, reorganize, nest folders in as many layers deeps as you want.
 
Changing the name of a saved location is done by simply renaming the file, make sure that it stays as a .txt file though as GeoSaver only searches for .txt files.


GeoSaver also comes with the ability to load the last loaded position in the current stage and a button to overwrite that last loaded position manually, these are labeled "Load Last Location" and "Save Temp Location" in the phone app. You also have a keyboard hotkey for each of these which is by default "x" to load and "z" to save, these can be changed in the config file for GeoSaver.

## How To Install Manually

If you want to install GeoSaver without r2modman you simply need to:

 - Place the GeoSaver.dll, GeoSaver_Icon.png, and MapStation.API.dll into the BepInEx plugins folder
 - Place the Locations folder into the BepInEx config folder

