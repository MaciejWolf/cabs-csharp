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
    [MemberData(nameof(DatesWithTariffs))]
    public async Task ShouldDisplayValidTariff(LocalDateTime date, string expectedName, float expectedKmRate)
    {
        // Arrange
        var transit = await Fixtures.ATransit(60, date);

        // Act
        var dto = await TransitService.LoadTransit(transit.Id);

        // Assert
        dto.Tariff.Should().Be(expectedName);
        dto.KmRate.Should().Be(expectedKmRate);
    }

    public static IEnumerable<object[]> DatesWithTariffs()
    {
        var standard = Dates.WithStandardTariff.Select(HasTariffAndKmRate("Standard", 1.0f));
        var weekend = Dates.WithWeekendTariff.Where(x => x.Hour != 16).Select(HasTariffAndKmRate("Weekend", 1.5f));
        var weekendPlus = Dates.WithWeekendPlusTariff.Where(x => x.Hour != 5).Select(HasTariffAndKmRate("Weekend+", 2.50f));
        var newYearsEve = Dates.WithNewYearsEveTariff.Select(HasTariffAndKmRate("Sylwester", 3.50f));

        return standard.Concat(weekend).Concat(weekendPlus).Concat(newYearsEve);
    }

    public static Func<LocalDateTime, object[]> HasTariffAndKmRate(string tariff, float kmRate) => date => new object[] { date, tariff, kmRate };
}
