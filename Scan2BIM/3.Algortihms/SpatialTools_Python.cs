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
                @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction\", // this doesn't work
                @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction\scan2bim",// this doesn't work
                @"D:\Scan - to - BIM repository\Scan - to - BIM - Grasshopper\Scan2BIM\4.Python",// this doesn't work
                @"D:\Scan - to - BIM repository\Scan - to - BIM - Grasshopper\Scan2BIM\4.Python\scan2bim",// this doesn't work
                @"C:\Program Files(x86)\Microsoft Visual Studio\Shared\Python37_64\Scripts", // this is just a test
                Path.Combine(pathToPython, "Lib"), // custom module should be copied here
                Path.Combine(pathToPython, "DLLs")
            };

            string paths = string.Join(";", lib);
            Environment.SetEnvironmentVariable("PYTHONPATH", paths, EnvironmentVariableTarget.Process);

            using (Py.GIL()) // Global Interpreter Lock: you have to contain all your statements in this Py.GIL to manage threads
            {
                // this is unneccesary but good practice to test whether all modules can be accessed
                dynamic np = Py.Import("numpy");
                dynamic o3d = Py.Import("open3d");
                dynamic cv2 = Py.Import("cv2"); // pip install opencv-contrib-python
                dynamic plt = Py.Import("matplotlib");
                dynamic torch = Py.Import("torch");
                dynamic tf = Py.Import("tensorflow");

                // keep searching for that path
                // can we put environment on the server?
                // can we put a build event on scan2bim that it copies to Lib?
                                

            }
        }

        /// <summary>
        /// Initialise a python converter dictionary according to needs
        /// </summary>
        public static Python.Runtime.PyConverter NewDictConverter()
        {
            using (Py.GIL())
            {
                var converter = new Python.Runtime.PyConverter();
                converter.AddListType();
                converter.Add(new StringType());
                converter.Add(new Int64Type());
                converter.Add(new Int32Type());
                converter.Add(new FloatType());
                converter.Add(new DoubleType());
                converter.Add(new PyListType<int>(converter));
                converter.Add(new Point3dType());
                converter.AddDictType<string, object>();
                // this can be expanded with custom types
                return converter;
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
        /// Run an Python function with single arguments. 
        /// </summary>
        public static void MyFirstPythonFunction(this GH_PointCloud pc, out Boolean results) // what is a scope
        {
            PythonInitialize3();

            using (Py.GIL()) // Global Interpreter Lock: you have to contain all your statements in this Py.GIL to manage threads
            {                                
                // import module
                dynamic s2b = Py.Import("scan2bim");  // it works if script is in Lib!!!!!!

                // run function without arguments
                var pyObject = s2b.My1stFunction();

                // convert results
                var converterclass = new PyConverter();
                var converter = converterclass.NewConverter(); // this currently has 4 converters. can we write more?

                var test1 = (int)pyObject; // input type has to be known                             

                var pyObject2 = s2b.My2ndFunction(3);
                var pyObject3 = s2b.My2ndFunction("test");
                var pyObject4 = s2b.My2ndFunction(3.0);
                var pyObject5 = s2b.My2ndFunction(false);
               
                var test2 = converter.Convert(pyObject2);   // this works! 
                var test3 = converter.Convert(pyObject3);   // this works! 
                var test4 = converter.Convert(pyObject4);   // this works! 
                var test5 = converter.Convert(pyObject5);   // this works! 

                // run function with multiple outputs (as Tuple)
                var pyObject6 = s2b.MyTupleFunction(3, false);
                var test6a = converter.Convert(pyObject6[0]);
                var test6b = converter.Convert(pyObject6[1]);

                //run function with array
                double[] b = { 1.0, 3.0, 5.0, 7.0, 9.0 };
                var pyObject8 = s2b.My2ndFunction((dynamic)b); 
                var test7 = converter.Convert(pyObject8[0]);   
                var test8 = converter.Convert(pyObject8[1]);   // this works! 

                //run function with Point
                //var p1 = pc.Value.AsReadOnlyListOfPoints().ElementAt(0);
                //var pyObject9 = s2b.My2ndFunction((dynamic)p1); 
                //var test9 = converter.Convert(pyObject9);  
                

                if (pyObject != null) results = true;
                else throw new Exception("something pythonish broke");
            }
        }
        /// <summary>
        /// Run an Python function with single arguments. 
        /// </summary>
        public static void MySecondPythonFunction(this GH_PointCloud pc, out Boolean results) // what is a scope
        {
            PythonInitialize3();

            using (Py.GIL()) 
            {
                // import module
                dynamic s2b = Py.Import("scan2bim");
                dynamic np = Py.Import("numpy");

                //create a instance of PyConverter                
                var converter = NewDictConverter();

                //CLR types
                int a = 0;
                double b = 1.0;
                string c = "test";
                bool d = true;
                int[] aa = { 0, 2, 4 };
                List<int> aaa = aa.ToList();

                // convert types => basic types normally can also be directly cast to dynamic!
                var pa= converter.ToPython(a); //PyInt
                var pb = converter.ToPython(b); //PyFloat
                var pcc = converter.ToPython(c); //PyString
                var pd = (dynamic)d; 
                var paaa = converter.ToPython(aaa); 
                var paaaa = (dynamic)aaa;
                var pyObject1 = s2b.My2ndFunction(paaa); // both now seem to work?
                var pyObject2 = s2b.My2ndFunction(paaaa); //
                var test1a = converter.ToClr(pyObject1[0]);
                var test1b = converter.ToClr(pyObject1[1]);
                // it seems that converter.cs and converters.cs offer the same functionality

                //run function with Point
                var p1 = pc.Value.AsReadOnlyListOfPoints().ElementAt(0);
                var pyObject9 = s2b.My2ndFunction((dynamic)p1);
                var test9 = converter.ToClr(pyObject9);

                results = true;
            }
        }

        /// <summary>
        /// Run an Python function with arguments. 
        /// </summary>
        public static void MyThirdPythonFunction(this GH_PointCloud pc, out Boolean results)
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
