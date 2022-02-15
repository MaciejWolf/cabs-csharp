using LegacyFighter.Cabs.Values;

namespace LegacyFighter.Cabs.Service;

public interface IDriverFeeService
{
    Task<Money> CalculateDriverFee(long? transitId);
}