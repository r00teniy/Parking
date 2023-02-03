using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

using Parking.Models;

namespace Parking.Functions;

internal static class DataProcessing
{
    //Function to get all informations on plot borders needed for model
    public static List<List<ZoneBorderModel>> GetZoneBorders()
    {
        //TODO: add option to get them from anywhere?
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        List<List<ZoneBorderModel>> output = new List<List<ZoneBorderModel>>()
        {
            new List<ZoneBorderModel>(),
            new List<ZoneBorderModel>()
        };
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            using (DocumentLock acLckDoc = doc.LockDocument())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead, false) as BlockTableRecord;
                foreach (ObjectId objectId in btr)
                {
                    //Getting all polylines
                    if (objectId.ObjectClass == Variables.rxClassPolyline)
                    {
                        var pl = tr.GetObject(objectId, OpenMode.ForRead) as Polyline;
                        //getting polylines of building plot borders
                        if (pl.Layer.Contains(Variables.buildingBorderLayer) && pl != null)
                        {
                            output[0].Add(new ZoneBorderModel(pl.Layer.Replace(Variables.buildingBorderLayer, ""), pl));
                        }
                        //getting stage borders
                        else if (pl.Layer.Contains(Variables.stageBorderLayer) && pl != null)
                        {
                            output[1].Add(new ZoneBorderModel(pl.Layer.Replace(Variables.stageBorderLayer, ""), pl));
                        }
                    }
                }
            }
            tr.Commit();
        }
        return output;
    }
    //Getting Plot borders
    public static List<PlotBorderModel> GetPlotBorders()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;
        List<PlotBorderModel> output = new List<PlotBorderModel>();
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            using (DocumentLock acLckDoc = doc.LockDocument())
            {
                List<Polyline> plotsBordersList = DataImport.GetAllElementsOfTypeOnLayer<Polyline>(Variables.plotsBorderLayer);
                List<DBText> plotsBorderTextobjects = DataImport.GetAllElementsOfTypeOnLayer<DBText>(Variables.plotNumbersLayer);
                if (plotsBordersList.Count == plotsBorderTextobjects.Count)
                {
                    for (int i = 0; i < plotsBordersList.Count; i++)
                    {
                        int countCheck = output.Count;
                        foreach (var item in plotsBorderTextobjects)
                        {
                            // Checking if text position is inside polyline
                            if (DataImport.CheckIfObjectIsInsidePolyline(plotsBordersList[i], item) != PointContainment.Outside)
                            {
                                // Creating EntityBorderM<odel
                                output.Add(new PlotBorderModel(item.TextString, plotsBordersList[i]));
                            }
                        }
                        if (countCheck == output.Count) //chercking that every border has text inside
                        {
                            ed.WriteMessage("К 1+ из границ не был найден номер, проверьте что у каждой границы есть номер внутри\n");
                        }
                    }
                }
                else
                {
                    ed.WriteMessage("Кол-во участков и подписей не совпадает\n");
                }
            }
            tr.Commit();
        }
        return output;
    }

    //Function that creates parking block models for existing parking parts
    public static List<ParkingBlockModel> GetExParkingBlocks(List<ZoneBorderModel> borders)
    {
        List<ParkingBlockModel> output = new List<ParkingBlockModel>();
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            using (DocumentLock acLckDoc = doc.LockDocument())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead, false) as BlockTableRecord;
                foreach (ObjectId objectId in btr)
                {
                    if (objectId.ObjectClass == Variables.rxClassBlockReference)
                    {
                        var br = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                        try
                        {
                            ObjectId oi = br.AttributeCollection[1];
                            var attRef = tr.GetObject(oi, OpenMode.ForRead) as AttributeReference;
                            ObjectId oi2 = br.AttributeCollection[0];
                            var attRef2 = tr.GetObject(oi2, OpenMode.ForRead) as AttributeReference;
                            ObjectId oi3 = br.AttributeCollection[3];
                            var attRef3 = tr.GetObject(oi3, OpenMode.ForRead) as AttributeReference;

                            if (attRef.Tag == "Этап" && attRef2.Tag == "КОЛ-ВО")
                            {
                                string[] dynBlockPropValues = new string[7];
                                var pc = br.DynamicBlockReferencePropertyCollection;
                                for (var j = 0; j < pc.Count; j++)
                                {
                                    for (int i = 0; i < Variables.parkingBlockPararmArray.Length; i++)
                                    {
                                        if (pc[j].PropertyName == Variables.parkingBlockPararmArray[i])
                                        { dynBlockPropValues[i] = pc[j].Value.ToString(); }
                                    }
                                }
                                dynBlockPropValues[4] = attRef.TextString;
                                dynBlockPropValues[5] = attRef2.TextString;
                                dynBlockPropValues[6] = attRef3.TextString;
                                output.Add(new ParkingBlockModel(dynBlockPropValues));
                            }
                        }
                        catch { }
                        if (br.Layer == Variables.parkingBuildingsLayer && br != null)
                        {
                            var pc = br.DynamicBlockReferencePropertyCollection;
                            var plotNumbner = borders.Where(x => x.Name == pc[1].ToString()).First().PlotNumber;
                            for (var i = 8; i < 22; i += 2)
                            {
                                if (Convert.ToInt32(pc[i + 1].Value) != 0)
                                {
                                    output.Add(new ParkingBlockModel(Convert.ToInt32(pc[i + 1].Value), pc[i].Value.ToString(), plotNumbner));
                                }
                            }
                        }
                    }
                }
            }
        }
        return output;
    }
    internal static void CreateParkingTableWithData(CityModel city)
    {
        var plotBorders = GetPlotBorders();
        var zoneBorders = GetZoneBorders();
        var parkingBlocks = GetExParkingBlocks(zoneBorders[0]);
        var buildingNames = zoneBorders[0].OrderBy(x => x.Name).Select(x => x.Name).ToList();
        var plotNumbers = plotBorders.Select(x => x.PlotNumber).ToList();
        var buildingPlotNumbers = zoneBorders[0].OrderBy(x => x.Name).Select(x => x.PlotNumber).ToList();
        var exParking = GetExParking(parkingBlocks);
        //Getting buildings
        var buildings = new List<ApartmentBuildingModel>();
        foreach (var br in DataImport.GetAllElementsOfTypeOnLayer<BlockReference>(Variables.apartmentsBuildingsLayer))
        {
            string[] dynBlockPropValues = new string[9];
            DynamicBlockReferencePropertyCollection pc = br.DynamicBlockReferencePropertyCollection;
            for (int i = 0; i < dynBlockPropValues.Length; i++)
            {
                dynBlockPropValues[i] = pc[i].Value.ToString();
            }
            Point3d midPoint = GetCenterOfABlock(br);
            buildings.Add(new ApartmentBuildingModel(city, dynBlockPropValues, zoneBorders[0].Where(x => x.Name == dynBlockPropValues[1]).First(), exParking.Where(x => x.Name == dynBlockPropValues[1]).First(), midPoint));
        }
        //Getting parking buildings
        var parkingBuildings = new List<ParkingBuildingModel>();
        foreach (var br in DataImport.GetAllElementsOfTypeOnLayer<BlockReference>(Variables.parkingBuildingsLayer))
        {
            string[] dynBlockPropValues = new string[8];
            DynamicBlockReferencePropertyCollection pc = br.DynamicBlockReferencePropertyCollection;
            for (int i = 0; i < 8; i++)
            {
                dynBlockPropValues[i] = pc[i].Value.ToString();
            }
            Point3d midPoint = GetCenterOfABlock(br);
            parkingBuildings.Add(new ParkingBuildingModel(city, dynBlockPropValues, zoneBorders[1].FirstOrDefault(c => c.Name == dynBlockPropValues[1]), midPoint));
        }
        //Creating lines for table
        List<string[]> parkTableList = new List<string[]>();
        foreach (var item in plotNumbers)
        {
            var test = parkingBuildings.Where(x => x.PlotNumber == item).ToList();
            if (test.Count == 0)
            {
                parkTableList.Add(CreateLineForParkingTable(parkingBlocks, buildingNames, item));
            }
            else
            {
                parkTableList.Add(CreateLineForParkingTable(parkingBlocks, buildingNames, item));
                foreach (var t in test)
                {
                    parkTableList.Add(CreateLineForParkingTable(parkingBlocks, buildingNames, item, zoneBorders[0], true, t));
                }
            }
        }
        parkTableList.RemoveAll(x => x == null);
        //Getting parking requirements with LINQ
        var parkReqForTable =
        (from build in buildings
         orderby build.Name
         select build.TotalParkingReq).ToList();
        //Getting total existing parkings on plot of each building.
        var exParkingOnPlot = GetExParkingOnBuildingPlot(parkingBlocks, buildingNames, buildingPlotNumbers);
        //Creating table in autocad
        DataExport.CreateTable(parkTableList, buildingNames, parkReqForTable, exParkingOnPlot, "Name");
    }
    public static string[] CreateLineForParkingTable(List<ParkingBlockModel> parkingBlocks, List<string> names, string plotNumber, List<ZoneBorderModel> borders = null, bool isParkingBuilding = false, ParkingBuildingModel parBuild = null)
    {
        string[] output = new string[names.Count * 5 + 3];
        int[] parkingNumbers = new int[names.Count * 5];
        output[0] = plotNumber;
        output[1] = (isParkingBuilding && parBuild != null) ? $"Паркинг {borders.Where(x => x.PlotNumber == plotNumber).Select(x => x.Name).First()}\n (на {parBuild.MaxNumberOfParkingSpaces} м/мест)" : "\nОткрытые парковки";
        //creating array for this plot
        foreach (var park in parkingBlocks)
        {
            if (park.PlotNumber == plotNumber && park.IsInBuilding == isParkingBuilding)
            {
                for (int i = 0; i < names.Count; i++)
                {
                    if (park.ParkingIsForBuildingName == names[i])
                    {
                        switch (park.Type)
                        {
                            case "Long":
                                parkingNumbers[i * 5] += park.NumberOfParkings;
                                break;
                            case "Short":
                                parkingNumbers[i * 5 + 1] += park.NumberOfParkings;
                                if (park.IsForDisabled)
                                {
                                    parkingNumbers[i * 5 + 3] += park.NumberOfParkings;
                                }
                                if (park.IsForDisabledExtended)
                                {
                                    parkingNumbers[i * 5 + 4] += park.NumberOfParkings;
                                }
                                break;
                            case "Guest":
                                parkingNumbers[i * 5 + 2] += park.NumberOfParkings;
                                if (park.IsForDisabled)
                                {
                                    parkingNumbers[i * 5 + 3] += park.NumberOfParkings;
                                }
                                if (park.IsForDisabledExtended)
                                {
                                    parkingNumbers[i * 5 + 4] += park.NumberOfParkings;
                                }
                                break;
                        }
                    }
                }
            }
        }
        if (parkingNumbers.Sum() != 0)
        {
            //Creating output string array
            output[output.Length - 1] = parkingNumbers.Sum().ToString();
            for (int i = 0; i < parkingNumbers.Length; i++)
            {
                output[i + 2] = parkingNumbers[i].ToString();
            }
            return output;
        }
        else
        {
            return null;
        }
    }
    //Creating number of parkings on same plot as building
    public static int[] GetExParkingOnBuildingPlot(List<ParkingBlockModel> parkingBlocks, List<string> names, List<string> plotNumbers)
    {
        int[] parkingNumbers = new int[names.Count * 5];
        foreach (var park in parkingBlocks)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (park.ParkingIsForBuildingName == names[i] && park.PlotNumber == plotNumbers[i])
                {
                    switch (park.Type)
                    {
                        case "Long":
                            parkingNumbers[i * 5] += park.NumberOfParkings;
                            break;
                        case "Short":
                            parkingNumbers[i * 5 + 1] += park.NumberOfParkings;
                            if (park.IsForDisabled)
                            { parkingNumbers[i * 5 + 3] += park.NumberOfParkings; }
                            if (park.IsForDisabledExtended)
                            { parkingNumbers[i * 5 + 4] += park.NumberOfParkings; }
                            break;
                        case "Guest":
                            parkingNumbers[i * 5 + 2] += park.NumberOfParkings;
                            if (park.IsForDisabled)
                            { parkingNumbers[i * 5 + 3] += park.NumberOfParkings; }
                            if (park.IsForDisabledExtended)
                            { parkingNumbers[i * 5 + 4] += park.NumberOfParkings; }
                            break;
                    }
                }
            }
        }
        return parkingNumbers;
    }
    //Get center of a block
    internal static Point3d GetCenterOfABlock(BlockReference bl)
    {
        try
        {
            Extents3d ext = bl.GeometricExtents;
            Point3d center = ext.MinPoint + (ext.MaxPoint - ext.MinPoint) / 2.0;
            return center;
        }
        catch
        {
            return bl.Position;
        }
    }
    //Calculating formula passed as string
    internal static double CalculateSimpleFormula(string formula)
    {
        System.Data.DataTable table = new();
        table.Columns.Add("myExpression", string.Empty.GetType(), formula);
        DataRow row = table.NewRow();
        table.Rows.Add(row);
        return double.Parse((string)row["myExpression"]);
    }
    private static List<ParkingModel> GetExParking(List<ParkingBlockModel> blocks)
    {
        List<ParkingModel> output = new List<ParkingModel>();
        var buildingNames = blocks.Select(x => x.ParkingIsForBuildingName).Distinct().OrderBy(x => x).ToArray();
        List<ParkingBlockModel>[] sortedBlocks = new List<ParkingBlockModel>[buildingNames.Length];
        foreach (var block in blocks)
        {
            var nameId = Array.IndexOf(buildingNames, block.ParkingIsForBuildingName);
            sortedBlocks[nameId].Add(block);
        }
        foreach (var block in sortedBlocks)
        {
            output.Add(new ParkingModel(block, null, block[0].ParkingIsForBuildingName));
        }
        return output;
    }
}