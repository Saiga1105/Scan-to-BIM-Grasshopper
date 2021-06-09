using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Scan2BIM_WIP
{
    public class Scan2BIM_WIPInfo : GH_AssemblyInfo
    {
        public override string Name => "Saiga";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("0B2EC377-ACBC-48D1-8EF8-F787EE4B9E5D");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}