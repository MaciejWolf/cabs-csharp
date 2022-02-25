using Cabs.Tests.Common;
using FluentAssertions;
using FluentAssertions.Execution;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Values;
using NodaTime;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Entity;

public class CalculateTransitPriceTests : IAsyncLifetime
{
    private readonly CabsApp _app = CabsApp.CreateInstance();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    [Fact]
    public void ShouldCalculatePrice()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Completed, Friday, 10).CalculateFinalCosts().Should().Be(Money.OfValue(1900));
        ATransit(Transit.Statuses.Completed, Friday, 20).CalculateFinalCosts().Should().Be(Money.OfValue(2900));
        ATransit(Transit.Statuses.Completed, Friday, 30).CalculateFinalCosts().Should().Be(Money.OfValue(3900));
        ATransit(Transit.Statuses.Completed, Friday, 40).CalculateFinalCosts().Should().Be(Money.OfValue(4900));
        ATransit(Transit.Statuses.Completed, Friday, 50).CalculateFinalCosts().Should().Be(Money.OfValue(5900));
    }

    [Fact]
    public void ShouldEstimatePrice()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Friday, 10).EstimateCost().Should().Be(Money.OfValue(1900));
        ATransit(Transit.Statuses.Draft, Friday, 20).EstimateCost().Should().Be(Money.OfValue(2900));
        ATransit(Transit.Statuses.Draft, Friday, 30).EstimateCost().Should().Be(Money.OfValue(3900));
        ATransit(Transit.Statuses.Draft, Friday, 40).EstimateCost().Should().Be(Money.OfValue(4900));
        ATransit(Transit.Statuses.Draft, Friday, 50).EstimateCost().Should().Be(Money.OfValue(5900));
    }

    [Fact]
    public void ShouldCalculatePriceForSaturday()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Saturday, 10).EstimateCost().Should().Be(Money.OfValue(2300));
        ATransit(Transit.Statuses.Draft, Saturday, 20).EstimateCost().Should().Be(Money.OfValue(3800));
        ATransit(Transit.Statuses.Draft, Saturday, 30).EstimateCost().Should().Be(Money.OfValue(5300));
        ATransit(Transit.Statuses.Draft, Saturday, 40).EstimateCost().Should().Be(Money.OfValue(6800));
        ATransit(Transit.Statuses.Draft, Saturday, 50).EstimateCost().Should().Be(Money.OfValue(8300));
    }

    [Fact]
    public void ShouldCalculatePriceForSaturdayNight()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, SaturdayNight, 10).EstimateCost().Should().Be(Money.OfValue(3500));
        ATransit(Transit.Statuses.Draft, SaturdayNight, 20).EstimateCost().Should().Be(Money.OfValue(6000));
        ATransit(Transit.Statuses.Draft, SaturdayNight, 30).EstimateCost().Should().Be(Money.OfValue(8500));
        ATransit(Transit.Statuses.Draft, SaturdayNight, 40).EstimateCost().Should().Be(Money.OfValue(11000));
        ATransit(Transit.Statuses.Draft, SaturdayNight, 50).EstimateCost().Should().Be(Money.OfValue(13500));
    }

    [Fact]
    public void ShouldCalculatePriceForSunday()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Sunday, 10).EstimateCost().Should().Be(Money.OfValue(2300));
        ATransit(Transit.Statuses.Draft, Sunday, 20).EstimateCost().Should().Be(Money.OfValue(3800));
        ATransit(Transit.Statuses.Draft, Sunday, 30).EstimateCost().Should().Be(Money.OfValue(5300));
        ATransit(Transit.Statuses.Draft, Sunday, 40).EstimateCost().Should().Be(Money.OfValue(6800));
        ATransit(Transit.Statuses.Draft, Sunday, 50).EstimateCost().Should().Be(Money.OfValue(8300));
    }

    [Fact]
    public void ShouldCalculatePriceForNewYearsEve()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, NewYearsEve, 10).EstimateCost().Should().Be(Money.OfValue(4600));
        ATransit(Transit.Statuses.Draft, NewYearsEve, 20).EstimateCost().Should().Be(Money.OfValue(8100));
        ATransit(Transit.Statuses.Draft, NewYearsEve, 30).EstimateCost().Should().Be(Money.OfValue(11600));
        ATransit(Transit.Statuses.Draft, NewYearsEve, 40).EstimateCost().Should().Be(Money.OfValue(15100));
        ATransit(Transit.Statuses.Draft, NewYearsEve, 50).EstimateCost().Should().Be(Money.OfValue(18600));
    }

    [Fact]
    public void ShouldCalculateUsingStandardPriceBefore2019()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Before2019, 10).EstimateCost().Should().Be(Money.OfValue(1900));
        ATransit(Transit.Statuses.Draft, Before2019, 20).EstimateCost().Should().Be(Money.OfValue(2900));
        ATransit(Transit.Statuses.Draft, Before2019, 30).EstimateCost().Should().Be(Money.OfValue(3900));
        ATransit(Transit.Statuses.Draft, Before2019, 40).EstimateCost().Should().Be(Money.OfValue(4900));
        ATransit(Transit.Statuses.Draft, Before2019, 50).EstimateCost().Should().Be(Money.OfValue(5900));
    }

    private static Transit ATransit(Transit.Statuses status, Instant date, int km)
    {
        var transit = new Transit
        {
            DateTime = date,
            Status = Transit.Statuses.Draft,
            Km = km
        };
        transit.Status = status;
        return transit;
    }

    private static Instant Friday => new LocalDateTime(2021, 4, 16, 8, 30).InUtc().ToInstant();

    private static Instant Saturday => new LocalDateTime(2021, 4, 17, 8, 30).InUtc().ToInstant();

    private static Instant SaturdayNight => new LocalDateTime(2021, 4, 17, 19, 30).InUtc().ToInstant();

    private static Instant Sunday => new LocalDateTime(2021, 4, 18, 8, 30).InUtc().ToInstant();

    private static Instant Before2019 => new LocalDateTime(2018, 1, 1, 8, 30).InUtc().ToInstant();

    private static Instant NewYearsEve => new LocalDateTime(2021, 12, 31, 8, 30).InUtc().ToInstant();
}
