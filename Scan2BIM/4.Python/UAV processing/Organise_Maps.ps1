Function Get-EXIFDataFromJPGFile
   {

    <#
    .NOTES
    =============================================================================================================================================
    Created with:     Windows PowerShell ISE
    Created on:       06-July-2020
    Created by:       Willem-Jan Vroom
    Organization:     
    Functionname:     Get-EXIFDataFromJPGFile
    =============================================================================================================================================
    .SYNOPSIS

    This function reads the latitude and longitude data from a JPG file.
    If no valid data is found, the return codes will be 1024 and 1024.
    This code is based on
    http://www.forensicexpedition.com/2017/08/03/imagemapper-a-powershell-metadata-and-geo-maping-tool-for-images/

    #>

    Param
     (
      [String] $FileName
     )
   
   
    $img    = New-Object -TypeName system.drawing.bitmap -ArgumentList $FileName
    $Encode = New-Object System.Text.ASCIIEncoding
    $global:GPSInfo = $true
    $global:GPSLat  = ""
    $global:GPSLon  = ""
     
    # =============================================================================================================================================
    # Try to get the latitude (N or S) from the image.
    # If not successfull then this information is not in the image
    # and quit with the numbers 1024 and 1024.
    # =============================================================================================================================================

    Try
     {
     
      $global:LatNS = $Encode.GetString($img.GetPropertyItem(1).Value)
     }
      Catch
     {
      $global:GPSInfo = $False
     }
               
    If ($global:GPSInfo -eq $true)
     {
      [double]$global:LatDeg = (([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(2).Value, 0))  / ([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(2).Value, 4)))
      [double]$global:LatMin = (([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(2).Value, 8))  / ([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(2).Value, 12)))
      [double]$global:LatSec = (([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(2).Value, 16)) / ([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(2).Value, 20)))
   
      $global:LonEW = $Encode.GetString($img.GetPropertyItem(3).Value)
      [double]$global:LonDeg = (([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(4).Value, 0))  / ([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(4).Value, 4)))
      [double]$global:LonMin = (([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(4).Value, 8))  / ([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(4).Value, 12)))
      [double]$global:LonSec = (([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(4).Value, 16)) / ([Decimal][System.BitConverter]::ToInt32($img.GetPropertyItem(4).Value, 20)))



      #$GPSLat = $($LatDeg.ToString("###")) + "º "+$($LatMin.ToString("##"))+"' "+$($LatSec.ToString("##"))+ $([char]34) + " "+$LatNS
      #$GPSLon = $($LonDeg.ToString("###")) + "º "+$($LonMin.ToString("##"))+"' "+$($LonSec.ToString("##"))+ $([char]34) + " "+$LonEW

      $global:GPSLat = "$([int]$global:LatDeg)º $([int]$global:LatMin)' $([int]$global:LatSec)$([char]34) $global:LatNS"
      $global:GPSLon = "$([int]$global:LonDeg)º $([int]$global:LonMin)' $([int]$global:LonSec)$([char]34) $global:LonEW"

      Write-Host "The picture $global:FileName has the following GPS coordinates:"
      Write-Host "Latitude is $global:GPSLat"       
      Write-Host "Longitude is $global:GPSLon"

    # =============================================================================================================================================
    # Convert the latitude and longitude to numbers that Google Maps recognizes.
    # =============================================================================================================================================

      $global:LatOrt = 0
      $global:LonOrt = 0

      If ($global:LatNS -eq 'S')
       {
        $global:LatOrt = "-"   
       }
      If ($global:LonEW -eq 'W')
       {
        $global:LonOrt = "-"
       }

      $global:LatDec = ($global:LatDeg + ($global:LatMin/60) + ($global:LatSec/3600))
      $global:LonDec = ($global:LonDeg + ($global:LonMin/60) + ($global:LonSec/3600))

      $global:LatOrt = $global:LatOrt + $global:LatDec
      $global:LonOrt = $global:LonOrt + $global:LonDec

    # =============================================================================================================================================
    # The numbers that where returned contained a decimal comma instead of a decimal point.
    # So the en-US culture is forced to get the correct number notation.
    # =============================================================================================================================================
   
      $global:LatOrt = $global:LatOrt.ToString([cultureinfo]::GetCultureInfo('en-US'))
      $global:LonOrt = $global:LonOrt.ToString([cultureinfo]::GetCultureInfo('en-US'))

      Write-Host "The picture $globalFileName has the following decimal coordinates:"
      Write-Host "Latitude is $global:LatOrt"
      Write-Host "Longitude is $global:LonOrt"
    }
     else
    {
     
   # =============================================================================================================================================
   # Ohoh... No GPS information in this picture.
   # =============================================================================================================================================
     
     Write-Host "The picture $FileName does not contain GPS information."
     $global:LatOrt = "1024"
     $global:LonOrt = "1024"
    }

    
    $global:FolderLat = [math]::Round($LatOrt,2)
    $global:FolderLon = [math]::Round($LonOrt,2)

    $global:FolderLatMap = $global:FolderLat.ToString()
    $global:FolderLonMap = $global:FolderLon.ToString()

    $global:FolderName = $global:FolderLatMap + "_" + $global:FolderLOnMap

    # Write-Host "Map name $folderLat"
    # Write-Host "Map name $folderLon"
    Write-Host "Map Name is $FolderName"

    # Return $global:LatOrt,$global:LonOrt,$global:GPSLat,$global:GPSLon,$global:FolderName
  }

  Add-Type -AssemblyName System.Drawing
  $JPG = Get-ChildItem -Path "K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames\INPUT\*.JPG" -Recurse -Force | Select-Object -First 1 
  $global:LatOrt
  $global:LonOrt
  $global:GPSLat
  $global:GPSLon
  $global:FolderName
  Get-EXIFDataFromJPGFile $JPG
  $global:IntFolderLat = $global:FolderLat * 100
  $global:IntFolderLon = $global:FolderLon * 100
  $global:startLat = $global:IntFolderLat - 2
  $global:startLon = $global:IntFolderLon - 3
  $global:stopLat = $global:IntFolderLat + 2
  $global:stopLon = $global:IntFolderLon + 3
  $global:intervalintLat = $global:startLat..$global:stopLat
  $global:intervalintLon = $global:startLon..$global:stopLon
  [bool]$global:exists = $false
  foreach ($global:Lat in $global:intervalintLat){
      $global:Lat = $global:Lat / 100
      foreach ($global:Lon in $global:intervalintLon){
          
          $global:Lon = $global:Lon / 100

          $global:Latstring = $global:Lat.ToString()
          $global:Lonstring = $global:Lon.ToString()

          $global:dirname = "K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames\" + $global:Latstring + "_" + $global:Lonstring
          Write-Host $global:dirname
          Write-Host $global:exists
          If(test-path $global:dirname){
              $global:exists = $true
              $global:FolderName = $global:dirname
            }


      }
  }
  If($global:exists -eq $false){
    $global:dirname = "K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames\" + $global:FolderName
    New-Item -ItemType Directory -Force -Path $global:dirname
    
  }

