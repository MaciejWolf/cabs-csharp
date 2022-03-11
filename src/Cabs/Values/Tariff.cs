using NodaTime;

namespace LegacyFighter.Cabs.Values;

public record Tariff
{
    public string Name { get; }
    public float KmRate { get; }

    private Tariff(string name, float kmRate)
    {
        Name = name;
        KmRate = kmRate;
    }

    public static Tariff Create(LocalDateTime date)
    {
        var (name, kmRate) = GetTariff(date);

        return new Tariff(name, kmRate);
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
}
