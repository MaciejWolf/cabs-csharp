using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Service;
using NodaTime;
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


    [Fact]
    public async Task WeekendPlusTariffShouldBeDisplayed()
    {
        // Arrange
        var transit = await Fixtures.ATransit(60, SaturdayNight);

        // Act
        var dto = await TransitService.LoadTransit(transit.Id);

        // Assert
        dto.Tariff.Should().Be("WeekendPlus");
    }

    [Fact]
    public async Task StandardTariffShouldBeDisplayed()
    {

    }

    [Fact]
    public async Task StandardTariffShouldBeDisplayedBefore2019()
    {

    }


    private static Instant Friday => new LocalDateTime(2021, 4, 16, 8, 30).InUtc().ToInstant();

    private static Instant Saturday => new LocalDateTime(2021, 4, 17, 8, 30).InUtc().ToInstant();

    private static Instant SaturdayNight => new LocalDateTime(2021, 4, 17, 19, 30).InUtc().ToInstant();

    private static Instant Sunday => new LocalDateTime(2021, 4, 18, 8, 30).InUtc().ToInstant();

    private static Instant Before2019 => new LocalDateTime(2018, 1, 1, 8, 30).InUtc().ToInstant();

    private static Instant NewYearsEve => new LocalDateTime(2021, 12, 31, 8, 30).InUtc().ToInstant();
}
