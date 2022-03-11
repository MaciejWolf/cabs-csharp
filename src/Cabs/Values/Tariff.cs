using NodaTime;
using System.Globalization;

namespace LegacyFighter.Cabs.Values;

public record Tariff
{
    private const int BaseFee = 8;

    public string Name { get; }
    public float KmRate { get; }

    private readonly int _baseFee;
    private readonly int _factor;

    private Tariff(string name, float kmRate, int baseFee, int factor)
    {
        Name = name;
        KmRate = kmRate;
        _baseFee = baseFee;
        _factor = factor;
    }

    public static Tariff Create(LocalDateTime day, int? factor = null)
    {
        factor ??= 1;

        // wprowadzenie nowych cennikow od 1.01.2019
        if (day.Year <= 2018)
        {
            return StandardTariff(factor.Value);
        }

        var year = day.Year;
        var leap = ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0);

        if (((leap && day.DayOfYear == 366) || (!leap && day.DayOfYear == 365)) ||
            (day.DayOfYear == 1 && day.Hour <= 6))
        {
            return NewYearsEveTariff(factor.Value);
        }
        else
        {
            switch (day.DayOfWeek)
            {
                case IsoDayOfWeek.Monday:
                case IsoDayOfWeek.Tuesday:
                case IsoDayOfWeek.Wednesday:
                case IsoDayOfWeek.Thursday:
                    return StandardTariff(factor.Value);
                case IsoDayOfWeek.Friday:
                    if (day.Hour < 17)
                    {
                        return StandardTariff(factor.Value);
                    }
                    else
                    {
                        return WeekendPlusTariff(factor.Value);
                    }
                case IsoDayOfWeek.Saturday:
                    if (day.Hour < 6 || day.Hour >= 17)
                    {
                        return WeekendPlusTariff(factor.Value);
                    }
                    else
                    {
                        return WeekendTariff(factor.Value);
                    }
                case IsoDayOfWeek.Sunday:
                    if (day.Hour < 6)
                    {
                        return WeekendPlusTariff(factor.Value);
                    }
                    else
                    {
                        return WeekendTariff(factor.Value);
                    }

                default:
                    throw new ArgumentException();
            }
        }
    }

    private static Tariff StandardTariff(int factor) => new("Standard", 1.0f, 9, factor);
    private static Tariff NewYearsEveTariff(int factor) => new("Sylwester", 3.50f, 11, factor);
    private static Tariff WeekendTariff(int factor) => new("Weekend", 1.5f, 8, factor);
    private static Tariff WeekendPlusTariff(int factor) => new("Weekend+", 2.50f, 10, factor);


    public Money CalculateCost(Distance distance)
    {
        var pricedecimal = new decimal(distance.Km * KmRate * _factor + _baseFee);
        pricedecimal = decimal.Round(pricedecimal, 2, MidpointRounding.ToPositiveInfinity);
        var finalPrice = int.Parse(pricedecimal.ToString("0.00", CultureInfo.InvariantCulture).Replace(".", ""));
        return Money.OfValue(finalPrice);
    }

}
