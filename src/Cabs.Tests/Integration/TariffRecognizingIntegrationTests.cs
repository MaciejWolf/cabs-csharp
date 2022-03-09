using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Service;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Integration;

public class TariffRecognizingIntegrationTests : IAsyncLifetime
{
    private readonly CabsApp _app = CabsApp.CreateInstance();
    private Fixtures Fixtures => _app.Fixtures;
    private ITransitService TransitService => _app.TransitService;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    [Theory]
    [MemberData(nameof(DatesAndTariffs))]
    public async Task ShouldDisplayValidTariff(Instant date, string expectedTariff)
    {
        // Arrange
        var transit = await Fixtures.ATransit(60, date);

        // Act
        var dto = await TransitService.LoadTransit(transit.Id);

        // Assert
        dto.Tariff.Should().Be(expectedTariff);
    }

    public static IEnumerable<object[]> DatesAndTariffs()
    {
        var standard = DatesWithStandardTariff.Select(HasTariff("Standard"));
        var weekend = DatesWithWeekendTariff.Select(HasTariff("Weekend"));
        var weekendPlus = DatesWithWeekendPlusTariff.Select(HasTariff("Weekend+"));
        var newYearsEve = DatesWithNewYearsEveTariff.Select(HasTariff("Sylwester"));

        return standard.Concat(weekend).Concat(weekendPlus).Concat(newYearsEve);
    }

    public static IEnumerable<object[]> DatesWithStandardTariff =>
        new List<object[]>
        {
            new object[] { Friday },
            new object[] { Before2019 }
        };

    public static IEnumerable<object[]> DatesWithWeekendPlusTariff =>
        new List<object[]>
        {
            new object[] { FridayNight },
            new object[] { SaturdayNight }
        };

    public static IEnumerable<object[]> DatesWithWeekendTariff =>
        new List<object[]>
        {
            new object[] { Saturday },
            new object[] { Sunday }
        };

    public static IEnumerable<object[]> DatesWithNewYearsEveTariff =>
        new List<object[]>
        {
            new object[] { NewYearsEve }
        };


    private static Func<object[], object[]> HasTariff(string tariff) => x => new[] { x[0], tariff };

    private static Instant Friday => new LocalDateTime(2021, 4, 16, 8, 30).InUtc().ToInstant();
    private static Instant FridayNight => new LocalDateTime(2021, 4, 16, 19, 30).InUtc().ToInstant();
    private static Instant Saturday => new LocalDateTime(2021, 4, 17, 8, 30).InUtc().ToInstant();
    private static Instant SaturdayNight => new LocalDateTime(2021, 4, 17, 19, 30).InUtc().ToInstant();
    private static Instant Sunday => new LocalDateTime(2021, 4, 18, 8, 30).InUtc().ToInstant();
    private static Instant Before2019 => new LocalDateTime(2018, 1, 1, 8, 30).InUtc().ToInstant();
    private static Instant NewYearsEve => new LocalDateTime(2021, 12, 31, 8, 30).InUtc().ToInstant();
}
