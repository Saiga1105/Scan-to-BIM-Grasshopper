"""
scan2bim - a Python library for BIM and point cloud data.

Orientation is given as a quaternion in scalar-last format [x, y, z, w]. For an
equirectangular pano format this is the orientation of the center of the pano.
The coordinate system is right-handed with z as the vertical direction.
"""

import matplotlib.pyplot as plt
import numpy as np
import cv2
import open3d as o3d
import json
import os
import pye57

def sample_function(arg1):
    """Explenation

    Args:
        arg1: format e.g. [3x1] (float)
            Explenation
    Returns:
        A numpy array representing a 360 degree panoramic image of the point
        cloud.
    """
    #code
    return arg1

def point_cloud_to_panorama_dmap(pcd,
                            v_res=0.42,
                            h_res = 0.35,
                            v_fov = (-24.9, 2.0),
                            d_range = (0,100),
                            y_fudge=3
                            ):
    """ Takes point cloud data as input and creates a 360 degree panoramic
        image, returned as a numpy array.

    Args:
        points: (np array)
            The numpy array containing the point cloud. .
            The shape should be at least Nx3 (allowing for more columns)
            - Where N is the number of points, and
            - each point is specified by at least 3 values (x, y, z)
        v_res: (float)
            vertical angular resolution in degrees. This will influence the
            height of the output image.
        h_res: (float)
            horizontal angular resolution in degrees. This will influence
            the width of the output image.
        v_fov: (tuple of two floats)
            Field of view in degrees (-min_negative_angle, max_positive_angle)
        d_range: (tuple of two floats) (default = (0,100))
            Used for clipping distance values to be within a min and max range.
        y_fudge: (float)
            A hacky fudge factor to use if the theoretical calculations of
            vertical image height do not match the actual data.
    Returns:
        A numpy array representing a 360 degree panoramic image of the point
        cloud.
    """
    # Projecting to 2D
    # x_points = points[:, 0]
    # y_points = points[:, 1]
    # z_points = points[:, 2]
    # r_points = points[:, 3]
    # d_points = np.sqrt(x_points ** 2 + y_points ** 2)  # map distance relative to origin
    # #d_points = np.sqrt(x_points**2 + y_points**2 + z_points**2) # abs distance

    # # We use map distance, because otherwise it would not project onto a cylinder,
    # # instead, it would map onto a segment of slice of a sphere.

    # # RESOLUTION AND FIELD OF VIEW SETTINGS
    # v_fov_total = -v_fov[0] + v_fov[1]

    # # CONVERT TO RADIANS
    # v_res_rad = v_res * (np.pi / 180)
    # h_res_rad = h_res * (np.pi / 180)

    # # MAPPING TO CYLINDER
    # x_img = np.arctan2(y_points, x_points) / h_res_rad
    # y_img = -(np.arctan2(z_points, d_points) / v_res_rad)

    # # THEORETICAL MAX HEIGHT FOR IMAGE
    # d_plane = (v_fov_total/v_res) / (v_fov_total* (np.pi / 180))
    # h_below = d_plane * np.tan(-v_fov[0]* (np.pi / 180))
    # h_above = d_plane * np.tan(v_fov[1] * (np.pi / 180))
    # y_max = int(np.ceil(h_below+h_above + y_fudge))

    # # SHIFT COORDINATES TO MAKE 0,0 THE MINIMUM
    # x_min = -360.0 / h_res / 2
    # x_img = np.trunc(-x_img - x_min).astype(np.int32)
    # x_max = int(np.ceil(360.0 / h_res))

    # y_min = -((v_fov[1] / v_res) + y_fudge)
    # y_img = np.trunc(y_img - y_min).astype(np.int32)

    # # CLIP DISTANCES
    # d_points = np.clip(d_points, a_min=d_range[0], a_max=d_range[1])

    # # CONVERT TO IMAGE ARRAY
    # img = np.zeros([y_max + 1, x_max + 1], dtype=np.uint8)
    # img[y_img, x_img] = scale_to_255(d_points, min=d_range[0], max=d_range[1])

    return 1

def point_cloud_select_box(pcd,
                        origin,
                        orientation,
                        dx,
                        dy,
                        dz):
    """Subselect a box volume from a Open3D point cloud

    Args:
        pcd: (Open3D PointCloud)
        origin: [3x1] (float)
            centre of the box 
        orientation: [4x1] (float)
            rotation of the box around Z
        dx: (float)
            range in X
        dy: (float)
            range in Y
        dz: (float)
            range in Z

    Returns:
        A numpy array representing a 360 degree panoramic image of the point
        cloud.
    """
    # define polygon (by the clock)
    bounding_polygon = np.array([ 
            [ origin[0]-dx, origin[1]-dy, 0],
            [ origin[0]+dx, origin[1]-dy, 0],
            [ origin[0]+dx, origin[1]+dy, 0],
            [ origin[0]-dx, origin[1]+dy, 0]]).astype("float64")
    # rotate (optional)
    # r = rot.from_quat(orientation)
    # T=r.apply(bounding_polygon)
    # define selection volume (2D polygon + extrusion)
    vol = o3d.visualization.SelectionPolygonVolume()
    vol.bounding_polygon = o3d.utility.Vector3dVector(bounding_polygon)
    vol.orthogonal_axis = "Z"
    vol.axis_max = origin[2]+dz
    vol.axis_min = origin[2]-dz
    # crop pcd
    pcd_cropped= vol.crop_point_cloud(pcd)
    return pcd_cropped


def e57_to_dmap(pcd,
                v_res,
                h_res,
                ):
    """ Takes point cloud data as input and creates a 360 degree panoramic
        image, returned as a numpy array.

    Args:

    """
    # load e57
    return 1

def MyFunction(result,
                ):
    """ This is the first attempt of calling a python function from C#

    Args:

    """
    # here goes the code
    result = 5
    return result

def MyTupleFunction(input1,
                    input2
                ):
    """ This is the first attempt of calling a python function from C#

    Args:

    """
    # here goes the code
    input1=5
    input2=10
    return input1,input2


    