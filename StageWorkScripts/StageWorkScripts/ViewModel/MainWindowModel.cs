using System.Collections.Generic;

using StageWorkScripts.Models;

namespace StageWorkScripts.ViewModel;
internal class MainWindowModel
{
    internal List<CurbModel> CurbTypes { get; set; }
    internal List<IPavement> PavementTypes { get; set; }
    public List<IGreenery> GreeneryType { get; set; }
    //public List<StreetFurnitureModel> FurnitureType { get; set; }


}
