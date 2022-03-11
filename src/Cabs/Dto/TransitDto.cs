using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Values;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class TransitDto
{
    public DriverDto Driver;

    public int? Factor;

    private Distance _distance;

    private string _distanceUnit;

    private decimal _baseFee;

    private Instant? _date;

    public TransitDto()
    {

    }

    public TransitDto(Transit transit)
    {
        Id = transit.Id;
        _distance = Distance.OfKm(transit.Km);
        Factor = transit.Factor;
        if (transit.Price != null)
        {
            Price = new decimal(transit.Price.IntValue);
        }

        _date = transit.DateTime;
        Status = transit.Status;
        Tariff = transit.Tariff.Name;
        KmRate = transit.Tariff.KmRate;
        foreach (var d in transit.ProposedDrivers)
        {
            ProposedDrivers.Add(new DriverDto(d));
        }
        To = new AddressDto(transit.To);
        From = new AddressDto(transit.From);
        CarClass = transit.CarType;
        ClientDto = new ClientDto(transit.Client);
        if (transit.DriversFee != null)
        {
            DriverFee = new decimal(transit.DriversFee.IntValue);
        }

        if (transit.EstimatedPrice != null)
        {
            EstimatedPrice = new decimal(transit.EstimatedPrice.IntValue);
        }

        DateTime = transit.DateTime;
        Published = transit.Published;
        AcceptedAt = transit.AcceptedAt;
        Started = transit.Started;
        CompleteAt = transit.CompleteAt;

    }

    public float KmRate { get; private set; }

    public string Tariff { get; private set; }

    public string GetDistance(string unit)
    {
        _distanceUnit = unit;
        return _distance.GetDistanceAs(unit);
    }

    public List<DriverDto> ProposedDrivers { get; set; } = new();
    public ClaimDto ClaimDto { get; set; }
    public AddressDto To { get; set; }
    public AddressDto From { get; set; }
    public CarType.CarClasses? CarClass { get; set; }
    public ClientDto ClientDto { get; set; }
    public long? Id { get; }
    public Transit.Statuses? Status { get; set; }
    public decimal? Price { get; }
    public decimal? DriverFee { get; set; }
    public Instant? DateTime { get; set; }
    public Instant? Published { get; set; }
    public Instant? AcceptedAt { get; set; }
    public Instant? Started { get; set; }
    public Instant? CompleteAt { get; set; }
    public decimal EstimatedPrice { get; set; }
}