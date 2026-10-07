using KasiTix.Domain.Entities;

namespace KasiTix.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey);

    Task<bool> HasConfirmedOrderAsync(Guid eventId, string buyerEmail);

    Task AddAsync(Order order);

    Task SaveChangesAsync();
}