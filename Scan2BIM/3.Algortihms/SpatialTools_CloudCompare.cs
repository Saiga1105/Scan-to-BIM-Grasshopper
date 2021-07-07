using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

using Volvox_Cloud; // volvox is not needed to run CC commands 
using Volvox_Instr;

namespace Scan2BIM
{
    public static class SpatialTools_CloudCompare // why does have to be a static class?
    {

        /// <summary>
        /// First attempt a running a python script
        /// </summary>
        public static void MyFirstCloudComparescript(this GH_PointCloud pc, out Boolean result)
        {
            // !!! we will not pursue Cloudcompare integration
            // 1. CCcommands cannot be run on rhinoclouds unless you load them from drive into cloudcompare through commandline, which is highly restrictive
            // 2. CCcommand results are on drive and thus have to be reimported which is highly inefficient
            // 3. While you can combine a number of functions within 1 command, Cloudcompare commands are actually not that extensive  
            result = false;
        }
        /// <summary>
        /// Run a 
        /// </summary>
        private static void ContactVolvox() // is this even possible?
        {
            string command = "CloudCompare -O myhugecloud.bin -SS SPATIAL 0.1";
            //run CloudCompare cmd line
        }
    }
}
