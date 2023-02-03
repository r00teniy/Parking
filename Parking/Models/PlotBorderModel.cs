using Autodesk.AutoCAD.DatabaseServices;

namespace Parking.Models
{
    internal class PlotBorderModel
    {
        public string PlotNumber { get; private set; }
        public Polyline Polyline { get; private set; }
        public PlotBorderModel(string number, Polyline pl)
        {
            PlotNumber = number;
            Polyline = pl;
        }
    }
}
