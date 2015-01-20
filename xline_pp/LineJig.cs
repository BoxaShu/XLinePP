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
using Clr = Autodesk.AutoCAD.Colors;

namespace xline_pp
{
    class XLineJig : Ed.EntityJig
    {
        private Gem.Point3d mCenterPt;
        private Gem.Point3d mActualPoint;

        public XLineJig(Db.Xline br)
            : base(br)
        {
            mCenterPt = br.BasePoint;
        }

        protected override Ed.SamplerStatus Sampler(Ed.JigPrompts prompts)
        {
            Ed.JigPromptPointOptions jigOpts = new Ed.JigPromptPointOptions();

            jigOpts.UserInputControls = (Ed.UserInputControls.Accept3dCoordinates
                            | Ed.UserInputControls.NoZeroResponseAccepted
                            | Ed.UserInputControls.NoNegativeResponseAccepted
                            | Ed.UserInputControls.NullResponseAccepted);

            jigOpts.Message = "\nEnter insert point: ";
            Ed.PromptPointResult dres = prompts.AcquirePoint(jigOpts);

            if (dres.Status == Ed.PromptStatus.OK)
            {
                if (mActualPoint == dres.Value)
                {
                    return Ed.SamplerStatus.NoChange;
                }
                else
                {
                    mActualPoint = dres.Value;
                }
                return Ed.SamplerStatus.OK;
            }
            else
            {
                return Ed.SamplerStatus.Cancel;
            }
        }

        protected override bool Update()
        {
            mCenterPt = mActualPoint;
            try
            {
                ((Db.Xline)Entity).BasePoint = mCenterPt;
                ((Db.Xline)Entity).Visible = true;
            }
            catch (System.Exception generatedExceptionName)
            {
                return false;
            }
            return true;
        }

        public Db.Entity GetEntity()
        {
            return Entity;
        }

    }
}


