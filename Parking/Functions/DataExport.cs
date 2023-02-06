using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using Parking.Models;

namespace Parking.Functions
{
    internal class DataExport
    {
        //Function that generates auotcad table, requires list of rows (string array)
        internal static void CreateTable(List<string[]> list, List<string> buildingNames, List<ParkingModel> parkingReq, int[] onPlot)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock acLckDoc = doc.LockDocument())
                {
                    var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;
                    ObjectId tbSt = new ObjectId();
                    DBDictionary tsd = (DBDictionary)tr.GetObject(db.TableStyleDictionaryId, OpenMode.ForRead);
                    foreach (DBDictionaryEntry entry in tsd)
                    {
                        var tStyle = (TableStyle)tr.GetObject(entry.Value, OpenMode.ForRead);
                        if (tStyle.Name == Variables.parkTableStyleName)
                        { tbSt = entry.Value; }
                    }
                    //Creating required row for table.
                    string[] req = new string[list[0].Length];
                    for (int i = 0; i < parkingReq.Count; i++)
                    {
                        req[2 + i * 5] = parkingReq[i].TotalLongParking.ToString();
                        req[3 + i * 5] = parkingReq[i].TotalShortParking.ToString();
                        req[4 + i * 5] = parkingReq[i].TotalGuestParking.ToString();
                        req[5 + i * 5] = parkingReq[i].TotalDisabledParking.ToString();
                        req[6 + i * 5] = parkingReq[i].TotalDisabledBigParking.ToString();
                    }
                    //Calculating summ of existing parkings
                    string[] ex = new string[list[0].Length];
                    for (int i = 2; i < list[0].Length; i++)
                    {
                        ex[i] = list.Sum(x => Convert.ToInt32(x[i])).ToString();
                    }
                    req[list[0].Length - 1] = parkingReq.Sum(x => x.TotalLongParking + x.TotalShortParking + x.TotalGuestParking).ToString();
                    //Creating table
                    Table tb = new Table
                    {
                        TableStyle = tbSt,
                        Position = DataImport.GetInsertionPoint()
                    };
                    //Creating title
                    tb.Rows[0].Style = "Название";
                    tb.Cells[0, 0].TextString = $"Распределение парковок по домам и участкам на площадке";
                    //Creating header
                    tb.SetRowHeight(8);
                    tb.SetColumnWidth(8);
                    tb.InsertRows(1, 10, 1);
                    tb.InsertRows(2, 8, 1);
                    tb.Rows[1].Style = "Заголовок";
                    tb.InsertRows(3, 18, 1);
                    tb.Rows[2].Style = "Данные";
                    tb.InsertColumns(1, 30, 1);
                    tb.InsertColumns(2, 6, list[0].Length - 3);
                    tb.InsertColumns(list[0].Length - 1, 8, 1);
                    for (int i = 0; i < buildingNames.Count; i++)
                    {
                        CellRange nameRange = CellRange.Create(tb, 1, 2 + i * 5, 1, 2 + i * 5 + 4);
                        tb.MergeCells(nameRange);
                    }
                    CellRange range = CellRange.Create(tb, 1, 0, 1, 1);
                    tb.MergeCells(range);
                    tb.Cells[1, 0].TextString = "Позиция";
                    range = CellRange.Create(tb, 2, 0, 3, 1);
                    tb.MergeCells(range);
                    tb.Cells[2, 0].TextString = "Номер Участка";
                    range = CellRange.Create(tb, 1, list[0].Length - 1, 3, list[0].Length - 1);
                    tb.MergeCells(range);
                    tb.Cells[1, list[0].Length - 1].TextString = "Итого по участку";
                    tb.Cells[1, list[0].Length - 1].Contents[0].Rotation = Math.PI / 2;
                    //Populating row 3
                    for (var i = 0; i < buildingNames.Count; i++)
                    {
                        tb.Cells[1, 2 + i * 5].TextString = buildingNames[i];
                        for (int j = 0; j < 5; j++)
                        {
                            if (j < 3)
                            {
                                range = CellRange.Create(tb, 2, 2 + i * 5 + j, 3, 2 + i * 5 + j);
                                tb.MergeCells(range);
                                tb.Cells[2, 2 + i * 5 + j].TextString = Variables.parkingTypes[j];
                                tb.Cells[2, 2 + i * 5 + j].Contents[0].Rotation = Math.PI / 2;
                            }
                            tb.Cells[3, 2 + i * 5 + j].TextString = Variables.parkingTypes[j];
                            tb.Cells[3, 2 + i * 5 + j].Contents[0].Rotation = Math.PI / 2;
                        }
                        range = CellRange.Create(tb, 2, 2 + i * 5 + 3, 2, 2 + i * 5 + 4);
                        tb.MergeCells(range);
                        tb.Cells[2, 2 + i * 5 + 3].TextString = "в т.ч. МГН";
                    }
                    //Creating individual plot rows
                    var currentRow = 3;
                    for (var j = 0; j < list.Count; j++)
                    {
                        tb.InsertRows(currentRow + 1, 8, 1);
                        currentRow++;
                        if (j != list.Count - 1 && list[j][0] == list[j + 1][0])
                        {
                            tb.InsertRows(currentRow + 1, 8, 1);
                            range = CellRange.Create(tb, currentRow, 0, currentRow + 1, 0);
                            tb.MergeCells(range);
                            tb.Cells[currentRow, 0].TextString = list[j][0];
                            tb.Cells[currentRow, 0].Contents[0].Rotation = Math.PI / 2;
                            //First row
                            for (int i = 1; i < list[j].Length; i++)
                            {
                                tb.Cells[currentRow, i].TextString = ((list[j][i] == "0") || (list[j][i] == null)) ? "" : list[j][i];
                            }
                            //Second row
                            for (int i = 1; i < list[j + 1].Length; i++)
                            {
                                tb.Cells[currentRow + 1, i].TextString = ((list[j + 1][i] == "0") || (list[j + 1][i] == null)) ? "" : list[j + 1][i];
                            }
                            currentRow++;
                            j++;
                        }
                        else
                        {
                            range = CellRange.Create(tb, currentRow, 0, currentRow, 1);
                            tb.MergeCells(range);
                            // In case we only have parking on this plot
                            tb.Cells[currentRow, 0].TextString = list[j][0] + " " + list[j][1];
                            //option for different naming
                            //tb.Cells[currentRow, 0].TextString = list[j][1].Contains("Паркинг") ? tb.Cells[currentRow, 0].TextString = list[j][0] + " " + list[j][1] : tb.Cells[currentRow, 0].TextString = list[j][0];
                            for (int i = 2; i < list[j].Length; i++)
                            {
                                tb.Cells[currentRow, i].TextString = ((list[j][i] == "0") || (list[j][i] == null)) ? "" : list[j][i];
                            }
                        }
                    }
                    //Adding summ rows
                    //Row for on same plot
                    currentRow++;
                    tb.InsertRows(currentRow, 8, 1);
                    for (var i = 0; i < onPlot.Length; i++)
                    {
                        tb.Cells[currentRow, 2 + i].TextString = onPlot[i].ToString() == "0" ? "" : onPlot[i].ToString();
                    }
                    range = CellRange.Create(tb, currentRow, 0, currentRow, 1);
                    tb.MergeCells(range);
                    tb.Cells[currentRow, 0].TextString = "Итого в границах ГПЗУ";
                    //Row for outside plot
                    currentRow++;
                    tb.InsertRows(currentRow, 8, 1);
                    for (var i = 0; i < onPlot.Length; i++)
                    {
                        tb.Cells[currentRow, 2 + i].TextString = (Convert.ToInt32(ex[2 + i]) - onPlot[i]).ToString() == "0" ? "" : (Convert.ToInt32(ex[2 + i]) - onPlot[i]).ToString();
                    }
                    range = CellRange.Create(tb, currentRow, 0, currentRow, 1);
                    tb.MergeCells(range);
                    tb.Cells[currentRow, 0].TextString = "Итого за границами ГПЗУ";
                    //Row for total ex
                    currentRow++;
                    tb.InsertRows(currentRow, 8, 1);
                    for (var i = 2; i < ex.Length; i++)
                    {
                        tb.Cells[currentRow, i].TextString = ex[i] == "0" ? "" : ex[i];
                    }
                    range = CellRange.Create(tb, currentRow, 0, currentRow, 1);
                    tb.MergeCells(range);
                    tb.Cells[currentRow, 0].TextString = "Итого для позиции";
                    //Row for total required
                    currentRow++;
                    tb.InsertRows(currentRow, 8, 1);
                    for (var i = 2; i < req.Length; i++)
                    {
                        tb.Cells[currentRow, i].TextString = ((req[i] == "0") || (req[i] == null)) ? "" : req[i];
                    }
                    range = CellRange.Create(tb, currentRow, 0, currentRow, 1);
                    tb.MergeCells(range);
                    tb.Cells[currentRow, 0].TextString = "Итого требуется";
                    currentRow++;
                    //Deficit/proficit
                    tb.InsertRows(currentRow, 8, 2);
                    var prof = 0; //for calculating total proficit
                    var def = 0; // for calculating total deficit
                    for (int i = 0; i < buildingNames.Count; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            var diff = Convert.ToInt32(ex[2 + i * 5 + j]) - Convert.ToInt32(req[2 + i * 5 + j]);
                            if (diff < 0)
                            {
                                tb.Cells[currentRow + 1, 2 + i * 5 + j].TextString = Math.Abs(diff).ToString();
                                def += j < 3 ? Math.Abs(diff) : 0;
                            }
                            if (diff > 0)
                            {
                                tb.Cells[currentRow, 2 + i * 5 + j].TextString = diff.ToString() == "0" ? "" : diff.ToString();
                                prof += j < 3 ? diff : 0;
                            }
                        }
                    }
                    range = CellRange.Create(tb, currentRow, 0, currentRow, 1);
                    tb.MergeCells(range);
                    tb.Cells[currentRow, 0].TextString = "Профицит";
                    range = CellRange.Create(tb, currentRow + 1, 0, currentRow + 1, 1);
                    tb.MergeCells(range);
                    tb.Cells[currentRow + 1, 0].TextString = "Дефицит";
                    tb.Cells[currentRow, list[0].Length - 1].TextString = prof.ToString();
                    tb.Cells[currentRow + 1, list[0].Length - 1].TextString = def.ToString();
                    currentRow += 2;
                    //Total +-
                    tb.InsertRows(currentRow, 8, 1);
                    range = CellRange.Create(tb, currentRow, 0, currentRow, list[0].Length - 2);
                    tb.MergeCells(range);
                    tb.Cells[currentRow, 0].TextString = $"Итого {(prof > def ? "профицит" : "дефицит")}  ";
                    tb.Cells[currentRow, 0].Alignment = CellAlignment.MiddleRight;
                    tb.Cells[currentRow, list[0].Length - 1].TextString = Math.Abs(prof - def).ToString();
                    range.Borders.Horizontal.Margin = 2;
                    //Setting border lineweight
                    //Horisontal
                    for (int i = 0; i < 7; i++)
                    {
                        range = CellRange.Create(tb, 3 + list.Count + i, 0, 3 + list.Count + i, list[0].Length - 1);
                        range.Borders.Bottom.LineWeight = LineWeight.LineWeight050;
                    }
                    range = CellRange.Create(tb, 3, 0, 3, list[0].Length - 1);
                    range.Borders.Bottom.LineWeight = LineWeight.LineWeight050;
                    //Vertical
                    for (int i = 0; i < list[0].Length - 2; i++)
                    {
                        if (i != 1 && (i - 1) % 5 != 0)
                        {
                            range = CellRange.Create(tb, 2, i, currentRow, i);
                            range.Borders.Right.LineWeight = LineWeight.ByLayer;
                        }
                    }
                    for (int i = 0; i < buildingNames.Count; i++)
                    {
                        range = CellRange.Create(tb, 4, 2 + i * 5, currentRow, 2 + i * 5 + 4);
                        short colorIndex = Variables.parkingTableColors[Convert.ToInt16(buildingNames[i].Split('-')[1]) - 1];
                        range.BackgroundColor = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                    }
                    //Adding table to drawing
                    tb.GenerateLayout();
                    btr.AppendEntity(tb);
                    tr.AddNewlyCreatedDBObject(tb, true);
                }
                tr.Commit();
            }
        }
    }
}
