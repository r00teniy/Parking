using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;

namespace Parking.Functions
{
    internal class DataImport
    {
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
        //Prompting user to select an object
        public static ObjectId? GetObjectIdOfEntity<T>(string type) where T : Entity
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            var options = new PromptEntityOptions($"\nSelect {type}: ");
            options.SetRejectMessage($"\nSelected object isn't {type}");
            options.AddAllowedClass(typeof(T), true);
            var result = ed.GetEntity(options);
            if (result.Status == PromptStatus.OK)
            {
                return result.ObjectId;
            }
            return null;
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
    }
}
