using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using App = Autodesk.AutoCAD.ApplicationServices;
using cad = Autodesk.AutoCAD.ApplicationServices.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Gem = Autodesk.AutoCAD.Geometry;
using Rtm = Autodesk.AutoCAD.Runtime;

namespace xline_pp
{
    public class Settings
    {
        private Db.LineWeight _weight = Db.LineWeight.LineWeight009;
        private int _colorInd = 10;

        public Settings()
        {
        }

        public Settings(int colorInd, Db.LineWeight weight)
        {
            this.colorInd = colorInd;
            this.weight = weight;
        }



        public int colorInd
        {
            get { return _colorInd; }
            set { _colorInd = value; }
        }
        public Db.LineWeight weight
        {
            get { return _weight; }
            set { _weight = value; }
        }


    }
}
