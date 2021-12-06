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
    public class PyConverter
    {
        /// <summary>
        /// Convert PyObjects to C# classes  
        /// </summary>

        private Dictionary<IntPtr, Func<PyObject, object>> Converters; // what is a dictionary?


        #region "Constructors"
        public PyConverter()
        {
            this.Converters = new Dictionary<IntPtr, Func<PyObject, object>>();
        }

        public PyConverter NewConverter()
        {
            var converter = new PyConverter();
            using (Py.GIL())
            {
                //XIncref is needed, or keep the PyObject
                converter.Add(PythonEngine.Eval("int").Handle, (args) => { return args.As<int>(); });
                converter.Add(PythonEngine.Eval("str").Handle, (args) => { return args.As<string>(); });
                converter.Add(PythonEngine.Eval("float").Handle, (args) => { return args.As<double>(); });
                converter.Add(PythonEngine.Eval("bool").Handle, (args) => { return args.As<bool>(); });

                //converter.Add(PythonEngine.Eval("int[]").Handle, (args) => { return args.As<int[]>(); }); //Test
                //converter.Add(PythonEngine.Eval("System.Int32[]").Handle, (args) => { return args.As<int[]>(); });
                //converter.Add(PythonEngine.Eval("double[]").Handle, (args) => { return args.As<double[]>(); });
                //Converter for list, omit here
            }
            return converter;
        }
        #endregion

        #region "Methods"

        public void Add(IntPtr type, Func<PyObject, object> func)
        {
            this.Converters.Add(type, func);
        }

        public object Convert(PyObject obj)
        {
            if (obj == null)
            {
                return null;
            }
            PyObject type = obj.GetPythonType();
            Func<PyObject, object> func;
            var state = Converters.TryGetValue(type.Handle, out func);
            if (!state)
            {
                throw new Exception($"Type {type.ToString()} not recognized");
            }
            return func(obj);
        }
        #endregion

    }
}
