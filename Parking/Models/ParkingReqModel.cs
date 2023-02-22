using System;
using System.Globalization;

namespace Parking.Models;

internal class ParkingReqModel
{
    public string Name { get; set; }
    public double LongParkingReqPeople { get; set; }
    public double LongParkingReqApartments { get; set; }
    public double LongParkingReqSqm { get; set; }
    public double BuildInShortParkingReqSqm { get; set; }
    public double GuestParkingReqPeople { get; set; }
    public double GuestParkingReqApartments { get; set; }
    public double GuestParkingReqSqm { get; set; }
    public double SchoolParkingReq { get; set; }
    public double KindergartenParkingReq { get; set; }
    public double HospitalParkingReq { get; set; }
    public double SportBuildingsParkingReq { get; set; }
    public double OfficesParkingReq { get; set; }
    public double StoreParkingReq { get; set; }
    public double PublicFoodParkingReq { get; set; }
    /*public string LongParkingFormula { get; set; }
    public string ShortParkingFormula { get; set; }
    public string GuestParkingFormula { get; set; }
    public double SchoolParkingFormula { get; set; }
    public double KindergartenParkingFormula { get; set; }
    public double HospitalParkingFormula { get; set; }
    public double SportBuildingsParkingFormula { get; set; }
    public double OfficesParkingFormula { get; set; }
    public double StoreParkingFormula { get; set; }
    public double PublicFoodParkingFormula { get; set; }*/

    public ParkingModel CalculateParking(string name, double[] parameters)
    {
        var parkLong = Convert.ToInt32(Math.Ceiling(LongParkingReqPeople * parameters[0] + LongParkingReqApartments * parameters[1] + LongParkingReqSqm * parameters[2]));
        var parkGuest = Convert.ToInt32(Math.Ceiling(GuestParkingReqPeople * parameters[0] + GuestParkingReqApartments * parameters[1] + GuestParkingReqSqm * parameters[2]));
        var parkShort = Convert.ToInt32(Math.Ceiling(BuildInShortParkingReqSqm * parameters[3] + OfficesParkingReq * parameters[4] + StoreParkingReq * parameters[5]));
        parkShort += Convert.ToInt32(Math.Ceiling(SchoolParkingReq * parameters[6]));
        parkShort += Convert.ToInt32(Math.Ceiling(KindergartenParkingReq * parameters[7]));
        parkShort += Convert.ToInt32(Math.Ceiling(HospitalParkingReq * parameters[8]));
        parkShort += Convert.ToInt32(Math.Ceiling(SportBuildingsParkingReq * parameters[9]));
        parkShort += Convert.ToInt32(Math.Ceiling(PublicFoodParkingReq * parameters[10]));
        ParkingModel parking = new(name, parkLong, parkShort, parkGuest);
        return parking;
    }

    public ParkingReqModel(string[] parameters)
    {
        LongParkingReqPeople = Convert.ToDouble(parameters[0], CultureInfo.InvariantCulture);
        LongParkingReqApartments = Convert.ToDouble(parameters[1], CultureInfo.InvariantCulture);
        LongParkingReqSqm = Convert.ToDouble(parameters[2], CultureInfo.InvariantCulture);
        BuildInShortParkingReqSqm = Convert.ToDouble(parameters[3], CultureInfo.InvariantCulture);
        GuestParkingReqPeople = Convert.ToDouble(parameters[4], CultureInfo.InvariantCulture);
        GuestParkingReqApartments = Convert.ToDouble(parameters[5], CultureInfo.InvariantCulture);
        GuestParkingReqSqm = Convert.ToDouble(parameters[6], CultureInfo.InvariantCulture);
        SchoolParkingReq = Convert.ToDouble(parameters[7], CultureInfo.InvariantCulture);
        KindergartenParkingReq = Convert.ToDouble(parameters[8], CultureInfo.InvariantCulture);
        HospitalParkingReq = Convert.ToDouble(parameters[9], CultureInfo.InvariantCulture);
        SportBuildingsParkingReq = Convert.ToDouble(parameters[10], CultureInfo.InvariantCulture);
        PublicFoodParkingReq = Convert.ToDouble(parameters[11], CultureInfo.InvariantCulture);
        OfficesParkingReq = Convert.ToDouble(parameters[12], CultureInfo.InvariantCulture);
        StoreParkingReq = Convert.ToDouble(parameters[13], CultureInfo.InvariantCulture);
        Name = parameters[14];
    }
}
