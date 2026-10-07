namespace KasiTix.Api.Services;
using FluentValidation;
using KasiTix.Api.Models;
using KasiTix.Domain.Repositories; // your interfaces, in Domain
public interface IEventService
{
 Task<EventResponse> CreateAsync(CreateEventRequest request);
 Task<EventResponse> GetByIdAsync(Guid id); // throws NotFoundException
 // TODO: AddTicketTypeAsync, PublishAsync (Tier 1); ListAsync, CancelAsync (Tier 2)
}
public interface IOrderService
{
 Task<(OrderResponse Order, bool IsReplay)> PlaceAsync(
 Guid eventId, string idempotencyKey, PlaceOrderRequest request);
}
// TODO: write EventService yourself.
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
 //
 // 2. Load the event WITH its ticket types, TRACKED: you're about to
 // change Sold. Missing -> 404. Not Published, or already started -> 422 (R5).
 //
 // 3. R9: does this buyer already hold a Confirmed order for this event?
 // -> 409. Ask the database. Don't load every order into memory.
 //
 // 4. Create the Order. For each requested line, find the ticket type on
 // the event (an id that isn't on this event: 404 or 400? Decide),
 // reserve the tickets, and add the line at today's price.
 //
 // 5. Save ONCE. If xmin catches a concurrent sale, let the exception
 // surface: your exception handler turns it into a 409.
 throw new NotImplementedException();
 }
}