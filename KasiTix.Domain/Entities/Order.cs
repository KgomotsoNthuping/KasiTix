namespace KasiTix.Domain.Entities;
public enum OrderStatus { Confirmed, Cancelled }
public class Order
{
 private readonly List<OrderLine> _lines = new();

 public Guid Id { get; private set; }

 public Guid EventId { get; private set; }

 public string BuyerEmail { get; private set; } = null!;

 public string IdempotencyKey { get; private set; } = null!;

 public OrderStatus Status { get; private set; }

 public DateTime CreatedAt { get; private set; }

 public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

 public decimal Total => _lines.Sum(l => l.Quantity * l.UnitPrice);

    public Order(Guid eventId, string buyerEmail, string idempotencyKey)
    {
        if (eventId == Guid.Empty)
        throw new ArgumentException("An order must belong to an event.", nameof(eventId));

        if (string.IsNullOrWhiteSpace(buyerEmail))
        throw new ArgumentException("Buyer email is required.", nameof(buyerEmail));

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        throw new ArgumentException("An idempotency key is required.", nameof(idempotencyKey));

        Id = Guid.NewGuid();
        EventId = eventId;
        BuyerEmail = buyerEmail;
        IdempotencyKey = idempotencyKey;
        Status = OrderStatus.Confirmed;
        CreatedAt = DateTime.UtcNow;
    }
 // TODO (R6, R10): add a line for this ticket type.
 // - an order holds at most 5 lines
 // - the same ticket type can't appear twice
 // - the line keeps the price the ticket type has RIGHT NOW
 // Should this method also call ticketType.Reserve(quantity)? Decide, and be
 // ready to defend your choice.
 // public void AddLine(TicketType ticketType, int quantity) => throw new NotImplementedException();
    public void AddLine(TicketType ticketType, int quantity)
    {
        if (_lines.Count >= 5)
        {
            throw new ArgumentException(
                "An order can have at most 5 lines.");
        }

        if (_lines.Any(line =>
                line.TicketTypeId ==
                ticketType.Id))
        {
            throw new ArgumentException(
                "The same ticket type cannot appear twice in one order.");
        }

        if (quantity is < 1 or > 10)
        {
            throw new ArgumentException(
                "Quantity must be between 1 and 10.",
                nameof(quantity));
        }

        ticketType.Reserve(quantity);

        var line =
            new OrderLine(
                Id,
                ticketType.Id,
                quantity,
                ticketType.Price);

        _lines.Add(line);
    }

    public void Cancel() => Status = OrderStatus.Cancelled;

 
}