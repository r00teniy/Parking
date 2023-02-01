using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

[assembly: ExtensionApplication(typeof(Parking.MyPlugin))]

namespace Parking
{
    internal class MyPlugin : IExtensionApplication

    {
        void IExtensionApplication.Initialize()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Program loaded \n");
        }
        void IExtensionApplication.Terminate()
        {

        }
    }
}