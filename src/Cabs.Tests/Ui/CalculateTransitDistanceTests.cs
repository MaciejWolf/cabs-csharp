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
        var act = () => TransitForDistance(50).GetDistance("invalid");
        act.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void ShouldPresentAsKm()
    {
        TransitForDistance(50).GetDistance("km").Should().Be("50km");
        TransitForDistance(150).GetDistance("km").Should().Be("150km");
        TransitForDistance(55.7f).GetDistance("km").Should().Be("55.700km");
        TransitForDistance(0).GetDistance("km").Should().Be("0km");
    }

    [Fact]
    public void ShouldPresentAsMiles()
    {
        TransitForDistance(10).GetDistance("miles").Should().Be("6.214miles");
        TransitForDistance(10.123f).GetDistance("miles").Should().Be("6.290miles");
        TransitForDistance(10.12345f).GetDistance("miles").Should().Be("6.290miles");
        TransitForDistance(0).GetDistance("miles").Should().Be("0miles");
    }

    private static TransitDto TransitForDistance(float km)
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
