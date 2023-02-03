using System;
using System.Collections.Generic;


namespace Parking.Models;

internal class ParkingModel
{
    public string Name { get; private set; }
    public string PlotNumber { get; private set; }
    public int TotalLongParking { get; private set; }
    public int TotalShortParking { get; private set; }
    public int TotalGuestParking { get; private set; }
    public int TotalDisabledParking { get { return ShortDisabled + GuestDisabled; } }
    public int TotalDisabledBigParking { get { return ShortDisabledBig + GuestDisabledBig; } }
    public int ShortDisabled { get; private set; }
    public int ShortDisabledBig { get; private set; }
    public int GuestDisabled { get; private set; }
    public int GuestDisabledBig { get; private set; }
    public ParkingModel(List<ParkingBlockModel> list, string plotNumber = null, string name = null)
    {
        if (plotNumber != null)
        {
            PlotNumber = plotNumber;
        }
        if (name != null)
        {
            Name = name;
        }
        foreach (var item in list)
        {
            if (item != null)
            {
                switch (item.Type)
                {
                    case "Long":
                        TotalLongParking += item.NumberOfParkings;
                        break;
                    case "Short":
                        TotalShortParking += item.NumberOfParkings;
                        if (item.IsForDisabled == true)
                        {
                            ShortDisabled += item.NumberOfParkings;
                            if (item.IsForDisabledExtended == true)
                            { ShortDisabledBig += item.NumberOfParkings; }
                        }
                        break;
                    case "Guest":
                        TotalGuestParking += item.NumberOfParkings;
                        if (item.IsForDisabled == true)
                        {
                            GuestDisabled += item.NumberOfParkings;
                            if (item.IsForDisabledExtended == true)
                            { GuestDisabledBig += item.NumberOfParkings; }
                        }
                        break;
                }
            }
        }
    }
    public ParkingModel(string name, List<ParkingModel> parking)
    {
        Name = name;
        foreach (var par in parking)
        {
            if (par != null)
            {
                TotalLongParking += par.TotalLongParking;
                TotalShortParking += par.TotalShortParking;
                TotalGuestParking += par.TotalGuestParking;
                ShortDisabled += par.ShortDisabled;
                ShortDisabledBig += par.ShortDisabledBig;
                GuestDisabled += par.GuestDisabled;
                GuestDisabledBig += par.GuestDisabledBig;
            }
        }
    }
    //This one is just for calculating requirements
    public ParkingModel(string name, int parkLong, int parkShort, int parkGuest)
    {
        Name = name;
        TotalLongParking = parkLong;
        TotalShortParking = parkShort;
        TotalGuestParking = parkGuest;
        ShortDisabled = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(parkShort) / 10));
        ShortDisabledBig = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(parkShort) / 20));
        GuestDisabled = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(parkGuest) / 10));
        GuestDisabledBig = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(parkGuest) / 20));
    }
}
