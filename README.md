# Scan-to-BIM Grasshopper toolbox

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

### Install
In Grasshopper, choose File > Special Folders > Components folder. Save all files from \bin there including **Scan2BIM.gha** and it's .dll's.  
Right-click the file > Properties > make sure there is no "blocked" text  
Restart Rhino and Grasshopper

### Contribute
There are several large files in this repository (matlab .dll's and sample files)
Use github's Large File System (lfs) to push changes to the origin.

make sure the .dll files are tracked after commiting
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

### Dependencies
* M. Schmidt. UGM: A Matlab toolbox for probabilistic undirected graphical models. http://www.cs.ubc.ca/~schmidtm/Software/UGM.html, 2007.
* MATLAB Runtime version 9.4 (R2018a). You can download it at http://www.mathworks.com/products/compiler/mcr/index.html
* Volvox https://www.food4rhino.com/app/volvox , DURAARK http://duraark.eu/ a European project
* RhinoInside https://www.rhino3d.com/inside
* Rhinocommon https://developer.rhino3d.com/api/RhinoCommon/html/R_Project_RhinoCommon.htm
* .NET framework 4.6.1 https://dotnet.microsoft.com/download/dotnet-framework/net461


### Related Toolboxes
The grashopper plug in consumes following Open Source Toolboxes from the same author.

S2B-Segmentation  
S2B-Classification  
S2B-Clustering  
S2B-Reconstruction  

