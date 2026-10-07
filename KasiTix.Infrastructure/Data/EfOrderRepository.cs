using KasiTix.Domain.Entities;
using KasiTix.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KasiTix.Infrastructure.Data;

public sealed class EfOrderRepository : IOrderRepository
{
    private readonly KasiTixDbContext  _dbContext;

    public EfOrderRepository(KasiTixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?>
        GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(order =>order.Lines)
            .SingleOrDefaultAsync(order =>
                    order.IdempotencyKey == idempotencyKey);
    }

    public async Task<bool>HasConfirmedOrderAsync(Guid eventId, string buyerEmail)
    {
        return await _dbContext.Orders
            .AnyAsync(order =>
                order.EventId == eventId &&
                order.BuyerEmail == buyerEmail &&
                order.Status == OrderStatus.Confirmed);
    }

    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}