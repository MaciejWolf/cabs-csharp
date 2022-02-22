using FluentAssertions;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using NodaTime;
using NodaTime.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Ui;

public class CalculateTransitDistanceTests
{
    [Fact]
    public void ShouldThrowExceptionWhenUnitIsInvalid()
    {
        var act = () => TransitForDistanceOfKm(50).GetDistance("invalid");
        act.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void ShouldPresentAsKm()
    {
        TransitForDistanceOfKm(50).GetDistance("km").Should().Be("50km");
        TransitForDistanceOfKm(150).GetDistance("km").Should().Be("150km");
        TransitForDistanceOfKm(55.7f).GetDistance("km").Should().Be("55.700km");
        TransitForDistanceOfKm(0).GetDistance("km").Should().Be("0km");
    }

    [Fact]
    public void ShouldPresentAsMeters()
    {
        TransitForDistanceOfKm(50).GetDistance("m").Should().Be("50000m");
        TransitForDistanceOfKm(150).GetDistance("m").Should().Be("150000m");
        TransitForDistanceOfKm(55.7f).GetDistance("m").Should().Be("55700m");
        TransitForDistanceOfKm(0).GetDistance("m").Should().Be("0m");
    }

    [Fact]
    public void ShouldPresentAsMiles()
    {
        TransitForDistanceOfKm(10).GetDistance("miles").Should().Be("6.214miles");
        TransitForDistanceOfKm(10.123f).GetDistance("miles").Should().Be("6.290miles");
        TransitForDistanceOfKm(10.12345f).GetDistance("miles").Should().Be("6.290miles");
        TransitForDistanceOfKm(0).GetDistance("miles").Should().Be("0miles");
    }

    private static TransitDto TransitForDistanceOfKm(float km)
    {
        var transit = new Transit
        {
            DateTime = SystemClock.Instance.InUtc().GetCurrentInstant(),
            Km = km,
            To = new Address(),
            From = new Address(),
            Status = Transit.Statuses.Draft,
            Client = new Client()
        };
        var dto = new TransitDto(transit);
        return dto;
    }
}
