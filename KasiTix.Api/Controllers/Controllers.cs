namespace KasiTix.Api.Controllers;
using KasiTix.Api.Models;
using KasiTix.Api.Services;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/events")]
public class EventsController(IEventService events) : ControllerBase
{
 /// <summary>Creates an event in Draft.</summary>
 [HttpPost]
 [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
 public async Task<ActionResult<EventResponse>> CreateAsync(CreateEventRequest request)
 {
 var created = await events.CreateAsync(request);
 return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
 }
 /// <summary>Gets one event with its ticket types and what's left of each.</summary>
 [HttpGet("{id:guid}")]
 [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 public async Task<ActionResult<EventResponse>> GetByIdAsync(Guid id) =>
 Ok(await events.GetByIdAsync(id));
 // TODO: POST /api/events/{id}/ticket-types (endpoint 3: 201, 400, 404, 409, 422)
[HttpPost("{id:guid}/ticket-types")]
[ProducesResponseType(typeof(TicketTypeResponse),StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status422UnprocessableEntity)]
public async Task<ActionResult<TicketTypeResponse>> AddTicketTypeAsync(Guid id , CreateTicketTypeRequest request)
{
    var created =await events.AddTicketTypeAsync( id,request);

    return StatusCode(StatusCodes.Status201Created, created);
}
 // TODO: POST /api/events/{id}/publish (endpoint 4: 204, 404, 422)
 // Every status code goes on the action as [ProducesResponseType].
[HttpPost("{id:guid}/publish")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status422UnprocessableEntity)]
public async Task<IActionResult> PublishAsync(Guid id)
{
    await events.PublishAsync(id);

    return NoContent();
}
}
[ApiController]
[Route("api/events/{eventId:guid}/orders")]
public class OrdersController(IOrderService orders) : ControllerBase
{
 /// <summary>Places an order. Safe to retry with the same Idempotency-Key.</summary>
 [HttpPost]
 [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
 [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderResponse>> PlaceAsync(
    Guid eventId,
    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
    PlaceOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        throw new ArgumentException("The Idempotency-Key header is required.", nameof(
        idempotencyKey));
        var (order, isReplay) = await orders.PlaceAsync(eventId, idempotencyKey, request);
        // TODO: a replay returns 200 with the original order; a new order returns 201.
        if (isReplay)
        {
            return Ok(order);
        }

        return StatusCode(StatusCodes.Status201Created,order);
        //throw new NotImplementedException();
    }
}