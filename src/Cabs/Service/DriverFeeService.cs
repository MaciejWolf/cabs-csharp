using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Values;

namespace LegacyFighter.Cabs.Service;

public class DriverFeeService : IDriverFeeService
{
    private readonly IDriverFeeRepository _driverFeeRepository;
    private readonly ITransitRepository _transitRepository;

    public DriverFeeService(IDriverFeeRepository driverFeeRepository, ITransitRepository transitRepository)
    {
        _driverFeeRepository = driverFeeRepository;
        _transitRepository = transitRepository;
    }

    public async Task<Money> CalculateDriverFee(long? transitId)
    {
        var transit = await _transitRepository.Find(transitId);
        if (transit == null)
        {
            throw new ArgumentException("transit does not exist, id = " + transitId);
        }

        if (transit.DriversFee != null)
        {
            return transit.DriversFee;
        }

        var transitPrice = transit.Price;
        var driverFee = await _driverFeeRepository.FindByDriver(transit.Driver);
        if (driverFee == null)
        {
            throw new ArgumentException("driver Fees not defined for driver, driver id = " +
                                               transit.Driver.Id);
        }

        return driverFee.Calculate(transitPrice);
    }
}