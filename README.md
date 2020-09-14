# Scan-to-BIM Grasshopper toolbox

![Overview](Pics/Overview.png)

This toolbox includes functions for the reconstruction of BIM geometry from meshes.
It features a general modular pipeline with the following steps

0. **Utility**: Some utility functions for mesh and point cloud mutation.
1. **Segmentation**: segments meshes and point clouds
2. **Classification**: labels the segments into walls, ceilings, floors, etc.
3. **Clustering**: groups labelled segments into groups of walls
4. **Reconstruction**: extracts information from grouped segments to feed BIM geometry e.g. https://www.visualarq.com/ walls.
5. **Linked Building Data**: Publish the intermediate results as .sjon/.rdf
 
The **Example** files contain example code for each step.  
The **Samples** contains some meshes and point clouds for each step.

Step 1-3 are based on Matlab code. To develop the native matlab functions, use the related toolboxes (see Related Toolboxes Section)

### Install & Use

* Install Vstudio
* Install .NET framework 4.6.1
* Install GIT
* Install LFS GIT
* Clone all repositories (LFS does not support regular download)
* Install Volvox 
* Install EleFront and MeshEdit (put it in C:\Users\...\AppData\Roaming\Grasshopper\Libraries)
* install matlab runtime 9.4.1 (2018a)
* Open project solution
* Add reference paths (properties of C# project Scan2BIM)
* build solution
* Right-click the .gha and .dll files > Properties > make sure there is no "blocked" text 
* start Rhino
* enter commandline GrassHopperDevelopersettings and add \bin folder (where you just built the plugin)
* Restart Rhino and Grasshopper

* open a model containing several meshes (e.g. Samples\1.Mechelen_castle\)
* open a grasshopper canvas (e.g. Examples\Classification\M_classification_machinelearning.gh)
* reference the meshes in the geometry on the left
* bake the classified geometry on the right

### Contribute
There are several large files in this repository (matlab .dll's and sample files)
Use github's Large File System (LFS) to push changes to the origin.

* make sure the .dll files are tracked after commiting
	* git lfs track '*.dll'
	* git lfs track '*.mat'
	* git lfs track '*.3dm'
	* git lfs track '*.obj'
	* git lfs track '*.stl'
	* git lfs track '*.json'
	* git lfs track '*.rdf'
	
### License 
If you use this software in a publication, please cite the work using the following information:

Bassier M., Vergauwen M. (2019) Clustering of Wall Geometry from Unstructured Point Clouds Using Conditional Random Fields. 
Remote Sensing, 11(13), 1586; https://doi.org/10.3390/rs11131586

Courtesy of the **KU Leuven research group in Geomatics**, TC BOUW, Department of Civil Engineering, KU Leuven, Belgium. https://iiw.kuleuven.be/onderzoek/geomatics

Do not use for commercial purposes.

### Dependencies
* GIT LFS https://git-lfs.github.com/
* M. Schmidt. UGM: A Matlab toolbox for probabilistic undirected graphical models. http://www.cs.ubc.ca/~schmidtm/Software/UGM.html, 2007.
* MATLAB Runtime version 9.4 (R2018a). You can download it at http://www.mathworks.com/products/compiler/mcr/index.html
* Volvox https://www.food4rhino.com/app/volvox , DURAARK http://duraark.eu/ a European project
* RhinoInside https://www.rhino3d.com/inside
* Rhinocommon https://developer.rhino3d.com/api/RhinoCommon/html/R_Project_RhinoCommon.htm
* .NET framework 4.6.1 https://dotnet.microsoft.com/download/dotnet-framework/net461
* EleFront grasshopper plugin
* MeshEdit Components grasshopper plugin

### Related Toolboxes
The grashopper plug in consumes following Open Source Toolboxes from the same author.

* S2B-Segmentation  
* S2B-Classification  
* S2B-Clustering  
* S2B-Reconstruction  

