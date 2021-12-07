"""
Panopy - a Python library for handling 360° panoramic images.

Orientation is given as a quaternion in scalar-last format [x, y, z, w]. For an
equirectangular pano format this is the orientation of the center of the pano.
The coordinate system is right-handed with z as the vertical direction.
"""


from datetime import datetime
import xml.etree.ElementTree as ET

import matplotlib.pyplot as plt
import numpy as np
from scipy.spatial.transform import Rotation
from mpl_toolkits.mplot3d import Axes3D
from PIL import Image
import cv2

class AlignmentPose():
    """
    An alignment pose is used to transform a collection of pano poses.
    """

    def __init__(self, x, y, z, orientation, name=None, validate=True):
        """
        """
        if validate is True:
            assert(isinstance(orientation, tuple) and len(orientation) == 4)

        self.x = x
        self.y = y
        self.z = z
        self.orientation = orientation
        self.name = name


class PanoPose(AlignmentPose):
    """
    A pano pose gives the position, orientation and optionally time and name of
    a pano.
    """

    def __init__(self, x, y, z, orientation, time=None, name=None, validate=True):
        """
        """
        if validate is True:
            if time is not None:
                assert(isinstance(time, datetime))

        super().__init__(x, y, z, orientation, name)

        self.time = time

    @property
    def heading(self):
        """
        Heading measured as angle from x to y axis. In equirectangular format
        this is the center of the pano. Headings are always positive to
        simplify subsequent calculations.

        See 'https://docs.scipy.org/doc/scipy/reference/generated/scipy.spatial.transform.Rotation.html'
        """
        heading = Rotation.from_quat(
                self.orientation).as_euler('xyz', degrees=True)[-1]

        if heading < 0:
            heading = 360 + heading

        return heading

    @property
    def zenit(self):
        """
        Angle with the vertical
        """
        zenit = Rotation.from_quat(
                self.orientation).as_euler('xyz', degrees=True)[0]

        return zenit


    def get_direction_to_other_pose(self, other_pose, validate=True):
        """
        Angle from x to y with 'self' in the origin. Angles are always positive
        to simplify subsequent calculations.

        other_pose: instance of 'self'
        """
        if validate is True:
            assert(isinstance(self, self.__class__))

        x = other_pose.x - self.x
        y = other_pose.y - self.y
        angle = np.degrees(np.arctan2(y, x))

        if angle < 0:
            angle = 360 + angle

        return angle


class PanoPoseCollection():
    """
    A collection of pano poses with position, orientation and time.
    """

    def __init__(self, pos_xs, pos_ys, pos_zs, ori_xs, ori_ys, ori_zs,
        ori_ws, times=None, validate=True):
        """
        """
        if validate is True:
            assert(len(pos_xs) == len(pos_ys) == len(pos_zs) == len(ori_xs) ==
                len(ori_ys) == len(ori_zs) == len(ori_ws))
            if times is not None:
                assert(len(times) == len(ori_ws))

        self.pos_xs = pos_xs
        self.pos_ys = pos_ys
        self.pos_zs = pos_zs
        self.ori_xs = ori_xs
        self.ori_ys = ori_ys
        self.ori_zs = ori_zs
        self.ori_ws = ori_ws
        self.times = times

    def __len__(self):
        """
        """
        return len(self.ori_ws)

    def __getitem__(self, i):
        """
        """
        pano_pose = PanoPose(self.pos_xs[i], self.pos_ys[i], self.pos_zs[i],
            (self.ori_xs[i], self.ori_ys[i], self.ori_zs[i], self.ori_ws[i]),
            self.times[i], name=str(i), validate=False)

        return pano_pose

    def __iter__(self):
        """
        """
        for i in range(len(self)):
            yield self[i]

    @property
    def headings(self):
        """
        """
        return [p.heading for p in self]

    @property
    def zenits(self):
        """
        """
        return [p.zenit for p in self]

    @property
    def box(self):
        """
        Give bounding box of pano collection.
        """
        x_min, x_max = self.pos_xs.min(), self.pos_xs.max()
        y_min, y_max = self.pos_ys.min(), self.pos_ys.max()

        return x_min, x_max, y_min, y_max

    def transform(self, aligment_pose, validate=True):
        """
        """
        if validate is True:
            assert(isinstance(aligment_pose, AlignmentPose))

        r = Rotation.from_quat(aligment_pose.orientation)
        coordinates = np.matmul(
            r.as_matrix(), np.array([self.pos_xs, self.pos_ys, self.pos_zs]))
        pos_xs = coordinates[0] + aligment_pose.x
        pos_ys = coordinates[1] + aligment_pose.y
        pos_zs = coordinates[2] + aligment_pose.z

        # TODO: Optimise this part so operation is performed at once
        ori_xs, ori_ys, ori_zs, ori_ws = [], [], [], []
        for p in self:
            pr = r * Rotation.from_quat(p.orientation)
        
            ori_x, ori_y, ori_z, ori_w = pr.as_quat()
            ori_xs.append(ori_x)
            ori_ys.append(ori_y)
            ori_zs.append(ori_z)
            ori_ws.append(ori_w)

        ppc = PanoPoseCollection(
            pos_xs, pos_ys, pos_zs, ori_xs, ori_ys, ori_zs, ori_ws, self.times)

        return ppc

    def plot(self, headings=False, size=None):
        """
        headings: boolean (default: False) - plots headings as vectors
        with size 1.
        """
        plot_pose_collections_3D(
            [self], colors=['k'], headings=headings, size=size)


