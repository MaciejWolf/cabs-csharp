using System.Linq;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.Cabs.Values;
using NodaTime;
using System.Threading.Tasks;
using LegacyFighter.Cabs.Dto;

namespace Cabs.Tests.Common;

public class Fixtures
{
    private readonly ICarTypeService carTypeService;
    private readonly IDriverFeeRepository driverFeeRepository;
    private readonly ITransitRepository transitRepository;
    private readonly IDriverService driverService;
    private readonly IClientRepository clientRepository;
    private readonly AddressRepository addressRepository;

    public Fixtures(
        IDriverFeeRepository driverFeeRepository,
        ICarTypeService carTypeService,
        ITransitRepository transitRepository,
        IDriverService driverService, 
        IClientRepository clientRepository,
        AddressRepository addressRepository)
    {
        this.driverFeeRepository = driverFeeRepository;
        this.carTypeService = carTypeService;
        this.transitRepository = transitRepository;
        this.driverService = driverService;
        this.clientRepository = clientRepository;
        this.addressRepository = addressRepository;
    }

    public Task<Client> AClient() => clientRepository.Save(new Client());

    public Task<Address> AnAddress(string country, string city, string street, int buildingNumber)
    {
        return addressRepository.Save(new Address(country, city, street, buildingNumber));
    }

    public Task<Driver> ADriver()
        => driverService.CreateDriver(
            "FARME100165AB5EW",
            "Kowalski",
            "Jan",
            Driver.Types.Regular,
            Driver.Statuses.Active,
            "xxx");

    public async Task<CarType> AnActiveCarCategory(CarType.CarClasses carClass)
    {
        var carTypeDto = new CarTypeDto
        {
            CarClass = carClass,
            Description = "opis"
        };
        var carType = await carTypeService.Create(carTypeDto);
        foreach (var _ in Enumerable.Range(1, carType.MinNoOfCarsToActivateClass))
        {
            await carTypeService.RegisterCar(carType.CarClass);
        }
        await carTypeService.Activate(carType.Id);
        return carType;
    }
    
    public Task<DriverFee> DriverHasFee(
        Driver driver,
        DriverFee.FeeTypes feeType,
        int amount) => CreateDriverFee(driver, feeType, amount, 0);

    public Task<DriverFee> CreateDriverFee(
        Driver driver,
        DriverFee.FeeTypes feeType,
        int amount) => CreateDriverFee(driver, feeType, amount, 0);

    public Task<DriverFee> CreateDriverFee(
        Driver driver,
        DriverFee.FeeTypes feeType,
        int amount,
        int min)
    {
        var fee = new DriverFee(feeType, driver, amount, Money.OfValue(min));
        return driverFeeRepository.Save(fee);
    }

    public Task<Transit> TransitOn(Driver driver, int price, int year, Month month, int day)
        => transitRepository.Save(new Transit
        {
            Driver = driver,
            Price = Money.OfValue(price),
            DateTime = new LocalDate(year, month.Value, day)
                .AtStartOfDayInZone(DateTimeZoneProviders.Bcl.GetSystemDefault())
                .ToInstant()
        });

    public async Task<Transit> ATransit(int price, LocalDateTime when) 
        => await transitRepository.Save(new Transit
    {
        Client = await AClient(),
        From = await AnAddress("PL", "Warsaw", "Aleje Jerozolimskie", 96),
        To = await AnAddress("PL", "Warsaw", "Inna", 9),
        Price = Money.OfValue(price),
        DateTime = when.InUtc().ToInstant()
    });

    public Task<Transit> TransitWithFee(Driver driver, int fee)
        => transitRepository.Save(new Transit
        {
            Driver = driver,
            DriversFee = Money.OfValue(fee),
            DateTime = AnyDate
        });

    public Task<Transit> TransitWithPrice(Driver driver, int price)
        => transitRepository.Save(new Transit
        {
            Driver = driver,
            Price = Money.OfValue(price),
            DateTime = AnyDate
        });

    private static Instant AnyDate => new LocalDate(2018, Month.January.Value, 1)
                .AtStartOfDayInZone(DateTimeZoneProviders.Bcl.GetSystemDefault())
                .ToInstant();
}
