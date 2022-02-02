C:
cd "C:\Program Files\CloudCompare"
cloudcompare -silent -auto_save off -o D:\_Temp\commandlineTest\test_cloud.pcd -c_export_fmt las -octree_normals 0.03 -save_clouds file D:\_Temp\commandlineTest\test_cloud1.las -log_file D:\_Temp\commandlineTest\test_cloud1.txt
cloudcompare -silent -auto_save off -o D:\_Temp\commandlineTest\test_cloud.pcd -c_export_fmt las -octree_normals 0.05 -save_clouds file D:\_Temp\commandlineTest\test_cloud2.las -log_file D:\_Temp\commandlineTest\test_cloud2.txt
cloudcompare -silent -auto_save off -o D:\_Temp\commandlineTest\test_cloud.pcd -c_export_fmt las -octree_normals 0.07 -save_clouds file D:\_Temp\commandlineTest\test_cloud3.las -log_file D:\_Temp\commandlineTest\test_cloud3.txt