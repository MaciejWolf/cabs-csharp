using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity.Miles;

public class AwardedMiles : BaseEntity
{
  public AwardedMiles()
  {
  }

  public AwardedMiles(AwardsAccount awardsAccount, Transit transit, Client client, Instant when, IMiles constantUntil) 
  {
    Account = awardsAccount;
    Transit = transit;
    Client = client;
    Date = when;
    Miles = constantUntil;
  }

  public void TransferTo(AwardsAccount account) 
  {
    Client = account.Client;
    Account = account;
  }

  public virtual Client Client { get; protected set; }

  private string MilesJson { get; set; }

  public IMiles Miles
  {
    get => MilesJsonMapper.Deserialize(MilesJson);
    private set => MilesJson = MilesJsonMapper.Serialize(value);
  }

  public int? GetMilesAmount(Instant when) 
  {
    return Miles.GetAmountFor(when);
  }

  public Instant Date { get; } = SystemClock.Instance.GetCurrentInstant();

  public Instant? ExpirationDate => Miles.ExpiresAt();

  public bool CantExpire => ExpirationDate.Value.ToUnixTimeTicks() == Instant.MaxValue.ToUnixTimeTicks();

  public virtual Transit Transit { get; }

  protected virtual AwardsAccount Account { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as AwardedMiles)?.Id;
  }

  public static bool operator ==(AwardedMiles left, AwardedMiles right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(AwardedMiles left, AwardedMiles right)
  {
    return !Equals(left, right);
  }

  public void RemoveAll(Instant forWhen) 
  {
    Miles = Miles.Subtract(GetMilesAmount(forWhen), forWhen);
  }

  public void Subtract(int? miles, Instant forWhen) 
  {
    Miles = Miles.Subtract(miles, forWhen);
  }
}