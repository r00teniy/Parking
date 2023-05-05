using Autodesk.AutoCAD.Runtime;

using Parking.Forms;

[assembly: CommandClass(typeof(Parking.MyCommands))]

namespace Parking
{
    internal class MyCommands
    {
        [CommandMethod("CalculateParking")]
        static public void CalculateParking()
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
        }
    }
}