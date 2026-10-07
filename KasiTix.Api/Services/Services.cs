namespace KasiTix.Api.Services;

using FluentValidation;
using KasiTix.Api.Models;
using KasiTix.Domain.Repositories; // your interfaces, in Domain
using KasiTix.Domain.Entities;
using KasiTix.Domain.Exceptions;

    public interface IEventService
    {
        Task<EventResponse> CreateAsync(CreateEventRequest request);
        Task<EventResponse> GetByIdAsync(Guid id); // throws NotFoundException
        // TODO: AddTicketTypeAsync, PublishAsync (Tier 1); ListAsync, CancelAsync (Tier 2)
        Task<TicketTypeResponse> AddTicketTypeAsync(Guid eventId, CreateTicketTypeRequest request);
        Task PublishAsync(Guid eventId);
    }
public interface IOrderService
{
 Task<(OrderResponse Order, bool IsReplay)> PlaceAsync(
 Guid eventId, string idempotencyKey, PlaceOrderRequest request);
}
// TODO: write EventService yourself.
public class EventService(
    IEventRepository events,
    IValidator<CreateEventRequest> createValidator,
    IValidator<CreateTicketTypeRequest> ticketValidator)
    : IEventService
{
    public async Task<EventResponse> CreateAsync(CreateEventRequest request)
    {
        await createValidator.ValidateAndThrowAsync(request);

        var @event = new Event(
            request.Name,
            request.Venue,
            request.StartsAt);

        await events.AddAsync(@event);

        await events.SaveChangesAsync();

        return EventResponse.FromEntity(@event);
    }

    public async Task<EventResponse> GetByIdAsync(Guid id)
    {
        var @event = await events.GetByIdAsync(id, trackChanges: false);

        if (@event is null)
        {
            throw new NotFoundException($"Event '{id}' was not found.");
        }

        return EventResponse.FromEntity(@event);
    }

    public async Task<TicketTypeResponse> AddTicketTypeAsync(Guid eventId, CreateTicketTypeRequest request)
    {
        await ticketValidator.ValidateAndThrowAsync(request);

        var @event =await events.GetByIdAsync(eventId, trackChanges: true);

        if (@event is null)
        {
            throw new NotFoundException($"Event '{eventId}' was not found.");
        }

        var ticketType = @event.AddTicketType(
                request.Name,
                request.Price,
                request.Capacity);

        await events.SaveChangesAsync();

        return TicketTypeResponse.FromEntity(ticketType);
    }

    public async Task PublishAsync(Guid eventId)
    {
        var @event =await events.GetByIdAsync(eventId,trackChanges: true);

        if (@event is null)
        {
            throw new NotFoundException($"Event '{eventId}' was not found.");
        }

        @event.Publish();

        await events.SaveChangesAsync();
    }
}

    public class OrderService(
    IEventRepository events,
    IOrderRepository orders,
    IValidator<PlaceOrderRequest> validator) : IOrderService
    {
        public async Task<(OrderResponse Order, bool IsReplay)> PlaceAsync(
        Guid eventId, string idempotencyKey, PlaceOrderRequest request)
        {
            await validator.ValidateAndThrowAsync(request);
            // TODO: in this order. Think about each step before you write it.
            //
            // 1. Replay? Look the key up. If an order already has it, return that
            // order with IsReplay = true. (What happens if two requests with the
            // same NEW key arrive at the same moment? Which constraint saves you?)
            
            var existing = await orders.GetByIdempotencyKeyAsync(idempotencyKey);

            if (existing is not null)
            {
                var existingEvent = await events.GetByIdAsync(existing.EventId,trackChanges: false);

                if (existingEvent is null)
                {
                    throw new NotFoundException($"Event '{existing.EventId}' was not found.");
                }

                return (OrderResponse.FromEntity(existing,existingEvent.TicketTypes),
                    true
                );
            }
            // 2. Load the event WITH its ticket types, TRACKED: you're about to
            // change Sold. Missing -> 404. Not Published, or already started -> 422 (R5).
            var @event = await events.GetByIdAsync(eventId, trackChanges: true);

            if (@event is null)
            {
                throw new NotFoundException($"Event '{eventId}' was not found.");
            }
            if (@event.Status != EventStatus.Published || @event.StartsAt <= DateTime.UtcNow)
            {
                throw new UnprocessableEntityException("The event is not currently on sale.");
            }


            // 3. R9: does this buyer already hold a Confirmed order for this event?
            // -> 409. Ask the database. Don't load every order into memory.
            var alreadyHasOrder = await orders.HasConfirmedOrderAsync(eventId, request.BuyerEmail);

            if (alreadyHasOrder)
            {
                throw new ConflictException("This buyer already has a confirmed order for this event.");
            }
            

            // 4. Create the Order. For each requested line, find the ticket type on
            // the event (an id that isn't on this event: 404 or 400? Decide),
            // reserve the tickets, and add the line at today's price.
            //
            // 5. Save ONCE. If xmin catches a concurrent sale, let the exception
            // surface: your exception handler turns it into a 409.


            throw new NotImplementedException();
    }
}