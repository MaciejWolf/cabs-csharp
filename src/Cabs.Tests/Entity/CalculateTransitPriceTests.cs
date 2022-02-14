using Cabs.Tests.Common;
using FluentAssertions;
using FluentAssertions.Execution;
using LegacyFighter.Cabs.Entity;
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
        ATransit(Transit.Statuses.Completed, Friday, 10).CalculateFinalCosts().Should().Be(1900);
        ATransit(Transit.Statuses.Completed, Friday, 20).CalculateFinalCosts().Should().Be(2900);
        ATransit(Transit.Statuses.Completed, Friday, 30).CalculateFinalCosts().Should().Be(3900);
        ATransit(Transit.Statuses.Completed, Friday, 40).CalculateFinalCosts().Should().Be(4900);
        ATransit(Transit.Statuses.Completed, Friday, 50).CalculateFinalCosts().Should().Be(5900);
    }

    [Fact]
    public void ShouldEstimatePrice()
    {
        using var _ = new AssertionScope();
        ATransit(Transit.Statuses.Draft, Friday, 10).EstimateCost().Should().Be(1900);
        ATransit(Transit.Statuses.Draft, Friday, 20).EstimateCost().Should().Be(2900);
        ATransit(Transit.Statuses.Draft, Friday, 30).EstimateCost().Should().Be(3900);
        ATransit(Transit.Statuses.Draft, Friday, 40).EstimateCost().Should().Be(4900);
        ATransit(Transit.Statuses.Draft, Friday, 50).EstimateCost().Should().Be(5900);
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
