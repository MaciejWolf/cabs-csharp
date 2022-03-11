using NodaTime;
using System.Globalization;

namespace LegacyFighter.Cabs.Values;

public record Tariff
{
    private const int BaseFee = 8;

    public string Name { get; }
    public float KmRate { get; }

    private readonly int _baseFee;

    private Tariff(string name, float kmRate, int baseFee)
    {
        Name = name;
        KmRate = kmRate;
        _baseFee = baseFee;
    }

    public static Tariff Create(LocalDateTime date)
    {
        var (name, kmRate) = GetTariff(date);
        var baseFee = CalculateBaseFee(date);

        return new Tariff(name, kmRate, baseFee);
    }

    public Money CalculateCost(Distance distance)
    {
        int? factorToCalculate = 1;
        //var factorToCalculate = Factor;
        //if (factorToCalculate == null)
        //{
        //    factorToCalculate = 1;
        //}
 
        var pricedecimal = new decimal(distance.Km * KmRate * factorToCalculate.Value + _baseFee);
        pricedecimal = decimal.Round(pricedecimal, 2, MidpointRounding.ToPositiveInfinity);
        var finalPrice = int.Parse(pricedecimal.ToString("0.00", CultureInfo.InvariantCulture).Replace(".", ""));
        return Money.OfValue(finalPrice);
    }

    private static (string Name, float KmRate) Standard => ("Standard", 1.0f);
    private static (string Name, float KmRate) NewYearsEve => ("Sylwester", 3.50f);
    private static (string Name, float KmRate) Weekend => ("Weekend", 1.5f);
    private static (string Name, float KmRate) WeekendPlus => ("Weekend+", 2.50f);

    private static (string Name, float KmRate) GetTariff(LocalDateTime day)
    {
        // wprowadzenie nowych cennikow od 1.01.2019
        if (day.Year <= 2018)
        {
            return Standard;
        }

        var year = day.Year;
        var leap = ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0);

        if (((leap && day.DayOfYear == 366) || (!leap && day.DayOfYear == 365)) ||
            (day.DayOfYear == 1 && day.Hour <= 6))
        {
            return NewYearsEve;
        }
        else
        {
            switch (day.DayOfWeek)
            {
                case IsoDayOfWeek.Monday:
                case IsoDayOfWeek.Tuesday:
                case IsoDayOfWeek.Wednesday:
                case IsoDayOfWeek.Thursday:
                    return Standard;
                case IsoDayOfWeek.Friday:
                    if (day.Hour < 17)
                    {
                        return Standard;
                    }
                    else
                    {
                        return WeekendPlus;
                    }
                case IsoDayOfWeek.Saturday:
                    if (day.Hour < 6 || day.Hour >= 17)
                    {
                        return WeekendPlus;
                    }
                    else/* if (day.Hour < 17)*/
                    {
                        return Weekend;
                    }
                case IsoDayOfWeek.Sunday:
                    if (day.Hour < 6)
                    {
                        return WeekendPlus;
                    }
                    else
                    {
                        return Weekend;
                    }

                default:
                    return (default, default);
            }
        }
    }

    private static int CalculateBaseFee(LocalDateTime day)
    {
        var baseFee = BaseFee;

        if (day.Year <= 2018)
        {
            baseFee++;
        }
        else
        {
            if ((day.Month == 12 && day.Day == 31) ||
                (day.Month == 1 && day.Day == 1 && day.Hour <= 6))
            {
                baseFee += 3;
            }
            else
            {
                // piątek i sobota po 17 do 6 następnego dnia
                if ((day.DayOfWeek == IsoDayOfWeek.Friday && day.Hour >= 17) ||
                    (day.DayOfWeek == IsoDayOfWeek.Saturday && day.Hour <= 6) ||
                    (day.DayOfWeek == IsoDayOfWeek.Saturday && day.Hour >= 17) ||
                    (day.DayOfWeek == IsoDayOfWeek.Sunday && day.Hour <= 6))
                {
                    baseFee += 2;
                }
                else
                {
                    // pozostałe godziny weekendu
                    if ((day.DayOfWeek == IsoDayOfWeek.Saturday && day.Hour > 6 && day.Hour < 17) ||
                        (day.DayOfWeek == IsoDayOfWeek.Sunday && day.Hour > 6))
                    {

                    }
                    else
                    {
                        // tydzień roboczy
                        baseFee++;
                    }
                }
            }
        }

        return baseFee;
    }
}
