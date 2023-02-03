using System;

using Autodesk.AutoCAD.Geometry;

namespace Parking.Models;

internal class ParkingBuildingModel
{
    public string StageName { get; private set; }
    public string Name { get; private set; }
    public int MaxNumberOfParkingSpaces { get; private set; }
    public double TotalCommerceArea { get { return CommerceArea + OfficeArea + StoreArea; } }
    public string PlotNumber { get; private set; }
    public double CommerceArea { get; private set; }
    public double OfficeArea { get; private set; }
    public double StoreArea { get; private set; }
    public ParkingModel TotalParkingReq { get; private set; }
    public Point3d MidPoint { get; private set; }
    public ParkingBuildingModel(CityModel city, string[] parameters, ZoneBorderModel plot, Point3d midPoint)
    {
        StageName = parameters[0];
        Name = parameters[1];
        PlotNumber = plot.PlotNumber;
        MaxNumberOfParkingSpaces = Convert.ToInt32(parameters[4]);
        CommerceArea = Convert.ToDouble(parameters[5]);
        OfficeArea = Convert.ToDouble(parameters[6]);
        StoreArea = Convert.ToDouble(parameters[7]);
        TotalParkingReq = city.CalculateParking(Name, new double[] { 0, 0, 0, CommerceArea, OfficeArea, StoreArea }, new ParkingModel(Name, 0, 0, 0));
        MidPoint = midPoint;
    }
}
