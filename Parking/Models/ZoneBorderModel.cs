using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;

namespace Parking.Models
{
    internal class ZoneBorderModel
    {
        public string Name { get; private set; }
        public string PlotNumber { get; set; }
        public Polyline Polyline { get; private set; }
        public ZoneBorderModel(string name, Polyline pl)
        {
            Name = name;
            Polyline = pl;
        }
    }
}
