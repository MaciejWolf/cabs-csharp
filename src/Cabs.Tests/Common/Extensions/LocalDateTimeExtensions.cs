using NodaTime;

namespace Cabs.Tests.Common.Extensions;

public static class LocalDateTimeExtensions
{
    public static LocalDateTime At(this LocalDateTime date, int hour, int minute)
        => date.With(x => new LocalTime(hour, minute));
}
