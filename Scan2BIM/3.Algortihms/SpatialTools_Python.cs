using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; // I don't really understand what the threading does?

using Rhino.Geometry; // https://developer.rhino3d.com/api/
using Rhino.DocObjects.Custom;
using Rhino.Display;

using Grasshopper; // https://developer.rhino3d.com/api/
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Python.Runtime; // this should be compatible with python 3.7 http://pythonnet.github.io/



namespace Scan2BIM
{
    public static class SpatialTools_Python // why does have to be a static class?
    {
        /// <summary>
        /// Initialize python env 
        /// </summary>
        public static void PythonInitialize() // maybe pass env?
        {
            string pathToVirtualEnv = @"C:\Users\u0094523\.conda\envs\py37";
            string pathToMyCode = @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction";//\scan2bim.py

            Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);
            
            // path was succesfully added
            Environment.SetEnvironmentVariable("PYTHONPATH", pathToMyCode, EnvironmentVariableTarget.Process);
           
            PythonEngine.PythonHome = pathToVirtualEnv; // this was fixed by Py3.7 env
            PythonEngine.PythonPath = PythonEngine.PythonPath + ";" + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// Initialize python env 
        /// </summary>
        public static void PythonInitialize2() // maybe pass env?
        {
            // Setup path to python environment. Be carefull for the bitstance of the environment (currently x64)
            string pathToPython = @"C:\Users\u0094523\.conda\envs\py37";
            string path = pathToPython + ";" +
                          Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToPython, EnvironmentVariableTarget.Process);

            // Setup all paths to modules
            var lib = new[]
            {
                // this first path doesn't work
                @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction\scan2bim",
                Path.Combine(pathToPython, "Lib"), // custom modules should be here
                Path.Combine(pathToPython, "DLLs")
            };
            string paths = string.Join(";", lib);
            Environment.SetEnvironmentVariable("PYTHONPATH", paths, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// Initialize python env 
        /// </summary>
        public static void PythonInitialize3() // maybe pass env?
        {
            // Setup path to python environment. Be carefull for the bitstance of the environment (currently x64)
            string pathToPython = @"C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64";
            string path = pathToPython + ";" +
                          Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToPython, EnvironmentVariableTarget.Process);

            // Setup all paths to modules
            var lib = new[]
            {
                @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction\",
                @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction\scan2bim",
                @"D:\Scan - to - BIM repository\Scan - to - BIM - Grasshopper\Scan2BIM\4.Python",
                @"D:\Scan - to - BIM repository\Scan - to - BIM - Grasshopper\Scan2BIM\4.Python\scan2bim",
                @"C:\Program Files(x86)\Microsoft Visual Studio\Shared\Python37_64\Scripts", // this is just a test
                Path.Combine(pathToPython, "Lib"),
                Path.Combine(pathToPython, "DLLs")
            };

            string paths = string.Join(";", lib);
            Environment.SetEnvironmentVariable("PYTHONPATH", paths, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// Convert C# classes to Python objects
        /// </summary>
        public static void ToPyObject(this GH_PointCloud pc)     
        {
            using (Py.GIL()) // you have to contain all your statements in this Py.GIL to manage threads
            {
                // create a Python scope (what is a scope?)
                using (PyScope scope = Py.CreateScope())
                {
                    //// convert the Person object to a PyObject
                    //PyObject pyPerson = Point3d.ToPython();

                    //// create a Python variable "person"
                    //scope.Set("person", pyPerson);

                    //// the person object may now be used in Python
                    //string code = "fullName = person.FirstName + ' ' + person.LastName";
                    //scope.Exec(code);
                }
            }
        }             

        /// <summary>
        /// Run a python 3.8 script with arguments. rCodeFilePath = full path + name + extension. args separated by spaces
        /// </summary>
        public static string MyFirstPython38ScriptFile(string rCodeFilePath, string args) 
        {
            //example rCodeFilePath = @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\scan2bim.py";
            string file = rCodeFilePath;
            string result = string.Empty;
            try
            {
                var info = new ProcessStartInfo(@"C:\Users\u0094523\.conda\envs\py37\python.exe"); // this is python 3.8.8
                info.Arguments = rCodeFilePath + " " + args;
                info.RedirectStandardInput = false;
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;

                using (var proc = new Process())
                {
                    proc.StartInfo = info;
                    proc.Start();
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        result = proc.StandardOutput.ReadToEnd();
                    }
                }
                return result;
            }
            catch (Exception ex) { throw new Exception("R Script failed: " + result, ex); }           
        }

        /// <summary>
        /// Run an Python function without arguments. 
        /// </summary>
        public static void MyFirstPythonFunction(this GH_PointCloud pc, out Boolean results)
        {

            //PythonInitialize();
            //PythonInitialize2();
            PythonInitialize3();

            using (Py.GIL()) // Global Interpreter Lock: you have to contain all your statements in this Py.GIL to manage threads
            {
                // this is unneccesary but good practice to test whether all modules can be accessed
                dynamic np = Py.Import("numpy");   
                dynamic o3d = Py.Import("open3d");
                dynamic cv2 = Py.Import("cv2"); // pip install opencv-contrib-python
                dynamic plt = Py.Import("matplotlib");  
                dynamic torch = Py.Import("torch"); 

                // can we test going inside repo's?
                // test with tensorflow?
                // keep searching for that path
                // can we put environment on the server?
                // can we put a build event on scan2bim that it copies to Lib?

                //dynamic nn = Py.Import("torch.nn");  
                //dynamic F = Py.Import("torch.nn.functional");  
                //dynamic optim = Py.Import("torch.optim"); 

                // it works if script is in Lib!!!!!!
                dynamic s2b = Py.Import("scan2bim");  

                var result = s2b.My1stFunction();

                // we still have to convert the result to a python object

                if (result != null) results = true;
                else throw new Exception("something 1st pythonish broke");
            }
        }

        /// <summary>
        /// Run an Python function with arguments. 
        /// </summary>
        public static void MySecondPythonFunction(this GH_PointCloud pc, out Boolean results)
        {
            PythonInitialize();

            using (Py.GIL()) // Global Interpreter Lock: you have to contain all your statements in this Py.GIL to manage threads
            {
                // shouldn't we use PyOBject rather than dynamic?
                dynamic np = Py.Import("numpy");
                dynamic s2b = PythonEngine.ImportModule(@"K:\\Projects\\2025-02 Project BAEKELAND MEETHET\\6.Code\\Repositories\\Scan2BIM\\Scan2BIM-python\\src\\3DReconstruction\\scan2bim.py");

                var result = s2b.My2ndFunction(5.0);

                if (result != null) results = true;
                else throw new Exception("something 2nd pythonish broke");
            }
        }

    }
}
