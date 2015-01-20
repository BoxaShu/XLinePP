using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Db = Autodesk.AutoCAD.DatabaseServices;


namespace xline_pp
{
    public class Settings
    {
        private Db.LineWeight _weight = Db.LineWeight.LineWeight009;
        private int _colorInd = 10;
        private string _layerName = "xline";

        public Settings()
        {
        }

        public Settings(int colorInd, Db.LineWeight weight, string layerName)
        {
            this.colorInd = colorInd;
            this.weight = weight;
            this.layerName = layerName;
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

        public string layerName
        {
            get { return _layerName; }
            set { _layerName = value; }
        }


    }
}