def read_leica_pano_poses_xml(filespec):
    """
    Read xml file from .e57 exported with Leica Cyclone.
    """
    root = ET.parse(filespec).getroot()

    ns = '{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}'

    datetimes = []
    pos_xs, pos_ys, pos_zs = [], [], []
    ori_xs, ori_ys, ori_zs = [], [], []
    ori_ws = []

    for i, pano in enumerate(root.findall('.//{}vectorChild'.format(ns))):
        datetimes.append(datetime.fromtimestamp(float(
            pano.find('{}acquisitionDateTime'.format(ns)).find(
                '{}dateTimeValue'.format(ns)).text)))

        pose = pano.find('{}pose'.format(ns))
        if pose is None and i == 0: ## no translation and rotation given for first point
            ori_x, ori_y, ori_z = 0, 0, 0
            ori_w = 1
            pos_x, pos_y, pos_z = 0, 0, 0
        else:
            rotation = pose.find('{}rotation'.format(ns))
            ori_x = rotation.find('{}x'.format(ns)).text
            ori_y = rotation.find('{}y'.format(ns)).text
            ori_z = rotation.find('{}z'.format(ns)).text
            ori_w = rotation.find('{}w'.format(ns)).text
            if ori_x is None:
                ori_x = 0
            if ori_y is None:
                ori_y = 0
            if ori_z is None:
                ori_z = 0
            if ori_w is None:
                ori_w = 1
            
            ori_x = float(ori_x)
            ori_y = float(ori_y)
            ori_z = float(ori_z)

            translation = pose.find('{}translation'.format(ns))
            pos_x = float(translation.find('{}x'.format(ns)).text)
            pos_y = float(translation.find('{}y'.format(ns)).text)
            pos_z = float(translation.find('{}z'.format(ns)).text)

        ## Correct for Leica pano's (at least export from Cyclone) pointing
        ## towards positive y axis with center. This should be the positive
        ## x-axis so have to rotate 90 degrees.
        r = (Rotation.from_quat((ori_x, ori_y, ori_z, ori_w)) *
            Rotation.from_euler('z', 90, degrees=True))
        ori_x, ori_y, ori_z, ori_w = r.as_quat()

        ori_xs.append(ori_x)
        ori_ys.append(ori_y)
        ori_zs.append(ori_z)
        ori_ws.append(ori_w)
        pos_xs.append(pos_x)
        pos_ys.append(pos_y)
        pos_zs.append(pos_z)

    ppc = PanoPoseCollection(
        np.array(pos_xs), np.array(pos_ys), np.array(pos_zs),
        np.array(ori_xs), np.array(ori_ys), np.array(ori_zs),
        np.array(ori_ws),
        np.array(datetimes), validate=False)

    return ppc


def read_navvis_pano_poses_csv(filespec):
    """
    NavVis provides a pano-poses.csv file in each post-processed dataset. It
    holds the timestamp, position and orientation of each pano in the dataset.
    """
    with open(filespec) as f:
        data = f.read().split('\n')

    n = len(data) - 2

    datetimes = []
    pos_xs, pos_ys, pos_zs = np.zeros(n), np.zeros(n), np.zeros(n)
    ori_xs, ori_ys, ori_zs = np.zeros(n), np.zeros(n), np.zeros(n)
    ori_ws = np.zeros(n)
    for i, l in enumerate(data[1:-1]):
        r = l.split('; ')
        datetimes.append(datetime.fromtimestamp(float(r[2])))
        pos_xs[i] = r[3]
        pos_ys[i] = r[4]
        pos_zs[i] = r[5]
        ori_xs[i] = r[7]
        ori_ys[i] = r[8]
        ori_zs[i] = r[9]
        ori_ws[i] = r[6]

    ppc = PanoPoseCollection(pos_xs, pos_ys, pos_zs, ori_xs, ori_ys, ori_zs,
        ori_ws, np.array(datetimes), validate=False)

    return ppc


