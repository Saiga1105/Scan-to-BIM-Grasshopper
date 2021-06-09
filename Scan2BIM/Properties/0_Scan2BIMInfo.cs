using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Scan2BIM
{
    public class Scan2BIMInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Scan2BIM";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.Icon_Saiga;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Processing point cloud data to Building Information Models";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("b76b3a48-30f1-472f-9ff8-031da36bbcb7");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Maarten Bassier";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "maarten.bassier@kuleuven.be";
            }
        }
    }
}
