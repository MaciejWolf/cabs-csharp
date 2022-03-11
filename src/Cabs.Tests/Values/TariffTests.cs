using Cabs.Tests.Common.TestData;
using FluentAssertions;
using FluentAssertions.Execution;
using LegacyFighter.Cabs.Values;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Cabs.Tests.Values;

public class TariffTests
{
    [Theory]
    [MemberData(nameof(DatesWithTariffs))]
    public void ShouldHaveValidTariffName(LocalDateTime date, string expectedName, float expectedKmRate)
    {
        var tariff = Tariff.Create(date);
        tariff.Name.Should().Be(expectedName);
        tariff.KmRate.Should().Be(expectedKmRate);
    }

    [Fact]
    public void ShouldEstimatePrice()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.Friday).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(1900));
        Tariff.Create(Dates.Friday).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(2900));
        Tariff.Create(Dates.Friday).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(3900));
        Tariff.Create(Dates.Friday).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(4900));
        Tariff.Create(Dates.Friday).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(5900));
    }

    [Fact]
    public void ShouldCalculatePriceForSaturday()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(2300));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(3800));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(5300));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(6800));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(8300));
    }

    [Fact]
    public void ShouldCalculatePriceForSaturdayNight()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.SaturdayNight).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(3500));
        Tariff.Create(Dates.SaturdayNight).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(6000));
        Tariff.Create(Dates.SaturdayNight).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(8500));
        Tariff.Create(Dates.SaturdayNight).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(11000));
        Tariff.Create(Dates.SaturdayNight).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(13500));
    }

    [Fact]
    public void ShouldCalculatePriceForSunday()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.Sunday).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(2300));
        Tariff.Create(Dates.Sunday).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(3800));
        Tariff.Create(Dates.Sunday).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(5300));
        Tariff.Create(Dates.Sunday).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(6800));
        Tariff.Create(Dates.Sunday).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(8300));
    }

    [Fact]
    public void ShouldCalculatePriceForNewYearsEve()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(4600));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(8100));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(11600));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(15100));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(18600));
    }

    [Fact]
    public void ShouldCalculateUsingStandardPriceBefore2019()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.Before2019).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(1900));
        Tariff.Create(Dates.Before2019).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(2900));
        Tariff.Create(Dates.Before2019).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(3900));
        Tariff.Create(Dates.Before2019).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(4900));
        Tariff.Create(Dates.Before2019).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(5900));
    }

    public static IEnumerable<object[]> DatesWithTariffs()
    {
        var standard = Dates.WithStandardTariff.Select(HasTariffAndKmRate("Standard", 1.0f));
        var weekend = Dates.WithWeekendTariff.Select(HasTariffAndKmRate("Weekend", 1.5f));
        var weekendPlus = Dates.WithWeekendPlusTariff.Select(HasTariffAndKmRate("Weekend+", 2.50f));
        var newYearsEve = Dates.WithNewYearsEveTariff.Select(HasTariffAndKmRate("Sylwester", 3.50f));

        return standard.Concat(weekend).Concat(weekendPlus).Concat(newYearsEve);
    }

    public static Func<LocalDateTime, object[]> HasTariffAndKmRate(string tariff, float kmRate) => date => new object[] { date, tariff, kmRate };
}
