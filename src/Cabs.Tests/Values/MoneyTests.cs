using FluentAssertions;
using FluentAssertions.Execution;
using LegacyFighter.Cabs.Values;
using Xunit;

namespace Cabs.Tests.Values;

public class MoneyTests
{
    [Fact]
    public void CanCreateFromAndProjectToInteger()
    {
        using var scope = new AssertionScope();
        Money.Zero.IntValue.Should().Be(0);
        Money.OfValue(5).IntValue.Should().Be(5);
        Money.OfValue(15).IntValue.Should().Be(15);
        Money.OfValue(25).IntValue.Should().Be(25);
        Money.OfValue(35).IntValue.Should().Be(35);
    }

    [Fact]
    public void CanAddMoney()
    {
        using var scope = new AssertionScope();
        (Money.OfValue(5) + Money.OfValue(10)).Should().Be(Money.OfValue(15));
        (Money.OfValue(15) + Money.OfValue(10)).Should().Be(Money.OfValue(25));
        (Money.OfValue(25) + Money.OfValue(10)).Should().Be(Money.OfValue(35));
        (Money.OfValue(35) + Money.OfValue(10)).Should().Be(Money.OfValue(45));
    }

    [Fact]
    public void CanSubstractMoney()
    {
        using var scope = new AssertionScope();
        (Money.OfValue(10) - Money.OfValue(4)).Should().Be(Money.OfValue(6));
        (Money.OfValue(10) - Money.OfValue(5)).Should().Be(Money.OfValue(5));
        (Money.OfValue(10) - Money.OfValue(6)).Should().Be(Money.OfValue(4));
        (Money.OfValue(10) - Money.OfValue(7)).Should().Be(Money.OfValue(3));
    }

    [Fact]
    public void CanCalculatePercentage()
    {
        using var scope = new AssertionScope();
        Money.OfValue(200).Percentage(50).Should().Be(Money.OfValue(100));
        Money.OfValue(200).Percentage(25).Should().Be(Money.OfValue(50));
        Money.OfValue(200).Percentage(30).Should().Be(Money.OfValue(60));
        Money.OfValue(200).Percentage(33).Should().Be(Money.OfValue(66));
    }
}
