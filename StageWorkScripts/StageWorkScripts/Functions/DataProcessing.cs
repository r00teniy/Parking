using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using StageWorkScripts.Models;

using AcBr = Autodesk.AutoCAD.BoundaryRepresentation;

namespace StageWorkScripts.Functions;
internal class DataProcessing
{
    private Variables _variables;
    private Database db = Application.DocumentManager.MdiActiveDocument.Database;
    public DataProcessing(Variables variables)
    {
        _variables = variables;
    }

    internal List<string[]> CreateTableData<T>(TableType type, List<T> list) where T : IElement
    {
        var output = new List<string[]>();
        switch (type)
        {
            case TableType.Curbs:
                output.Add(_variables.curbsHeader);
                List<string> curbCodes = list.Select(x => x.Code).Distinct().ToList();
                curbCodes.Sort();
                foreach (var curbCode in curbCodes)
                {
                    output.Add(CreateTableRow(_variables.curbsHeader.Length, type, list, curbCode));
                }
                break;
            case TableType.Pavements:
                output.Add(_variables.pavementHeader);
                List<string> pavementCodes = list.Select(x => x.Code).Distinct().ToList();
                pavementCodes.Sort();
                foreach (var pavementCode in pavementCodes)
                {
                    output.Add(CreateTableRow(_variables.pavementHeader.Length, type, list, pavementCode));
                }
                break;
            case TableType.ItemGreenery:
                output.Add(_variables.GreeneryItemHeader);
                List<string> greeneryItemCodes = list.Select(x => x.Code).Distinct().ToList();
                greeneryItemCodes.Sort();
                foreach (var greeneryItemCode in greeneryItemCodes)
                {
                    output.Add(CreateTableRow(_variables.GreeneryItemHeader.Length, type, list, greeneryItemCode));
                }
                break;
            case TableType.AreaGreenery:
                output.Add(_variables.GreeneryAreaHeader);
                var notGrassList = list.Where(x => x is not GrassGreeneryModel).ToList();
                List<string> greeneryAreaCodes = notGrassList.Select(x => x.Code).Distinct().ToList();
                greeneryAreaCodes.Sort();
                foreach (var greeneryAreaCode in greeneryAreaCodes)
                {
                    output.Add(CreateTableRow(_variables.GreeneryAreaHeader.Length, type, list, greeneryAreaCode));
                }
                break;
            case TableType.GrassGreenery:
                output.Add(_variables.GreeneryGrassHeader);
                var grassList = list.Where(x => x is GrassGreeneryModel).ToList();
                List<string> grassAreaCodes = grassList.Select(x => x.Code).Distinct().ToList();
                grassAreaCodes.Sort();
                foreach (var grassAreaCode in grassAreaCodes)
                {
                    output.Add(CreateTableRow(_variables.GreeneryGrassHeader.Length, type, list, grassAreaCode));
                }
                break;
            case TableType.StreetFurniture:
                output.Add(_variables.streetFurnitureHeader);
                List<string> furnitureCodes = list.Select(x => x.Code).Distinct().ToList();
                furnitureCodes.Sort();
                foreach (var code in furnitureCodes)
                {
                    output.Add(CreateTableRow(_variables.streetFurnitureHeader.Length, type, list, code));
                }
                break;
            case TableType.EarthVolumes:
                break;
        }
        return output;
    }
    internal string[] CreateTableRow<T>(int numberOfCollumns, TableType type, List<T> list, string code) where T : IElement
    {
        var elements = list.Where(x => x.Code == code).ToList();
        switch (type)
        {
            case TableType.Curbs:
                var curb = elements[0] as CurbModel;
                return new string[] { "", curb.Code, curb.CurbFullName, curb.CurbSize, curb.CurbColor, Math.Ceiling(elements.Sum(x => x.Amount)).ToString(), "" };
            case TableType.Pavements:
                var pavement = elements[0] as IPavement;
                return new string[] { "", pavement.Code, _variables.pointOfUseText[(int)pavement.PointOfUse], pavement.FullName, pavement.Parameters, pavement.Color, elements.Sum(x => x.Amount).ToString("{0.##}"), "" };
            case TableType.ItemGreenery:
                var greeneryItems = elements as List<IGreeneryItem>;
                var specialPavementAreaGI = greeneryItems[0].HasSpecialPavement ? greeneryItems.Sum(x => x.SpecialPavementAreaInM2).ToString("{0.##}") : " ";
                return new string[] { "", greeneryItems[0].Code, greeneryItems[0].GreeneryName, greeneryItems[0].Height.ToString(), greeneryItems[0].TrunkGirthInM.ToString(), greeneryItems[0].CrownSizeInM.ToString(), greeneryItems[0].StoolBedSize, greeneryItems.Sum(x => x.Amount).ToString(), greeneryItems[0].HasSpecialPavement ? greeneryItems[0].SpecialPavementName : " ", specialPavementAreaGI, "" };
            case TableType.AreaGreenery:
                var greeneryArea = elements as List<IGreeneryArea>;
                var heightPlusCrownSize = greeneryArea[0].Height == 0 ? "-" : greeneryArea[0].Height.ToString() + " / " + greeneryArea[0].CrownSizeInM.ToString();
                var specialPavementArea = greeneryArea[0].HasSpecialPavement ? greeneryArea.Sum(x => x.SpecialPavementAreaInM2).ToString("{0.##}") : " ";
                return new string[] { "", greeneryArea[0].Code, greeneryArea[0].GreeneryName, heightPlusCrownSize, greeneryArea[0].StoolBedSize, greeneryArea.Sum(x => x.Amount).ToString("{0.##}"), greeneryArea[0].NumberOfPlantsPerSQM.ToString(), greeneryArea.Sum(x => x.NumberOfPlants).ToString(), greeneryArea[0].HasSpecialPavement ? greeneryArea[0].SpecialPavementName : " ", specialPavementArea, "" };
            case TableType.GrassGreenery:
                var grass = elements[0] as GrassGreeneryModel;
                return new string[] { "", grass.Code, grass.GreeneryName, grass.SeedsDetails, elements.Sum(x => x.Amount).ToString("{0.##}"), "" };
            case TableType.StreetFurniture:
                var furniture = elements[0] as StreetFurnitureModel;
                return new string[] { "", furniture.Code, furniture.FullName, elements.Count.ToString(), furniture.SafeDropHeight.ToString(), furniture.MountType, "" };
            default: return new string[5];

        }
    }
    //Point Containment for checking out if point is inside plot or outside it
    public Region RegionFromClosedCurve(Curve curve)
    {
        if (!curve.Closed)
            throw new ArgumentException("Curve must be closed.");
        DBObjectCollection curves = new DBObjectCollection
        {
            curve
        };
        using (DBObjectCollection regions = Region.CreateFromCurves(curves))
        {
            if (regions == null || regions.Count == 0)
                throw new InvalidOperationException("Failed to create regions");
            if (regions.Count > 1)
                throw new InvalidOperationException("Multiple regions created");
            return regions.Cast<Region>().First();
        }
    }
    public PointContainment GetPointContainment(Curve curve, Point3d point)
    {
        if (!curve.Closed)
            throw new ArgumentException("Полилиния границы должна быть замкнута");
        Region region = RegionFromClosedCurve(curve);
        if (region == null)
            throw new InvalidOperationException("Ошибка, проверьте полилинию границы");
        using (region)
        { return GetPointContainment(region, point); }
    }
    public PointContainment GetPointContainment(Region region, Point3d point)
    {
        PointContainment result = PointContainment.Outside;
        using (Brep brep = new Brep(region))
        {
            if (brep != null)
            {
                using (BrepEntity ent = brep.GetPointContainment(point, out result))
                {
                    if (ent is AcBr.Face)
                        result = PointContainment.Inside;
                }
            }
        }
        return result;
    }
    //Getting hatch border or polyline points to check if it is inside plot or not
    public List<Point3d> GetPointsFromObject<T>(T obj)
    {
        List<Point3d> output = new();
        if (obj is Hatch hat)
        {
            HatchLoop loop = hat.GetLoopAt(0);
            Plane plane = hat.GetPlane();
            var poly = GetBorderFromHatchLoop(loop, plane);
            for (var i = 0; i < poly.NumberOfVertices; i++)
            {
                output.Add(poly.GetPoint3dAt(i));
            }
            return output;
        }
        if (obj is Polyline pl)
        {
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                output.Add(pl.GetPoint3dAt(i));
            }
            return output;
        }
        if (obj is BlockReference br)
        {
            output.Add(br.Position);
            return output;
        }
        throw new System.Exception("This method works with BlockReference, Polyline or Hatch only.");
    }
    //Checking if group of points is inside/on the polyline
    public bool ArePointsInsidePolyline(List<Point3d> points, Polyline pl)
    {
        var isFirstPointIn = GetPointContainment(pl, points[0]);
        for (int i = 1; i < points.Count; i++)
        {
            var isThisPointIn = GetPointContainment(pl, points[i]);
            if (isThisPointIn != isFirstPointIn)
            {
                if (isFirstPointIn == PointContainment.OnBoundary)
                {
                    isFirstPointIn = isThisPointIn;
                }
                else if (isThisPointIn != PointContainment.OnBoundary)
                {
                    throw new System.Exception("Одна из полилиний или штриховок пересекает границу участка, необходимо исправить.");
                }
            }
        }
        return isFirstPointIn == PointContainment.Inside;
    }
    //Getting points that are on other side of plotBorder
    public Point3d? ArePointsOnBothSidesOfBorder(List<Point3d> points, Polyline pl)
    {
        var isFirstPointIn = GetPointContainment(pl, points[0]);
        for (int i = 1; i < points.Count; i++)
        {
            var isThisPointIn = GetPointContainment(pl, points[i]);
            if (isThisPointIn != isFirstPointIn)
            {
                if (isFirstPointIn == PointContainment.OnBoundary)
                {
                    isFirstPointIn = isThisPointIn;
                }
                else if (isThisPointIn != PointContainment.OnBoundary)
                {
                    return points[i];
                }
            }
        }
        return null;
    }
    //Function to check if hatch/polyline/blockreference is inside plot (can have 2+ borders)
    public List<bool> AreObjectsInsidePlot<T>(Polyline plotBorder, List<T> objects)
    {
        if (objects == null)
        {
            return null;
        }
        List<bool> results = new();
        foreach (var item in objects)
        {
            var tempResult = false;
            if (item is Point3d point)
            {
                if (ArePointsInsidePolyline(new List<Point3d>() { point }, plotBorder))
                {
                    tempResult = true;
                }
            }
            else
            {
                if (ArePointsInsidePolyline(GetPointsFromObject(item), plotBorder))
                {
                    tempResult = true;
                }
            }
            results.Add(tempResult);
        }
        return results;
    }
    //Function that returns Polyline from HatchLoop
    public Polyline GetBorderFromHatchLoop(HatchLoop loop, Plane plane)
    {
        //Modified code from Rivilis Restore Hatch Boundary program
        Polyline looparea = new Polyline();
        if (loop.IsPolyline)
        {
            using (Polyline poly = new Polyline())
            {
                int iVertex = 0;
                foreach (BulgeVertex bv in loop.Polyline)
                {
                    poly.AddVertexAt(iVertex++, bv.Vertex, bv.Bulge, 0.0, 0.0);
                }
                if (looparea != null)
                {
                    try
                    {
                        looparea.JoinEntity(poly);
                    }
                    catch
                    {
                        throw new System.Exception("Граница штриховки не может быть воссоздана");
                    }
                }
                else
                {
                    looparea = poly;
                }
            }
        }
        else
        {
            foreach (Curve2d cv in loop.Curves)
            {
                LineSegment2d line2d = cv as LineSegment2d;
                CircularArc2d arc2d = cv as CircularArc2d;
                EllipticalArc2d ellipse2d = cv as EllipticalArc2d;
                NurbCurve2d spline2d = cv as NurbCurve2d;
                if (line2d != null)
                {
                    using (Line ent = new Line())
                    {
                        try
                        {
                            ent.StartPoint = new Point3d(plane, line2d.StartPoint);
                            ent.EndPoint = new Point3d(plane, line2d.EndPoint);
                            looparea.JoinEntity(ent);
                        }
                        catch
                        {
                            looparea.AddVertexAt(0, line2d.StartPoint, 0, 0, 0);
                            looparea.AddVertexAt(1, line2d.EndPoint, 0, 0, 0);
                        }

                    }
                }
                else if (arc2d != null)
                {
                    try
                    {
                        if (arc2d.IsClosed() || Math.Abs(arc2d.EndAngle - arc2d.StartAngle) < 1e-5)
                        {
                            using (Circle ent = new Circle(new Point3d(plane, arc2d.Center), plane.Normal, arc2d.Radius))
                            {
                                looparea.JoinEntity(ent);
                            }
                        }
                        else
                        {
                            if (arc2d.IsClockWise)
                            {
                                arc2d = arc2d.GetReverseParameterCurve() as CircularArc2d;
                            }
                            double angle = new Vector3d(plane, arc2d.ReferenceVector).AngleOnPlane(plane);
                            double startAngle = arc2d.StartAngle + angle;
                            double endAngle = arc2d.EndAngle + angle;
                            using (Arc ent = new Arc(new Point3d(plane, arc2d.Center), plane.Normal, arc2d.Radius, startAngle, endAngle))
                            {
                                looparea.JoinEntity(ent);
                            }
                        }
                    }
                    catch
                    {
                        // Calculating Bulge
                        double deltaAng = arc2d.EndAngle - arc2d.StartAngle;
                        if (deltaAng < 0)
                            deltaAng += 2 * Math.PI;
                        double GetArcBulge = Math.Tan(deltaAng * 0.25);
                        //Adding first arc to polyline
                        looparea.AddVertexAt(0, new Point2d(arc2d.StartPoint.X, arc2d.StartPoint.Y), GetArcBulge, 0, 0);
                        looparea.AddVertexAt(1, new Point2d(arc2d.EndPoint.X, arc2d.EndPoint.Y), 0, 0, 0);
                    }
                }
                else if (ellipse2d != null)
                {
                    using (Ellipse ent = new Ellipse(new Point3d(plane, ellipse2d.Center), plane.Normal, new Vector3d(plane, ellipse2d.MajorAxis) * ellipse2d.MajorRadius, ellipse2d.MinorRadius / ellipse2d.MajorRadius, ellipse2d.StartAngle, ellipse2d.EndAngle))
                    {
                        ent.GetType().InvokeMember("StartParam", BindingFlags.SetProperty, null, ent, new object[] { ellipse2d.StartAngle });
                        ent.GetType().InvokeMember("EndParam", BindingFlags.SetProperty, null, ent, new object[] { ellipse2d.EndAngle });

                        looparea.JoinEntity(ent);
                    }
                }
                else if (spline2d != null)
                {
                    if (spline2d.HasFitData)
                    {
                        NurbCurve2dFitData n2fd = spline2d.FitData;
                        using (Point3dCollection p3ds = new Point3dCollection())
                        {
                            foreach (Point2d p in n2fd.FitPoints) p3ds.Add(new Point3d(plane, p));
                            using (Spline ent = new Spline(p3ds, new Vector3d(plane, n2fd.StartTangent), new Vector3d(plane, n2fd.EndTangent), n2fd.Degree, n2fd.FitTolerance.EqualPoint))
                            {
                                looparea.JoinEntity(ent);
                            }
                        }
                    }
                    else
                    {
                        NurbCurve2dData n2fd = spline2d.DefinitionData;
                        using (Point3dCollection p3ds = new Point3dCollection())
                        {
                            DoubleCollection knots = new DoubleCollection(n2fd.Knots.Count);
                            foreach (Point2d p in n2fd.ControlPoints) p3ds.Add(new Point3d(plane, p));
                            foreach (double k in n2fd.Knots) knots.Add(k);
                            double period = 0;
                            using (Spline ent = new Spline(n2fd.Degree, n2fd.Rational, spline2d.IsClosed(), spline2d.IsPeriodic(out period), p3ds, knots, n2fd.Weights, n2fd.Knots.Tolerance, n2fd.Knots.Tolerance))
                            {
                                looparea.JoinEntity(ent);
                            }
                        }
                    }
                }
            }
        }
        return looparea;
    }
    //Function to get areas for a list on hatches.
    public List<double> GetHatchArea(Transaction tr, List<Hatch> hatchList)
    {
        List<double> hatchAreaList = new();
        var bT = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var bTr = (BlockTableRecord)tr.GetObject(bT[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        for (var i = 0; i < hatchList.Count; i++)
        {
            try
            {
                hatchAreaList.Add(hatchList[i].Area);
            }
            catch
            {
                //changing to count self-intersecting hatches
                Plane plane = hatchList[i].GetPlane();
                double corArea = 0.0;
                for (int k = 0; k < hatchList[i].NumberOfLoops; k++)
                {
                    HatchLoop loop = hatchList[i].GetLoopAt(k);
                    HatchLoopTypes hlt = hatchList[i].LoopTypeAt(k);
                    Polyline looparea = GetBorderFromHatchLoop(loop, plane);
                    // Can get correct value from AcadObject, but need to add in first
                    bTr.AppendEntity(looparea);
                    tr.AddNewlyCreatedDBObject(looparea, true);
                    object pl = looparea.AcadObject;
                    var corrval = (double)pl.GetType().InvokeMember("Area", BindingFlags.GetProperty, null, pl, null);
                    looparea.Erase(); // Erasing polyline we just created
                    if (hlt == HatchLoopTypes.External) // External loops with +
                    {
                        corArea += corrval;
                    }
                    else // Internal with -
                    {
                        corArea -= corrval;
                    }
                }
                hatchAreaList.Add(corArea);
            }
        }
        return hatchAreaList;
    }
}
