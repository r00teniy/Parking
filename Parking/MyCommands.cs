﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: CommandClass(typeof(Parking.MyCommands))]

namespace Parking
{
    internal class MyCommands
    {
        [CommandMethod("Command")]
        static public void Command()
        {

        }
    }
}