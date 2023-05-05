using Autodesk.AutoCAD.Runtime;

using StageWorkScripts.Forms;

[assembly: CommandClass(typeof(StageWorkScripts.MyCommands))]

namespace StageWorkScripts;
internal class MyCommands
{
    [CommandMethod("StageWorkScripts")]
    static public void StageWorkScripts()
    {
        //var variables = SettingsStorage.ReadSettingsFromXML();
        var variables = new Variables();
        //variables.SavedData = SettingsStorage.ReadData();
        var MW = new MainWindow(variables);
        MW.Show();
    }

}