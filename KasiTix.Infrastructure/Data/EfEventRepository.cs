using KasiTix.Domain.Entities;
using KasiTix.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KasiTix.Infrastructure.Data;

public sealed class EfEventRepository : IEventRepository
{
    private readonly KasiTixDbContext  _dbContext;

    public EfEventRepository(KasiTixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Event?> GetByIdAsync(Guid id, bool trackChanges = false)
    {
        IQueryable<Event> query = _dbContext.Events
                .Include(e => e.TicketTypes);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .SingleOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddAsync(Event @event)
    {
        await _dbContext.Events.AddAsync(@event);
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Event>> ListAsync(EventStatus status)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .Where(e => e.Status == status)
            .Include(e => e.TicketTypes)
            .AsSingleQuery()
            .OrderBy(e => e.StartsAt)
            .ToListAsync();
    }
}