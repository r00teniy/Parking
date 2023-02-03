using System;
using System.Globalization;

namespace Parking.Models;
internal class CityModel
{
    public string Name { get; private set; }
    public double SqMPerPerson { get; private set; }
    public string LongParkingFormula { get; private set; }
    public string GuestParkingFormula { get; private set; }
    public string OfficeParkingFormula { get; private set; }
    public string StoreParkingFormula { get; private set; }

    public CityModel(string longParkingFormula, string guestParkingFormula, string officesParkingFormula, string storeParkingFormula, string name, string sqMPerPerson)
    {
        LongParkingFormula = longParkingFormula;
        GuestParkingFormula = guestParkingFormula;
        OfficeParkingFormula = officesParkingFormula;
        StoreParkingFormula = storeParkingFormula;
        Name = name;
        SqMPerPerson = Convert.ToDouble(sqMPerPerson.Replace(',', '.'), CultureInfo.InvariantCulture);
    }

    public ParkingModel CalculateParking(string name, double[] parameters, ParkingModel exParking)
    {
        string longParkingFormulaWithData = ReplaceDataInFormula(LongParkingFormula, parameters, exParking, 0);
        var parkLong = (int)Math.Ceiling(Functions.DataProcessing.CalculateSimpleFormula(longParkingFormulaWithData));
        string guestParkingFormulaWithData = ReplaceDataInFormula(GuestParkingFormula, parameters, exParking, parkLong);
        var parkGuest = (int)Math.Ceiling(Functions.DataProcessing.CalculateSimpleFormula(guestParkingFormulaWithData));
        int parkShort = 0;
        if (parameters[4] != 0)
        {
            string storeParkingFormulaWithData = ReplaceDataInFormula(StoreParkingFormula, parameters, exParking, parkLong);
            parkShort += (int)Math.Ceiling(Functions.DataProcessing.CalculateSimpleFormula(storeParkingFormulaWithData));
        }
        if (parameters[3] != 0)
        {
            string officeParkingFormulaWithData = ReplaceDataInFormula(OfficeParkingFormula, parameters, exParking, parkLong);
            parkShort += (int)Math.Ceiling(Functions.DataProcessing.CalculateSimpleFormula(officeParkingFormulaWithData));
        }
        return new ParkingModel(name, parkLong, parkShort, parkGuest);
    }
    private string ReplaceDataInFormula(string formula, double[] parameters, ParkingModel exParking, int parkLong)
    {
        return formula.Replace("КолЖит", parameters[0].ToString()).Replace("КолКварт", parameters[2].ToString()).Replace("ПлКварт", parameters[1].ToString()).Replace("ПлОфис", parameters[3].ToString()).Replace("ПлМагаз", parameters[4].ToString()).Replace("ПостСтНаУч", exParking.TotalLongParking.ToString()).Replace("ТребПостСт", parkLong.ToString());
    }
}
