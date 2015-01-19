using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using App = Autodesk.AutoCAD.ApplicationServices;
using cad = Autodesk.AutoCAD.ApplicationServices.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Gem = Autodesk.AutoCAD.Geometry;
using Rtm = Autodesk.AutoCAD.Runtime;

[assembly: Rtm.CommandClass(typeof(xline_pp.Commands))]

namespace xline_pp
{
    public class Commands
    {
        static bool exit = false;
        public static Settings setting = new Settings();


        [Rtm.CommandMethod("AddXLineV")]
        static public void AddXLineV()
        {
            xline_pp.Commands.exit = false;
            while (true)
            {
                Gem.Point3d pointRes = getpoint();
                if (xline_pp.Commands.exit == false)
                {
                    AddXLine(pointRes, new Gem.Point3d(pointRes.X, pointRes.Y + 1, pointRes.Z));
                }
                else
                {
                    return;
                }
            }
        }


        [Rtm.CommandMethod("AddXLineH")]
        static public void AddXLineH()
        {
            xline_pp.Commands.exit = false;
            while (true)
            {
                Gem.Point3d pointRes = getpoint();
                if (xline_pp.Commands.exit == false)
                {
                    AddXLine(pointRes, new Gem.Point3d(pointRes.X + 1, pointRes.Y, pointRes.Z));
                }
                else
                {
                    return;
                }
            }
        }

        private static Gem.Point3d getpoint()
        {
            // Получение текущего документа и базы данных
            App.Document acDoc = App.Application.DocumentManager.MdiActiveDocument;
            Db.Database acCurDb = acDoc.Database;
            Ed.Editor acEd = acDoc.Editor;

            Ed.PromptPointOptions pointOpt = new Ed.PromptPointOptions("\nУкажите базовую точку:");
            pointOpt.UseDashedLine = true;
            pointOpt.AllowNone = true;

            Ed.PromptPointResult pointRes = acEd.GetPoint(pointOpt);
            if (pointRes.Status != Ed.PromptStatus.OK)
            {
                xline_pp.Commands.exit = true;
                return new Gem.Point3d();
            }

            return pointRes.Value;
        }


        static void AddXLine(Gem.Point3d basePoint, Gem.Point3d secpoint)
        {

            setting = getParam();
            saveParam(setting);

            // Получение текущего документа и базы данных
            App.Document acDoc = App.Application.DocumentManager.MdiActiveDocument;
            Db.Database acCurDb = acDoc.Database;
            Ed.Editor acEd = acDoc.Editor;
            //старт транзакции
            using (Db.Transaction acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                // Открытие таблицы Блоков для чтения
                Db.BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, Db.OpenMode.ForRead) as Db.BlockTable;

                // Открытие записи таблицы Блоков пространства Модели для записи
                Db.BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[Db.BlockTableRecord.ModelSpace],
                                                                                Db.OpenMode.ForWrite) as Db.BlockTableRecord;
                // Создание отрезка начинающегося в 0,0 и заканчивающегося в 5,5
                Db.Xline acLine = new Db.Xline();

                acLine.ColorIndex = setting.colorInd;
                acLine.LineWeight = setting.weight;

                acLine.BasePoint = basePoint;
                acLine.SecondPoint = secpoint;

                acLine.SetDatabaseDefaults();
                // Добавление нового объекта в запись таблицы блоков и в транзакцию
                acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);
                //Сохранение нового объекта в базе данных
                acTrans.Commit();
            }
        }




        private static Settings getParam()
        {

            string path = Assembly.GetExecutingAssembly().Location;
            FileInfo fi1 = new FileInfo(path);
            path = fi1.Directory.ToString() + "\\MySettings.xml";
            fi1 = null;



            Settings myObject = default(Settings);


            if (System.IO.File.Exists(path))
            {
                try
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(Settings));
                    using (FileStream myFileStream = new FileStream(path, FileMode.Open))
                    {
                        myObject = (Settings)mySerializer.Deserialize(myFileStream);
                    }

                }
                catch (System.InvalidOperationException ex)
                {
                    System.IO.File.Delete(path);
                    myObject = new Settings(256, Db.LineWeight.LineWeight009);
                }

            }
            else
            {
                myObject = new Settings(256, Db.LineWeight.LineWeight009);
            }
            return myObject;
        }

        private static void saveParam(Settings Setting)
        {
            string path = Assembly.GetExecutingAssembly().Location;
            FileInfo fi1 = new FileInfo(path);
            path = fi1.Directory.ToString() + "\\MySettings.xml";
            fi1 = null;

            XmlSerializer ser = new XmlSerializer(typeof(Settings));
            using (TextWriter writer = new StreamWriter(path, false))
            {
                ser.Serialize(writer, Setting);
            }
        }

    }
}