def read_navvis_alignment_xml(filespec):
    """
    Read xml file generated by NavVis aligment tool.
    """
    root = ET.parse(filespec).getroot()

    alignment_poses = []
    for dataset in root.findall('.//dataset'):
        name = dataset.find('name').text
        position = dataset.find('.//position')
        pos_x = float(position.find('x').text)
        pos_y = float(position.find('y').text)
        pos_z = float(position.find('z').text)
        orientation = dataset.find('.//orientation')
        ori_x = float(orientation.find('x').text)
        ori_y = float(orientation.find('y').text)
        ori_z = float(orientation.find('z').text)
        ori_w = float(orientation.find('w').text)

        alignment_poses.append(AlignmentPose(
            pos_x, pos_y, pos_z, (ori_x, ori_y, ori_z, ori_w), name))

    return alignment_poses


def plot_pose_collections(ppcs, colors=None, headings=False, size=None):
    """
    ppcs: list of PanoPoseCollection
    headings: boolean (default: False) - plots headings as vectors
    with size 1.
    """
    _, ax = plt.subplots(figsize=size)

    for i, ppc in enumerate(ppcs):
        kwargs = {}
        if colors is not None:
            kwargs['c'] = colors[i]
        ax.scatter(ppc.pos_xs, ppc.pos_ys, **kwargs)

        if headings is True:
            pc_headings = ppc.headings
            xs = np.cos(np.radians(pc_headings))
            ys = np.sin(np.radians(pc_headings))
            ax.quiver(ppc.pos_xs, ppc.pos_ys, xs, ys, color='k')

    ax.axis('equal')

    plt.show()

def plot_pose_collections_3D(ppcs, colors=None, headings=False, size=None):
    """
    ppcs: list of PanoPoseCollection
    headings: boolean (default: False) - plots headings as vectors
    with size 1.
    """
    fig = plt.figure()
    ax = fig.add_subplot(projection='3d')

    for i, ppc in enumerate(ppcs):
        kwargs = {}
        if colors is not None:
            kwargs['c'] = colors[i]
        ax.scatter(ppc.pos_xs, ppc.pos_ys, ppc.pos_zs, **kwargs)

        if headings is True:
            pc_headings = ppc.headings
            pc_zenits = ppc.zenits
            xs = np.cos(np.radians(pc_headings))
            ys = np.sin(np.radians(pc_headings))
            zs = np.sin(np.radians(pc_zenits))

            ax.quiver(ppc.pos_xs, ppc.pos_ys,ppc.pos_zs, xs, ys, zs,color='k')

    ax.axis('auto')

    plt.show()
def decode_depthmap(source, resize = True, size = (8192,4096), show = False):
    """
    Function to decode the depthmaps generated by the navvis processing
    source: Location of the PNG files containing the depthmap
    resize(bool): If the resulting dethmap needs to be resized to match the size of the corresponding pano, by default True
    size: size of the corresponding pano, by default 8192x4096
    show: if true the result wil be shown, by default False
    """
    depthmap = np.asarray(Image.open(source)).astype(float)
    converted_depthmap = np.empty([np.shape(depthmap)[0], np.shape(depthmap)[1]])
    r = 0
    while r < np.shape(depthmap)[0]:
        c = 0
        while c < np.shape(depthmap)[1]:
            value = depthmap[r,c]
            depth_value = value[0] / 256 * 256 + value[1] / 256 * 256 * 256 + value[2] / 256 * 256 * 256 * 256 + value[3] / 256 * 256 * 256 * 256 * 256
            converted_depthmap[r,c] = depth_value
            c = c + 1
        r = r + 1
    if resize:
        resized_depthmap = cv2.resize(converted_depthmap,size)
        if show:
            plt.imshow(resized_depthmap, cmap="plasma")
            plt.show()
        return resized_depthmap
    else:
        if show:
            plt.imshow(converted_depthmap, cmap="plasma")
            plt.show()
        return converted_depthmap

