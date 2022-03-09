using Cabs.Tests.Common;
using Cabs.Tests.Common.TestData;
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
        ATransit(Transit.Statuses.Completed, Dates.Friday, 10).CalculateFinalCosts().Should().Be(Money.OfValue(1900));
        ATransit(Transit.Statuses.Completed, Dates.Friday, 20).CalculateFinalCosts().Should().Be(Money.OfValue(2900));
        ATransit(Transit.Statuses.Completed, Dates.Friday, 30).CalculateFinalCosts().Should().Be(Money.OfValue(3900));
        ATransit(Transit.Statuses.Completed, Dates.Friday, 40).CalculateFinalCosts().Should().Be(Money.OfValue(4900));
        ATransit(Transit.Statuses.Completed, Dates.Friday, 50).CalculateFinalCosts().Should().Be(Money.OfValue(5900));
    }

    [Fact]
    public void ShouldEstimatePrice()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Dates.Friday, 10).EstimateCost().Should().Be(Money.OfValue(1900));
        ATransit(Transit.Statuses.Draft, Dates.Friday, 20).EstimateCost().Should().Be(Money.OfValue(2900));
        ATransit(Transit.Statuses.Draft, Dates.Friday, 30).EstimateCost().Should().Be(Money.OfValue(3900));
        ATransit(Transit.Statuses.Draft, Dates.Friday, 40).EstimateCost().Should().Be(Money.OfValue(4900));
        ATransit(Transit.Statuses.Draft, Dates.Friday, 50).EstimateCost().Should().Be(Money.OfValue(5900));
    }

    [Fact]
    public void ShouldCalculatePriceForSaturday()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Dates.Saturday, 10).EstimateCost().Should().Be(Money.OfValue(2300));
        ATransit(Transit.Statuses.Draft, Dates.Saturday, 20).EstimateCost().Should().Be(Money.OfValue(3800));
        ATransit(Transit.Statuses.Draft, Dates.Saturday, 30).EstimateCost().Should().Be(Money.OfValue(5300));
        ATransit(Transit.Statuses.Draft, Dates.Saturday, 40).EstimateCost().Should().Be(Money.OfValue(6800));
        ATransit(Transit.Statuses.Draft, Dates.Saturday, 50).EstimateCost().Should().Be(Money.OfValue(8300));
    }

    [Fact]
    public void ShouldCalculatePriceForSaturdayNight()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Dates.SaturdayNight, 10).EstimateCost().Should().Be(Money.OfValue(3500));
        ATransit(Transit.Statuses.Draft, Dates.SaturdayNight, 20).EstimateCost().Should().Be(Money.OfValue(6000));
        ATransit(Transit.Statuses.Draft, Dates.SaturdayNight, 30).EstimateCost().Should().Be(Money.OfValue(8500));
        ATransit(Transit.Statuses.Draft, Dates.SaturdayNight, 40).EstimateCost().Should().Be(Money.OfValue(11000));
        ATransit(Transit.Statuses.Draft, Dates.SaturdayNight, 50).EstimateCost().Should().Be(Money.OfValue(13500));
    }

    [Fact]
    public void ShouldCalculatePriceForSunday()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Dates.Sunday, 10).EstimateCost().Should().Be(Money.OfValue(2300));
        ATransit(Transit.Statuses.Draft, Dates.Sunday, 20).EstimateCost().Should().Be(Money.OfValue(3800));
        ATransit(Transit.Statuses.Draft, Dates.Sunday, 30).EstimateCost().Should().Be(Money.OfValue(5300));
        ATransit(Transit.Statuses.Draft, Dates.Sunday, 40).EstimateCost().Should().Be(Money.OfValue(6800));
        ATransit(Transit.Statuses.Draft, Dates.Sunday, 50).EstimateCost().Should().Be(Money.OfValue(8300));
    }

    [Fact]
    public void ShouldCalculatePriceForNewYearsEve()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Dates.NewYearsEve, 10).EstimateCost().Should().Be(Money.OfValue(4600));
        ATransit(Transit.Statuses.Draft, Dates.NewYearsEve, 20).EstimateCost().Should().Be(Money.OfValue(8100));
        ATransit(Transit.Statuses.Draft, Dates.NewYearsEve, 30).EstimateCost().Should().Be(Money.OfValue(11600));
        ATransit(Transit.Statuses.Draft, Dates.NewYearsEve, 40).EstimateCost().Should().Be(Money.OfValue(15100));
        ATransit(Transit.Statuses.Draft, Dates.NewYearsEve, 50).EstimateCost().Should().Be(Money.OfValue(18600));
    }

    [Fact]
    public void ShouldCalculateUsingStandardPriceBefore2019()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Dates.Before2019, 10).EstimateCost().Should().Be(Money.OfValue(1900));
        ATransit(Transit.Statuses.Draft, Dates.Before2019, 20).EstimateCost().Should().Be(Money.OfValue(2900));
        ATransit(Transit.Statuses.Draft, Dates.Before2019, 30).EstimateCost().Should().Be(Money.OfValue(3900));
        ATransit(Transit.Statuses.Draft, Dates.Before2019, 40).EstimateCost().Should().Be(Money.OfValue(4900));
        ATransit(Transit.Statuses.Draft, Dates.Before2019, 50).EstimateCost().Should().Be(Money.OfValue(5900));
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
}
