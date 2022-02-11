using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class AwardsAccountDto
{
    public AwardsAccountDto()
    {
    }

    public AwardsAccountDto(AwardsAccount account)
    {
        Active = account.Active;
        Client = new ClientDto(account.Client);
        Transactions = account.Transactions;
        Date = account.Date;
    }

    public ClientDto Client { set; get; }

    public Instant Date { set; get; }

    public bool Active { set; get; }

    public int Transactions { get; set; }
}