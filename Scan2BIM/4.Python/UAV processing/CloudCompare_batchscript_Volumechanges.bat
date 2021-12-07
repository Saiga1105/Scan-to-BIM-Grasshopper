C:
cd "C:\Program Files\CloudCompare"
:: fchooser.bat
:: launches a folder chooser and outputs choice to the console
:: https://stackoverflow.com/a/15885133/1683264

@echo off
setlocal

set "psCommand="(new-object -COM 'Shell.Application')^
.BrowseForFolder(0,'Please choose a folder.',0,'K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames').self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "folder1=%%I"

.BrowseForFolder(0,'Please choose a folder.',0,'K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames').self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "folder2=%%I"

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


echo You chose folder !folder1!
echo You chose folder !folder2!
echo maximum is !max!
echo minimum is !min!
echo projection is !proj!
cloudcompare -silent -NO_TIMESTAMP -auto_save off -LOG_FILE "!folder2!\!proj!_Volume_Log.txt" -o "!folder1!\PCD!proj!.e57"  -SS SPATIAL 0.1  -o "!folder2!\PCD!proj!.e57" -SS SPATIAL 0.1 -VOLUME -GRID_STEP 0.1 -FILTER_SF "!min!" "!max!" -save_clouds file "'!folder1!\!proj!_Volume' '!folder2!\!proj!_Volume''!folder1!\!proj!_Volume_Result'"
endlocal
pause

