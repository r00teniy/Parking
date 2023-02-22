using Autodesk.AutoCAD.Runtime;

using Parking.Forms;

[assembly: CommandClass(typeof(Parking.MyCommands))]

namespace Parking
{
    internal class MyCommands
    {
        [CommandMethod("ParkNew")]
        static public void ParkNew()
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
        }
    }
}