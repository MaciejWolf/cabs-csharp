using Cabs.Tests.Common;
using Cabs.Tests.Common.TestData;
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
        var standard = Dates.WithStandardTariff.Select(HasTariffName("Standard"));
        var weekend = Dates.WithWeekendTariff.Select(HasTariffName("Weekend"));
        var weekendPlus = Dates.WithWeekendPlusTariff.Select(HasTariffName("Weekend+"));
        var newYearsEve = Dates.WithNewYearsEveTariff.Select(HasTariffName("Sylwester"));

        return standard.Concat(weekend).Concat(weekendPlus).Concat(newYearsEve);
    }

    public static Func<Instant, object[]> HasTariffName(string tariff) => date => new object[] { date, tariff };
}
