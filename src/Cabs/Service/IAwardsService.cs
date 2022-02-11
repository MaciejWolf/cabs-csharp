using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface IAwardsService
{
    Task<AwardsAccountDto> FindBy(long? clientId);
    Task RegisterToProgram(long? clientId);
    Task ActivateAccount(long? clientId);
    Task DeactivateAccount(long? clientId);
    Task<AwardedMiles> RegisterMiles(long? clientId, long? transitId);
    Task<AwardedMiles> RegisterSpecialMiles(long? clientId, int miles);
    Task RemoveMiles(long? clientId, int miles);
    Task<int> CalculateBalance(long? clientId);
    Task TransferMiles(long? fromClientId, long? toClientId, int miles);
}