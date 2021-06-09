using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Scan2BIM2
{
    public class Scan2BIM2Info : GH_AssemblyInfo
    {
        public override string Name => "Scan2BIM2";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("99DCBEF8-2DC8-476D-A33F-D5CA63847712");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}