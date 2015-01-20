
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
//using System.Threading.Tasks;
using System.Xml.Serialization;

using App = Autodesk.AutoCAD.ApplicationServices;
using cad = Autodesk.AutoCAD.ApplicationServices.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Gem = Autodesk.AutoCAD.Geometry;
using Rtm = Autodesk.AutoCAD.Runtime;
using Clr= Autodesk.AutoCAD.Colors;

[assembly: Rtm.CommandClass(typeof(xline_pp.Commands))]

namespace xline_pp
{
    public class Commands
    {
        static bool exit = false;
        public static Settings setting = new Settings();


        [Rtm.CommandMethod("XLineV")]
        static public void XLineV()
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


        [Rtm.CommandMethod("XLineH")]
        static public void XLineH()
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

        [Rtm.CommandMethod("XLineJigV")]
        static public void XLineJigV()
        {
            XLineJig(new Gem.Point3d(0, 0, 0), 
                      new Gem.Point3d(0,1,0));
        }

        [Rtm.CommandMethod("XLineJigH")]
        static public void XLineJigH()
        {
            XLineJig(new Gem.Point3d(0, 0, 0),
                      new Gem.Point3d(1, 0, 0));
        }





         private static void XLineJig(Gem.Point3d basePoint, Gem.Point3d secpoint)
        {

            setting = getParam();
            saveParam(setting);
            crLayers(setting.layerName, setting.colorInd, setting.weight);
         
            // Получение текущего документа и базы данных
            App.Document acDoc = App.Application.DocumentManager.MdiActiveDocument;
            Db.Database acCurDb = acDoc.Database;
            Ed.Editor acEd = acDoc.Editor;

            while (true)
            {
                    using (Db.Transaction acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                    {
                        Db.ObjectId tabId = AddXLine_Jig(basePoint, secpoint);
                        Db.Xline acXLine = (Db.Xline)acTrans.GetObject(tabId, Db.OpenMode.ForWrite);

                        XLineJig entJig = new XLineJig(acXLine);
                        acDoc.TransactionManager.QueueForGraphicsFlush();
                        Ed.PromptPointResult pkr = acEd.Drag(entJig) as Ed.PromptPointResult;
                        if (pkr.Status == Ed.PromptStatus.OK)
                        {
                            acXLine.BasePoint = pkr.Value;
                            acXLine.Layer = setting.layerName;
                            acXLine.ColorIndex = setting.colorInd;
                            acXLine.LineWeight = setting.weight;
                            acDoc.TransactionManager.QueueForGraphicsFlush();
                        }
                        else
                        {
                            return;
                        }
                        acTrans.Commit();
                    }
            }
        }



        private static Db.ObjectId AddXLine_Jig(Gem.Point3d basePoint, Gem.Point3d secpoint)
        {

            Db.ObjectId ret = Db.ObjectId.Null;

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

                acLine.Layer = setting.layerName;

                acLine.BasePoint = basePoint;
                acLine.SecondPoint = secpoint;
                acLine.Visible = false;

                acLine.SetDatabaseDefaults();
                // Добавление нового объекта в запись таблицы блоков и в транзакцию
                acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);

                ret = acLine.ObjectId;
                //Сохранение нового объекта в базе данных
                acTrans.Commit();
            }
            return ret;
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

        private static void AddXLine(Gem.Point3d basePoint, Gem.Point3d secpoint)
        {

            setting = getParam();
            saveParam(setting);
            crLayers(setting.layerName, setting.colorInd, setting.weight);

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
               
                acLine.Layer = setting.layerName;

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
                    myObject = new Settings(256, Db.LineWeight.LineWeight009,"xline");
                }

            }
            else
            {
                myObject = new Settings(256, Db.LineWeight.LineWeight009,"xline");
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


        private static void crLayers(string str, int colorInd, Db.LineWeight weight)
        {
            // Получение текущего документа и базы данных
            App.Document acDoc = App.Application.DocumentManager.MdiActiveDocument;
            Db.Database acCurDb = acDoc.Database;
            Ed.Editor acEd = acDoc.Editor;

            //Функция создания слоев
            //Начало транзакции
            using (Db.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                //Открываю таблицу слоев для чтения
                Db.LayerTable acLyrTbl = default(Db.LayerTable);
                //тут возникает ошибка при отладке.
                acLyrTbl = (Db.LayerTable)acTrans.GetObject(acCurDb.LayerTableId, Db.OpenMode.ForRead);
                //В этой переменной наименование слоя
                //string sLayerName = str.ToString();
                //Если этого слоя нет, то создаем его
                if (acLyrTbl.Has(str) == false)
                {
                    Db.LayerTableRecord acLyrTblRec = new Db.LayerTableRecord();

                    //Создаем новый слой с заданными параметрами
                    acLyrTblRec.Name = str;
                    acLyrTblRec.Color = Clr.Color.FromColorIndex(
                        Clr.ColorMethod.ByAci,
                        (short)colorInd
                        );

                    acLyrTblRec.Description = str + " создан программой";
                    acLyrTblRec.LineWeight = weight;

                    acLyrTblRec.IsPlottable = true;
                    acLyrTblRec.IsOff = false;
                    acLyrTblRec.IsFrozen = false;
                    acLyrTblRec.IsLocked = false;

                    //Обновляем таблицу слоев для записи
                    acLyrTbl.UpgradeOpen();

                    //Добавляем новый слой в таблицу слоев
                    acLyrTbl.Add(acLyrTblRec);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                }

                //Сохранение изменений и завершение транзакции
                acTrans.Commit();
            }
        }
    }
}
