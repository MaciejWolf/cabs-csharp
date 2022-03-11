using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Values;
using NodaTime;
using System.Globalization;

namespace LegacyFighter.Cabs.Entity;

public class Transit : BaseEntity
{


    public Transit()
    {
    }

    public enum Statuses
    {
        Draft,
        Cancelled,
        WaitingForDriverAssignment,
        DriverAssignmentFailed,
        TransitToPassenger,
        InTransit,
        Completed
    }

    public enum DriverPaymentStatuses
    {
        NotPaid,
        Paid,
        Claimed,
        Returned
    }

    public enum ClientPaymentStatuses
    {
        NotPaid,
        Paid,
        Returned
    }

    private DriverPaymentStatuses? DriverPaymentStatus { get; set; }
    private ClientPaymentStatuses? ClientPaymentStatus { get; set; }
    private Client.PaymentTypes? PaymentType { get; set; }
    public Instant? Date { get; private set; }
    public int? Factor { get; set; }
    private Distance _distance = Distance.OfKm(0);

    public CarType.CarClasses? CarType { get; set; }
    public virtual Driver Driver { get; set; }

    public Money Price
    {
        get;
        set; //just for testing
    }

    private LocalDateTime CurrentDate => DateTime.Value.InZone(DateTimeZoneProviders.Bcl.GetSystemDefault()).LocalDateTime;

    public Tariff Tariff => Tariff.Create(CurrentDate, Factor);

    public Statuses? Status { get; set; }

    public Instant? CompleteAt { get; private set; }

    public Money EstimateCost()
    {
        if (Status == Statuses.Completed)
        {
            throw new InvalidOperationException("Estimating cost for completed transit is forbidden, id = " + Id);
        }

        EstimatedPrice = Tariff.CalculateCost(_distance);
        Price = null;

        return EstimatedPrice;
    }

    public virtual Client Client { get; set; }

    public Money CalculateFinalCosts()
    {
        if (Status == Statuses.Completed)
        {
            Price = Tariff.CalculateCost(_distance);
            return Price;
        }
        else
        {
            throw new InvalidOperationException("Cannot calculate final cost if the transit is not completed");
        }
    }

    public Instant? DateTime { set; get; }

    public Instant? Published { get; set; }

    public float Km
    {
        get => _distance.Km;
        set
        {
            _distance = Distance.OfKm(value);
            EstimateCost();
        }
    }

    public int AwaitingDriversResponses { get; set; } = 0;
    public virtual ISet<Driver> DriversRejections { get; set; } = new HashSet<Driver>();
    public virtual ISet<Driver> ProposedDrivers { get; set; } = new HashSet<Driver>();
    public Instant? AcceptedAt { get; set; }
    public Instant? Started { get; set; }
    public virtual Address From { get; set; }
    public virtual Address To { get; set; }

    public int PickupAddressChangeCounter { get; set; } = 0;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj != null && Id != null && Id == (obj as Transit)?.Id;
    }

    public static bool operator ==(Transit left, Transit right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Transit left, Transit right)
    {
        return !Equals(left, right);
    }

    public void CompleteTransitAt(Instant when)
    {
        CompleteAt = when;
    }

    public Money DriversFee { get; set; }

    public Money EstimatedPrice { get; set; }
}