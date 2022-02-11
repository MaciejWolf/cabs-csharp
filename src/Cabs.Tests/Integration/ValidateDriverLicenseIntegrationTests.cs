using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Integration;

public class ValidateDriverLicenseIntegrationTests : IAsyncLifetime
{
    private CabsApp _app = default!;

    public Task InitializeAsync()
    {
        _app = CabsApp.CreateInstance();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public async Task CannotCreateActiveDriverWithInvalidLicense(string license)
    {
        var act = async () => await CreateActiveDriver(license);
        await act.Should().ThrowExactlyAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public async Task CanCreateInactiveDriverWithInvalidLicense(string license)
    {
        var driver = await CreateInactiveDriver(license);
        var dto = await Load(driver);

        dto.DriverLicense.Should().Be(license);
        dto.Status.Should().Be(Driver.Statuses.Inactive);
    }

    [Theory]
    [InlineData(Driver.Statuses.Active)]
    [InlineData(Driver.Statuses.Inactive)]
    public async Task CannotCreateDriverWithNullLicense(Driver.Statuses status)
    {
        var act = async () => await CreateDriver(null, status);
        await act.Should().ThrowAsync<Exception>();
    }

    [Theory]
    [InlineData(Driver.Statuses.Active)]
    [InlineData(Driver.Statuses.Inactive)]
    public async Task CanCreateDriverWithValidLicense(Driver.Statuses status)
    {
        const string ValidLicense = "FARME100165AB5EW";

        var driver = await CreateDriver(ValidLicense, status);
        var dto = await Load(driver);

        dto.DriverLicense.Should().Be(ValidLicense);
        dto.Status.Should().Be(status);
    }

    private Task<Driver> CreateActiveDriver(string license) 
        => CreateDriver(license, Driver.Statuses.Active);

    private Task<Driver> CreateInactiveDriver(string license)
        => CreateDriver(license, Driver.Statuses.Inactive);

    private Task<Driver> CreateDriver(string license, Driver.Statuses status)
        => _app.DriverService.CreateDriver(
            license,
            "Kowalski",
            "Jan",
            Driver.Types.Regular,
            status,
            null);

    private Task<DriverDto> Load(Driver driver)
        => _app.DriverService.LoadDriver(driver.Id);
}
