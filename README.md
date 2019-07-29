# Scan-to-BIM Grasshopper toolbox

This toolbox includes functions for reconstruction BIM geometry from meshes.
It features a general modular pipeline with the following steps

1. **Segmentation**: segments meshes and point clouds
2. **Classification**: labels the segments into walls, ceilings, floors, etc.
3. **Clustering**: groups labelled segments into groups of walls
4. **Reconstruction**: extracts information from grouped segments to feed BIM geometry e.g. Visual Arq Walls.
5. **Linked Building Data**: Publish the reconstructed results as .sjon/.rdf
 
The **Example** files contain example code for each step.
The **samples** contains some meshes and point clouds for each step 

Most steps are based on Matlab code. To develop the native matlab functions, use the related toolboxes (see Related Toolboxes Section)

### Contribute
There are several large files in this repository (matlab .dll's and sample files)
Use github's Large File System (lfs) to push changes to the origin.

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

Courtesy of the **KU Leuven research group in Geomatics**, TC BOUW, Department of Civil Engineering, KU Leuven, Belgium.
 *	https://iiw.kuleuven.be/onderzoek/geomatics

### Dependencies
M. Schmidt. UGM: A Matlab toolbox for probabilistic undirected graphical models. http://www.cs.ubc.ca/~schmidtm/Software/UGM.html, 2007.
Verify that version 9.4 (R2018a) of the MATLAB Runtime is installed.
You can download it at http://www.mathworks.com/products/compiler/mcr/index.html

### Related Toolboxes
S2B-Segmentation
S2B-Classification
S2B-Clustering
S2B-Grasshopper
S2B-Reconstruction

