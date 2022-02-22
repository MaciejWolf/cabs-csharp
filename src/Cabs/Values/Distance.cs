using System.Globalization;

namespace LegacyFighter.Cabs.Values;

public record Distance
{
    private const float KmToMilesRatio = 1.609344f;

    public float Km { get; private set; }

    private Distance(float km)
    {
        Km = km;
    }

    public static Distance OfKm(float km)
    {
        return new Distance(km);
    }

    public string GetDistanceAs(string unit)
    {
        var usCulture = CultureInfo.CreateSpecificCulture("en-US");
        if (unit == "km")
        {
            if (Km == Math.Ceiling(Km))
            {
                return Math.Round(Km).ToString(usCulture) + "km";

            }

            return Km.ToString("0.000", usCulture) + "km";
        }

        if (unit == "miles")
        {
            var distance = Km / KmToMilesRatio;
            if (distance == Math.Ceiling(distance))
            {
                return Math.Round(distance).ToString(usCulture) + "miles";
            }

            return distance.ToString("0.000", usCulture) + "miles";
        }

        if (unit == "m")
        {
            return Math.Round(Km * 1000).ToString(usCulture) + "m";
        }

        throw new ArgumentException("Invalid unit " + unit);
    }
}
