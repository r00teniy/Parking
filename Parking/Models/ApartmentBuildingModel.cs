using System;


using Autodesk.AutoCAD.Geometry;

namespace Parking.Models;
internal class ApartmentBuildingModel
{
    // Buildimg part
    public string StageName { get; private set; }
    public string Name { get; private set; }
    public string PlotNumber { get; private set; }
    public Point3d MidPoint { get; private set; }
    // Apartment part
    public int TotalResidents { get; private set; }
    public int TotalNumberOfApartments { get; private set; }
    public double TotalApartmentArea { get; private set; }
    // commerce part
    public double TotalCommerceArea { get { return CommerceArea + OfficeArea + StoreArea; } }
    public double CommerceArea { get; private set; }
    public double OfficeArea { get; private set; }
    public double StoreArea { get; private set; }
    // Parking
    public ParkingModel TotalParkingReq { get; private set; }
    public ParkingModel ExParking { get; private set; }

    public ApartmentBuildingModel(CityModel city, string[] buildingParams, ZoneBorderModel plot, ParkingModel exParking, Point3d midPoint)
    {
        StageName = buildingParams[0];
        Name = buildingParams[1];
        MidPoint = midPoint;
        TotalNumberOfApartments = Convert.ToInt32(buildingParams[4]);
        TotalApartmentArea = Convert.ToDouble(buildingParams[5]);
        CommerceArea = Convert.ToDouble(buildingParams[6]);
        OfficeArea = Convert.ToDouble(buildingParams[7]);
        StoreArea = Convert.ToDouble(buildingParams[8]);
        PlotNumber = plot.PlotNumber;
        ExParking = exParking;
        TotalResidents = Convert.ToInt32(Math.Floor(TotalApartmentArea / city.SqMPerPerson));
        //Parking
        TotalParkingReq = city.CalculateParking(Name, new double[] { TotalResidents, TotalApartmentArea, TotalNumberOfApartments, OfficeArea, StoreArea }, exParking);
    }
}
