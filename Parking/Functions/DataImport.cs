﻿using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using AcBr = Autodesk.AutoCAD.BoundaryRepresentation;

namespace Parking.Functions;

internal class DataImport
{
    public static List<T> GetAllElementsOfTypeInDrawing<T>(string xrefName = null, bool everywhere = false) where T : Entity
    {
        List<T> output = new();
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        var xrefList = new List<XrefGraphNode>();
        var btrList = new List<BlockTableRecord>();
        using (DocumentLock lk = doc.LockDocument())
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                var bT = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord bTr;
                if (everywhere)
                {
                    XrefGraph XrGraph = db.GetHostDwgXrefGraph(false);
                    for (int i = 0; i < XrGraph.NumNodes; i++)
                    {
                        xrefList.Add(XrGraph.GetXrefNode(i));
                    }
                    btrList.Add((BlockTableRecord)tr.GetObject(bT[BlockTableRecord.ModelSpace], OpenMode.ForRead));
                }
                else if (xrefName != null)
                {
                    XrefGraph XrGraph = db.GetHostDwgXrefGraph(false);
                    for (int i = 0; i < XrGraph.NumNodes; i++)
                    {
                        XrefGraphNode XrNode = XrGraph.GetXrefNode(i);
                        if (XrNode.Name == xrefName)
                        {
                            xrefList.Add(XrNode);
                            break;
                        }
                    }
                }
                if (xrefList.Count == 0)
                {
                    bTr = (BlockTableRecord)tr.GetObject(bT[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    foreach (var item in bTr)
                    {
                        if (item.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(T))))
                        {
                            output.Add((T)tr.GetObject(item, OpenMode.ForRead));
                        }
                    }
                }
                else
                {
                    foreach (var xref in xrefList)
                    {
                        btrList.Add((BlockTableRecord)tr.GetObject(xref.BlockTableRecordId, OpenMode.ForRead));
                    }
                    foreach (var btr in btrList)
                    {
                        foreach (var item in btr)
                        {
                            if (item.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(T))))
                            {
                                output.Add((T)tr.GetObject(item, OpenMode.ForRead));
                            }
                        }
                    }
                }
                tr.Commit();
            }
        }
        return output;
    }
    public static List<T> GetAllElementsOfTypeOnLayer<T>(string layer, string xrefName = null) where T : Entity
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        List<T> output = new();
        using (DocumentLock lk = doc.LockDocument())
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                var bT = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord bTr;
                XrefGraphNode xref;
                if (xrefName == null)
                {
                    bTr = (BlockTableRecord)tr.GetObject(bT[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                }
                else
                {
                    XrefGraph XrGraph = db.GetHostDwgXrefGraph(false);
                    xref = XrGraph.GetXrefNode(0);
                    for (int i = 1; i < XrGraph.NumNodes; i++)
                    {
                        XrefGraphNode XrNode = XrGraph.GetXrefNode(i);
                        if (XrNode.Name == xrefName)
                        {
                            xref = XrNode;
                            break;
                        }
                    }
                    bTr = (BlockTableRecord)tr.GetObject(xref.BlockTableRecordId, OpenMode.ForRead);
                }
                foreach (var item in bTr)
                {
                    if (item.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(T))))
                    {
                        var entity = (T)tr.GetObject(item, OpenMode.ForRead);
                        if (entity.Layer == layer)
                        {
                            output.Add(entity);
                        }
                    }
                }
                tr.Commit();
            }
        }
        return output;
    }
    //Method to get all layer names containing provided string
    public static List<string> GetAllLayersContainingString(string str)
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        List<string> output = new();
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            using (DocumentLock acLckDoc = doc.LockDocument())
            {
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord layer = (LayerTableRecord)tr.GetObject(item, OpenMode.ForRead);
                    if (layer.Name.Contains(str))
                    {
                        output.Add(layer.Name);
                    }
                }
            }
            tr.Commit();
        }
        return output;
    }
    //Propmpting user for insertion point
    public static Point3d GetInsertionPoint()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;
        PromptPointOptions pPtOpts = new("\nВыберете точку положения таблицы: ");
        return ed.GetPoint(pPtOpts).Value;
    }
    //Method to get all attributes from a block
    public static List<Dictionary<string, string>> GetAllAttributesFromBlockReferences(List<BlockReference> brList)
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        var output = new List<Dictionary<string, string>>();
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            using (DocumentLock acLckDoc = doc.LockDocument())
            {
                for (var i = 0; i < brList.Count; i++)
                {
                    output.Add(new Dictionary<string, string>());
                    foreach (ObjectId id in brList[i].AttributeCollection)
                    {
                        // open the attribute reference
                        var attRef = (AttributeReference)tr.GetObject(id, OpenMode.ForRead);
                        //Adding it to dictionary
                        output[i].Add(attRef.Tag, attRef.TextString);
                    }
                }
            }
            tr.Commit();
        }
        return output;
    }
    internal static List<XrefGraphNode> GetXRefList()
    {
        List<XrefGraphNode> output = new();
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        XrefGraph XrGraph = db.GetHostDwgXrefGraph(false);
        for (int i = 1; i < XrGraph.NumNodes; i++)
        {
            output.Add(XrGraph.GetXrefNode(i));
        }
        return output;
    }
    //Functions to check if something is inside polyline
    internal static PointContainment CheckIfObjectIsInsidePolyline(Polyline pl, Object obj)
    {
        Point3d pt = new Point3d(0, 0, 0);
        Curve cur = (Curve)pl;
        if (obj is DBText tx)
        { pt = tx.Position; }
        if (obj is BlockReference br)
        { pt = br.Position; }
        using (Region region = RegionFromClosedCurve(pl))
        {
            return GetPointContainment(region, pt);
        }
    }
    private static Region RegionFromClosedCurve(Curve curve)
    {
        if (!curve.Closed)
            throw new ArgumentException("Curve must be closed.");
        DBObjectCollection curves = new DBObjectCollection();
        curves.Add(curve);
        using (DBObjectCollection regions = Region.CreateFromCurves(curves))
        {
            if (regions == null || regions.Count == 0)
                throw new InvalidOperationException("Failed to create regions");
            if (regions.Count > 1)
                throw new InvalidOperationException("Multiple regions created");
            return regions.Cast<Region>().First();
        }
    }
    private static PointContainment GetPointContainment(Curve curve, Point3d point)
    {
        if (!curve.Closed)
            throw new ArgumentException("Curve must be closed.");
        Region region = RegionFromClosedCurve(curve);
        if (region == null)
            throw new InvalidOperationException("Failed to create region");
        using (region)
        { return GetPointContainment(region, point); }
    }
    private static PointContainment GetPointContainment(Region region, Point3d point)
    {
        PointContainment result = PointContainment.Outside;
        using (Brep brep = new Brep(region))
        {
            if (brep != null)
            {
                using (BrepEntity ent = brep.GetPointContainment(point, out result))
                {
                    if (ent is AcBr.Face)
                    {
                        result = PointContainment.Inside;
                    }
                }
            }
        }
        return result;
    }
}