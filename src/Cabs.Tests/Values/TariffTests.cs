using Cabs.Tests.Common.TestData;
using FluentAssertions;
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
