﻿using NodaTime;
using System.Collections.Generic;

namespace Cabs.Tests.Common.TestData
{
    public static class Dates
    {
        public static LocalDateTime Friday => new(2021, 4, 16, 8, 30);

        public static LocalDateTime FridayNight => new(2021, 4, 16, 19, 30);

        public static LocalDateTime Saturday => new(2021, 4, 17, 8, 30);

        public static LocalDateTime SaturdayNight => new(2021, 4, 17, 19, 30);

        public static LocalDateTime Sunday => new(2021, 4, 18, 8, 30);

        public static LocalDateTime Before2019 => new(2018, 1, 1, 8, 30);

        public static LocalDateTime NewYearsEve => new(2021, 12, 31, 8, 30);

        public static IEnumerable<LocalDateTime> WithStandardTariff =>
        new List<LocalDateTime>
        {
            Friday,
            Before2019
        };

        public static IEnumerable<LocalDateTime> WithWeekendPlusTariff =>
            new List<LocalDateTime>
            {
                FridayNight,
                SaturdayNight
            };

        public static IEnumerable<LocalDateTime> WithWeekendTariff =>
            new List<LocalDateTime>
            {
                Saturday,
                Sunday
            };

        public static IEnumerable<LocalDateTime> WithNewYearsEveTariff =>
            new List<LocalDateTime>
            {
                NewYearsEve
            };
    }
}
