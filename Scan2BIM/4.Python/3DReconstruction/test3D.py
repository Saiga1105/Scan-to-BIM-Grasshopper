import os
from panopy import read_navvis_alignment_xml, read_navvis_pano_poses_csv, plot_pose_collections_3D

filespec = r'K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\Samples\PanoPositioning\alignment.xml'

alignment_poses = read_navvis_alignment_xml(filespec)
# print(alignment_poses)

datasets = [
    'pano-poses - dataset1.csv',
    'pano-poses - dataset2.csv',
    'pano-poses - dataset3.csv'
]

ppcs = [read_navvis_pano_poses_csv(os.path.join(r'K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\Samples\PanoPositioning', dataset)) for dataset in datasets]
transformed_ppcs = []
for ppc, ap in zip(ppcs, alignment_poses):
    transformed_ppcs.append(ppc.transform(ap))

plot_pose_collections_3D(transformed_ppcs, size=(16, 10), headings= True)

