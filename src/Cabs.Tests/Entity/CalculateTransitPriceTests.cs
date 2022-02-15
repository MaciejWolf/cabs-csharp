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
}
