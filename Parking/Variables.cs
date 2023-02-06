using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using Parking.Models;

namespace Parking
{
    internal static class Variables
    {
        internal static string apartmentsBuildingsLayer = "21_Жилые_дома";
        internal static string parkingBuildingsLayer = "25_Паркинги";
        internal static string buildingBorderLayer = "13_Граница_";
        internal static string stageBorderLayer = "12_Граница_";
        internal static string plotNumbersLayer = "10_Номера_участков";
        internal static string plotsBorderLayer = "19_Кад_участки";
        internal static string parkTableStyleName = "ГП Таблица паркомест";
        internal static RXClass rxClassBlockReference = RXClass.GetClass(typeof(BlockReference));
        internal static RXClass rxClassPolyline = RXClass.GetClass(typeof(Polyline));
        internal static RXClass rxClassText = RXClass.GetClass(typeof(DBText));
        internal static List<ParkingReqModel> parkingCalcTypeList = new List<ParkingReqModel>();
        //Parameters_of_dyn_blocks
        internal static string[] parkingBlockPararmArray = { "Размер", "Тип", "Обычн_МГН", "РасширенноеММ" };
        //Arrays with data for table
        internal static string[] parkingTypes = { "Постоянные", "Временные", "Гостевые", "Всего", "в т.ч. расш." };
        internal static short[] parkingTableColors = { 6, 30, 33, 135, 63, 13, 85, 2, 3, 1, 4, 200, 5, 181, 140, 244, 21, 161, 230, 214, 184, 94, 66, 41, 155, 71, 211, 27, 175, 241 };
        internal static BindingList<CityModel> cityList = new BindingList<CityModel>();
        internal static string[] whereToFind = { "в этом файле", "во внешней ссылке", "везде" };
    }
}

