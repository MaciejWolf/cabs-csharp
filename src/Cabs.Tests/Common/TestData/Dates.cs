using NodaTime;

namespace Cabs.Tests.Common.TestData
{
    public static class Dates
    {
        public static Instant Friday => new LocalDateTime(2021, 4, 16, 8, 30).InUtc().ToInstant();

        public static Instant FridayNight => new LocalDateTime(2021, 4, 16, 19, 30).InUtc().ToInstant();

        public static Instant Saturday => new LocalDateTime(2021, 4, 17, 8, 30).InUtc().ToInstant();

        public static Instant SaturdayNight => new LocalDateTime(2021, 4, 17, 19, 30).InUtc().ToInstant();

        public static Instant Sunday => new LocalDateTime(2021, 4, 18, 8, 30).InUtc().ToInstant();

        public static Instant Before2019 => new LocalDateTime(2018, 1, 1, 8, 30).InUtc().ToInstant();

        public static Instant NewYearsEve => new LocalDateTime(2021, 12, 31, 8, 30).InUtc().ToInstant();
    }
}
