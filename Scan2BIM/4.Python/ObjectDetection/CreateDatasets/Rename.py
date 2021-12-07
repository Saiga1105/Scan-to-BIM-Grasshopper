
# Pythono3 code to rename multiple 
# files in a directory or folder
  
# importing os module
import os
import time
from shutil import copyfile

directory = r"C:\Data\ObjectDetectionLibrary\Panos"
srcdir = r"C:\Users\SamDeGeyter\Data\Mapping\VLX\datasets_proc\2021-02-04_13.46.28\pano"
# Function to rename multiple files
def main():
    trial = 0
    count = 0

    for direc in os.listdir(directory):
        for f in os.listdir(os.path.join(directory,direc)):
            if f.endswith(".jpg"):
                filepath = os.path.join(directory,direc,f)
                target = "pic" + str(count) + ".jpg"
                targetpath = os.path.join(directory, target)
                try:
                    copyfile(filepath, targetpath)
                    count = count + 1
                except IOError as e:
                    print("Unable to copy file. %s" % e)
                    exit(1)
                except:
                    print("Unexpected error:", sys.exc_info())
                    exit(1)
            

print("\nFile copy done!\n")

    # for count, filename in enumerate(os.listdir(directory)):
    #     dst ="0000"+ str(trial) +str(count) + ".jpg"
    #     src = os.path.join(directory, filename)
    #     dst = os.path.join(directory, dst)
    #     while os.path.exists(dst):
    #         trial = trial + 1
    #         dst ="0000"+ str(trial) +str(count) + ".jpg"
    #         dst = os.path.join(directory, dst)
           
    #     else:
    #         # rename() function will
    #         # rename all the files
    #         os.rename(src, dst)
            
  
# Driver Code
if __name__ == '__main__':
      
    # Calling main() function
    main()