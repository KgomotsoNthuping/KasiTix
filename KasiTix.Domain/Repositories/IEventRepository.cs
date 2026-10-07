using KasiTix.Domain.Entities;

namespace KasiTix.Domain.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, bool trackChanges = false);
    

    Task AddAsync(Event @event);

    Task SaveChangesAsync();

    Task<IReadOnlyList<Event>> ListAsync(EventStatus status);
}