#   foreach ($global:element in $globalintervalintLat){
#       $global:element=$global:element/100
#   }
#   $global:intervalLat = $globalintervalintLat / 10
  Write-Host $global:startLat, $global:stopLat
  Write-Host $global:intervalintLat
  Write-Host $global:intervalintLon
  Write-Host $global:element
  Write-Host $JPG
  $global:pathToCopyList = $JPG.ToString().Split("\")
  Write-Host $global:pathToCopyList
  $global:len = $global:pathToCopyList.Length
  Write-Host $global:len
  $global:maxindex = $global:len - 1
  $i=0
  $global:indexdirname = $global:len -2
  $global:fligthname = $global:pathToCopyList[$global:indexdirname]
  while($i -lt $global:maxindex){
      if($i -eq 0){
        $global:pathToCopy = $global:pathToCopyList[$i]
      }
      else {
        $global:pathToCopy = $global:pathToCopy + "\" + $global:pathToCopyList[$i]
      }
      $i ++
  }
  $global:pathToCopy= $global:pathToCopy + "\*"
  Write-Host $global:pathToCopy
  $global:fligthdir = $global:dirname + "\" + $global:fligthname
  New-Item -ItemType Directory -Force -Path $global:fligthdir
  $global:Photodir = $global:fligthdir + "\photos"
  New-Item -ItemType Directory -Force -Path $global:Photodir
  Copy-Item -Path $global:pathToCopy -Destination $global:Photodir -Recurse
  Remove-Item -Path $global:pathToCopy -Recurse
  pause