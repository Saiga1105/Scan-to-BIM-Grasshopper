using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; // I don't really understand what the threading does?

using IronPython.Hosting; // iron python is only 2.7. won't we need python 3.8 to acess complex code? https://ironpython.net/
using Microsoft.Scripting.Hosting; // latest version is IronPython3.4 (not in nuggetpackage)

using Python.Runtime; // this should be compatible with python 3.8



namespace Scan2BIM
{
    public static class SpatialTools_Python // why does have to be a static class?
    {
        /// <summary>
        /// Run an Ironpython script without arguments. rCodeFilePath = full path + name + extension
        /// </summary>        
        public static void MyFirstIronPythonScript(string rCodeFilePath)
        {
            // so this runs an entire file instead of a function?
            ScriptEngine engine = IronPython.Hosting.Python.CreateEngine();
            engine.ExecuteFile(rCodeFilePath); // where is this local path?
        }

        /// <summary>
        /// Run an Ironpython script with arguments. rCodeFilePath = full path + name + extension
        /// </summary>
        public static void MyFirstIronPythonScript(string rCodeFilePath, List<string> argList) // can we pass something else than strings?
        {
            // step0: create the python engine
            ScriptEngine engine = IronPython.Hosting.Python.CreateEngine();

            // step1: provide a script 
            //var rCodeFilePath = @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction\scan2bim.py"; // is there a way to make this more dynamic?
            var source = engine.CreateScriptSourceFromFile(rCodeFilePath);

            // step2: provide agruments
            //var argList = new List<string>();
            //argList.Add(""); // this contains the name of the script
            //argList.Add(""); // first data. Can we pass something else than strings?
            engine.GetSysModule().SetVariable("argList", argList);

            // step2: redirect the outputs
            var eIO = engine.Runtime.IO; // do we only have todo this once?
            var errors = new MemoryStream();
            eIO.SetErrorOutput(errors, Encoding.Default);
            var results = new MemoryStream();
            eIO.SetOutput(results, Encoding.Default);

            // step3: execute the script
            var scope = engine.CreateScope(); // this is optional. what does it do? its for debugging
            source.Execute(scope);
            string str(byte[] x) => Encoding.Default.GetString(x); // this is acually a new function that we define to convert the outputs from python
            var errorsC = str(errors.ToArray());
            var resultsC = str(results.ToArray());
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
                var info = new ProcessStartInfo(@"C:\Users\u0094523\.conda\envs\scan2bim2\python.exe"); // this is python 3.8.8
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
        /// Run an Ironpython function with arguments. Unclear if this is possible!
        /// </summary>
        public static void MyFirstPythonFunction(this GH_PointCloud pc, out Boolean result)
        {
            //IDictionary<string, object> options = new Dictionary<string, object>();
            //options["Arguments"] = new[] { "C:\\Program Files (x86)\\IronPython 2.7\\Lib", "bar" }; // On windows we need double \\ for folders

            //var ipy = Python.CreateRuntime(options);
            //var script = @"K:\Projects\2025-02 Project BAEKELAND MEETHET\6.Code\Repositories\Scan2BIM\Scan2BIM-python\src\3DReconstruction\scan2bim.py";

            //dynamic Python_File = ipy.UseFile(script);

            //Python_File.MethodCall("MyFunction"); // can i access the results?

            result = false;
        }

    }
}
