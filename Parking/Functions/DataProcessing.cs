using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

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
    public static List<List<ZoneBorderModel>> GetZoneBorders(string xRefName, bool everywhere)
    {
        List<List<ZoneBorderModel>> output = new()
        {
            new List<ZoneBorderModel>(),
            new List<ZoneBorderModel>()
        };
        List<Polyline> buildingBorders;
        if (everywhere)
        {
            buildingBorders = DataImport.GetAllElementsOfTypeOnLayer<Polyline>(Variables.xRefBuildingBorderLayer, xRefName, everywhere);
            buildingBorders.AddRange(DataImport.GetAllElementsOfTypeOnLayer<Polyline>(Variables.buildingBorderLayer, null, false));
        }
        else
        {
            buildingBorders = DataImport.GetAllElementsOfTypeOnLayer<Polyline>(Variables.buildingBorderLayer, xRefName, everywhere);
        }
        foreach (var buildingBorder in buildingBorders)
        {
            if (buildingBorder.Layer.Contains("|"))
            {
                Regex name = new(@"ГП-\d+");
                output[0].Add(new ZoneBorderModel(name.Match(buildingBorder.Layer).Value, buildingBorder));
            }
            else
            {
                output[0].Add(new ZoneBorderModel(buildingBorder.Layer.Replace(Variables.buildingBorderLayer, ""), buildingBorder));
            }
        }
        var stageBorders = DataImport.GetAllElementsOfTypeOnLayer<Polyline>(Variables.stageBorderLayer, xRefName, everywhere);
        foreach (var stageBorder in stageBorders)
        {
            if (stageBorder.Layer.Contains("|"))
            {
                output[1].Add(new ZoneBorderModel(stageBorder.Layer.Split('|')[1].Replace(Variables.stageBorderLayer, ""), stageBorder));
            }
            else
            {
                output[1].Add(new ZoneBorderModel(stageBorder.Layer.Replace(Variables.stageBorderLayer, ""), stageBorder));
            }
        }
        return output;
    }
    //Getting Plot borders
    public static List<PlotBorderModel> GetPlotBorders(string xRefName, bool everywhere)
    {
        List<PlotBorderModel> output = new();

        List<Polyline> plotsBordersList = DataImport.GetAllElementsOfTypeOnLayer<Polyline>(Variables.plotsBorderLayer, xRefName, everywhere);

        foreach (var border in plotsBordersList)
        {
            if (border.Layer.Contains('|'))
            {
                output.Add(new PlotBorderModel(border.Layer.Split('|')[1].Replace(Variables.plotsBorderLayer, ""), border));
            }
            else
            {
                output.Add(new PlotBorderModel(border.Layer.Replace(Variables.plotsBorderLayer, ""), border));
            }
        }

        return output;
    }

    //Function that creates parking block models for existing parking parts
    public static List<ParkingBlockModel> GetExParkingBlocks(List<ZoneBorderModel> borders, string xRefName, bool everywhere)
    {
        List<ParkingBlockModel> output = new();
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;

        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            using (DocumentLock acLckDoc = doc.LockDocument())
            {
                var xrefList = new List<XrefGraphNode>();
                var btrList = new List<BlockTableRecord>();
                var bT = tr.GetObject(db.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                if (everywhere)
                {
                    XrefGraph XrGraph = db.GetHostDwgXrefGraph(false);
                    for (int i = 1; i < XrGraph.NumNodes; i++)
                    {
                        xrefList.Add(XrGraph.GetXrefNode(i));
                    }
                    btrList.Add((BlockTableRecord)tr.GetObject(bT[BlockTableRecord.ModelSpace], OpenMode.ForRead));
                }
                else if (xRefName != null)
                {
                    XrefGraph XrGraph = db.GetHostDwgXrefGraph(false);
                    for (int i = 0; i < XrGraph.NumNodes; i++)
                    {
                        XrefGraphNode XrNode = XrGraph.GetXrefNode(i);
                        if (XrNode.Name == xRefName)
                        {
                            xrefList.Add(XrNode);
                            break;
                        }
                    }
                }
                else
                {
                    btrList.Add((BlockTableRecord)tr.GetObject(bT[BlockTableRecord.ModelSpace], OpenMode.ForRead));
                }

                foreach (var xref in xrefList)
                {
                    btrList.Add((BlockTableRecord)tr.GetObject(xref.BlockTableRecordId, OpenMode.ForRead));
                }
                foreach (var btr in btrList)
                {
                    foreach (ObjectId objectId in btr)
                    {
                        if (objectId.ObjectClass == Variables.rxClassBlockReference)
                        {
                            var br = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                            if (br.AttributeCollection.Count == 4)
                            {

                            }
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
                                var plotNumbner = borders.Where(x => x.Name == pc[1].Value.ToString()).First().PlotNumber;
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
            tr.Commit();
        }
        return output;
    }
    internal static void CreateParkingTableWithData(CityModel city, string plotsXref, bool plotsAll, string zonesXref, bool zonesAll, string parkingXref, bool parkingAll)
    {
        var plotBorders = GetPlotBorders(plotsXref, plotsAll);
        var zoneBorders = GetZoneBorders(zonesXref, zonesAll);
        AddPlotNumbersToZones(ref zoneBorders, plotBorders);
        DataExport.SetPlotNumbersInBlocks(plotBorders, parkingXref, parkingAll);
        var parkingBlocks = GetExParkingBlocks(zoneBorders[0], parkingXref, parkingAll);
        var buildingNames = zoneBorders[0].OrderBy(x => x.Name).Select(x => x.Name).ToList();
        var plotNumbers = plotBorders.Select(x => x.PlotNumber).ToList();
        var buildingPlotNumbers = zoneBorders[0].OrderBy(x => x.Name).Select(x => x.PlotNumber).ToList();
        var exParking = GetExParking(parkingBlocks);
        //Getting buildings
        var buildings = DataImport.GetApartmentBuildings(city, zoneBorders[0], exParking);
        //Getting parking buildings
        var parkingBuildings = DataImport.GetParkingBuildings(city, zoneBorders[0], exParking);
        //Creating lines for table
        List<string[]> parkTableList = new();
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
        DataExport.CreateTable(parkTableList, buildingNames, parkReqForTable, exParkingOnPlot);
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
        List<ParkingModel> output = new();
        var buildingNames = blocks.Select(x => x.ParkingIsForBuildingName).Distinct().OrderBy(x => x).ToArray();
        List<List<ParkingBlockModel>> sortedBlocks = new();
        for (int i = 0; i < buildingNames.Length; i++)
        {
            sortedBlocks.Add(new List<ParkingBlockModel>());
        }
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
    //Function that adds plotNumber to zones
    private static void AddPlotNumbersToZones(ref List<List<ZoneBorderModel>> zones, List<PlotBorderModel> plots)
    {
        for (int j = 0; j < zones.Count; j++)
        {
            for (var i = 0; i < zones[j].Count; i++)
            {
                Point3d point = zones[j][i].Polyline.GeometricExtents.MinPoint + (zones[j][i].Polyline.GeometricExtents.MaxPoint - zones[j][i].Polyline.GeometricExtents.MinPoint) / 2;
                if (DataImport.GetPointContainment(zones[j][i].Polyline, point) != PointContainment.Outside)
                {
                    foreach (var pl in plots)
                    {
                        if (DataImport.GetPointContainment(pl.Polyline, point) != PointContainment.Outside)
                        {
                            zones[j][i].PlotNumber = pl.PlotNumber;
                        }
                    }
                }
            }
        }
    }
}