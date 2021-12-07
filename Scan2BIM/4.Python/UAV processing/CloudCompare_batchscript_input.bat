C:
cd "C:\Program Files\CloudCompare"
:: fchooser.bat
:: launches a folder chooser and outputs choice to the console
:: https://stackoverflow.com/a/15885133/1683264

@echo off
setlocal

set "psCommand="(new-object -COM 'Shell.Application')^
.BrowseForFolder(0,'Please choose a folder.',0,'K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames').self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "folder=%%I"
set /P max="What Is the max distance between the clouds? eg. 0.5m : "
set /P min="What Is the min distance between the clouds? eg -0.5m : "

ECHO 1. LAMBERT08_ELLIPS
ECHO 2. PCDLAMBERT08_TAW
ECHO 3. PCDLAMBERT72_ELLIPS
ECHO 4. PCDLAMBERT72_TAW

set /P numb="What Is the projection? (give a number, see above) : "
set proj=0

setlocal enabledelayedexpansion

echo numb is !numb!

if "!numb!" == "1"  (set  proj=LAMBERT08_ELLIPS)
if "!numb!" == "2"  (set  proj=LAMBERT08_TAW)
if "!numb!" == "3"  (set  proj=LAMBERT72_ELLIPS)
if "!numb!" == "4"  (set  proj=LAMBERT72_TAW)


echo You chose folder !folder!
echo maximum is !max!
echo minimum is !min!
echo projection is !proj!
cloudcompare -silent -NO_TIMESTAMP -auto_save off -LOG_FILE "!folder!\!proj!_Log.txt" -o "!folder!\PCD!proj!.e57"  -SS SPATIAL 0.1 -COMPUTE_NORMALS -ORIENT_NORMS_MST 50 -o "!folder!\TIN!proj!.obj" -SAMPLE_MESH DENSITY 100 -COMPUTE_NORMALS -ORIENT_NORMS_MST 50 -C2C_DIST -SPLIT_XYZ -SET_ACTIVE_SF "0" -FILTER_SF "!min!" "!max!" -save_clouds file "'!folder!\!proj!_output_CC' '!folder!\!proj!_Mesh_CC'"
endlocal
pause

