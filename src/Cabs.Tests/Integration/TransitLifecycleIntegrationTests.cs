using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Integration;

public class TransitLifecycleIntegrationTests : IAsyncLifetime
{
    private readonly CabsApp _app;

    private ITransitService TransitService => _app.TransitService;
    private Fixtures Fixtures => _app.Fixtures;

    private Mock<IGeocodingService> _getcodingService;

    private Client _client;

    public TransitLifecycleIntegrationTests()
    {
        _getcodingService = new Mock<IGeocodingService>();

        _getcodingService
            .Setup(x => x.GeocodeAddress(It.IsAny<Address>()))
            .Returns(new[] { 1d, 1d });

        _app = CabsApp.CreateInstance(services =>
        {
            services.AddSingleton(_getcodingService.Object);
        });
    }

    public async Task InitializeAsync()
    {
        _client = await Fixtures.AClient();
    }

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    [Fact]
    public async Task ShouldCreateTransitFromDto()
    {
        // Arrange
        var addresses = await GenerateAddresses(2);
        var dto = TransitDto(addresses[0], addresses[1]);

        // Act
        var transit = await TransitService.CreateTransit(dto);

        // Assert
        var loaded = await LoadTransit();
        loaded.Should().BeEquivalentTo(new TransitDto(transit));
    }

    [Fact]
    public async Task ShouldChangeTransitPickupAddress()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        await CreateTransit(addresses[0], addresses[1]);

        // Act
        await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        var loaded = await LoadTransit();
        loaded.From.Should().BeEquivalentTo(new AddressDto(addresses[2]));
    }

    [Fact]
    public async Task CanChangePickupAddressThreeTimes()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        var transit = await CreateTransit(addresses[0], addresses[1]);

        await ChangeTransitAddressFrom(addresses[2]);
        await ChangeTransitAddressFrom(addresses[1]);
        await ChangeTransitAddressFrom(addresses[2]);

        // Act
        var act = async () => await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>("Address 'from' cannot be changed, id = " + transit.Id);
    }

    [Fact]
    public async Task CannotChangePickupAddressTooFarAway()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        AddressIsFarAway(addresses[2]);

        var transit = await CreateTransit(addresses[0], addresses[1]);

        // Act
        var act = async () => await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>("Address 'from' cannot be changed, id = " + transit.Id);
    }

    [Fact]
    public async Task CannotChangePickupAddressOfCancelledTransit()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        var transit = await CreateTransit(addresses[0], addresses[1]);
        await CancelTransit();

        // Act
        var act = async () => await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>("Address 'from' cannot be changed, id = " + transit.Id);
    }

    [Fact]
    public async Task CannotChangeDestinationAfterOfCompletedTransit()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        var transit = await CreateTransit(addresses[0], addresses[1]);
        await CancelTransit();

        // Act
        var act = async () => await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>("Address 'from' cannot be changed, id = " + transit.Id);
    }



    [Fact]
    public async Task ShouldChangeTransitAddressTo()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);

        var client = await Fixtures.AClient();

    }

    public async Task CanCancelTransit()
    {

    }

    private Task<Transit> CreateTransit(Address from, Address to)
        => TransitService.CreateTransit(TransitDto(from, to));

    private Task ChangeTransitAddressFrom(Address address)
        => TransitService.ChangeTransitAddressFrom(_client.Id, address);

    private Task CancelTransit()
        => TransitService.CancelTransit(_client.Id);

    private Task PublishTransit()
        => TransitService.PublishTransit(_client.Id);

    private Task<Transit> FindDriversForTransit()
        => TransitService.FindDriversForTransit(_client.Id);



    private Task<TransitDto> LoadTransit()
        => TransitService.LoadTransit(_client.Id);

    private TransitDto TransitDto(Address from, Address to)
    {
        var dto = new TransitDto
        {
            From = new AddressDto(from),
            To = new AddressDto(to),
            ClientDto = new ClientDto
            {
                Id = _client.Id
            }
        };

        return dto;
    }

    private Task<Address[]> GenerateAddresses(int count)
    {
        return Task.WhenAll(GenerateAddressTasks(count));
    }

    private IEnumerable<Task<Address>> GenerateAddressTasks(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return Fixtures.AnAddress("PL", "Warsaw", "Dolna", i);
        }
    }

    private void AddressIsFarAway(Address address)
    {
        _getcodingService
            .Setup(x => x.GeocodeAddress(It.Is<Address>(a => a == address)))
            .Returns(new[] { 1000d, 1000d });
    }
}
