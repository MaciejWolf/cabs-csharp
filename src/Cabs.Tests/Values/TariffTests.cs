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

    public static IEnumerable<object[]> DatesWithStandardTariff => Dates.WithStandardTariff.Select(x => new object[] { x });

    [Theory]
    [MemberData(nameof(DatesWithStandardTariff))]
    public void ShouldCalculateWithStandardTariff(LocalDateTime date)
    {
        using var _ = new AssertionScope();
        Tariff.Create(date).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(1900));
        Tariff.Create(date).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(2900));
        Tariff.Create(date).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(3900));
        Tariff.Create(date).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(4900));
        Tariff.Create(date).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(5900));
    }

    [Fact]
    public void ShouldCalculatePriceWithWeekendTariff()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(2300));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(3800));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(5300));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(6800));
        Tariff.Create(Dates.Saturday).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(8300));
    }

    [Theory]
    [InlineData(10, 3500)]
    [InlineData(20, 6000)]
    [InlineData(30, 8500)]
    [InlineData(40, 11000)]
    [InlineData(50, 13500)]
    public void ShouldCalculatePriceWithWeekendPlusTariff(float km, int cost) 
        => Tariff.Create(Dates.WithWeekendPlusTariff.First()).CalculateCost(Distance.OfKm(km)).Should().Be(Money.OfValue(cost));

    [Fact]
    public void ShouldCalculatePriceWithNewYearsEveTariff()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(10)).Should().Be(Money.OfValue(4600));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(20)).Should().Be(Money.OfValue(8100));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(11600));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(40)).Should().Be(Money.OfValue(15100));
        Tariff.Create(Dates.NewYearsEve).CalculateCost(Distance.OfKm(50)).Should().Be(Money.OfValue(18600));
    }

    [Fact]
    public void ShouldCalculateWithFactor()
    {
        using var _ = new AssertionScope();
        Tariff.Create(Dates.Before2019, 0).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(900));
        Tariff.Create(Dates.Before2019, 1).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(3900));
        Tariff.Create(Dates.Before2019, 2).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(6900));
        Tariff.Create(Dates.Before2019, 3).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(9900));
        Tariff.Create(Dates.Before2019, 4).CalculateCost(Distance.OfKm(30)).Should().Be(Money.OfValue(12900));
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
