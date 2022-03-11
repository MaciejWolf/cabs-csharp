using FluentAssertions;
using LegacyFighter.Cabs.Values;
using System;
using Xunit;

namespace Cabs.Tests.Entity;

public class DriverLicenseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public void CannotCreateInvalidDriverLicense(string license)
    {
        var act = () => DriverLicense.WithLicense(license);
        act.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void CanCreateValidDriverLicense()
    {
        const string ValidLicense = "FARME100165AB5EW";

        var driverLicense = DriverLicense.WithLicense(ValidLicense);

        driverLicense.ValueAsString.Should().Be(ValidLicense);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public void CanCreateInvalidDriverLicenseExplicitly(string license)
    {
        var driverLicense = DriverLicense.WithoutValidation(license);

        driverLicense.ValueAsString.Should().Be(license);
    }
}
