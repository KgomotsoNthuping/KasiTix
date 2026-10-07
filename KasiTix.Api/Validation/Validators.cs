namespace KasiTix.Api.Validation;
using FluentValidation;
using KasiTix.Api.Models;
public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
 public CreateEventRequestValidator()
 {
 RuleFor(r => r.Name).NotEmpty().MaximumLength(120);
 RuleFor(r => r.Venue).NotEmpty();
 // TODO (R1): StartsAt must be in the future.
 }
}
public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
 public PlaceOrderRequestValidator()
 {
 RuleFor(r => r.BuyerEmail).NotEmpty().EmailAddress();
 RuleFor(r => r.Lines)
 .NotEmpty()
 .Must(lines => lines is null || lines.Count <= 5)
 .WithMessage("An order can have at most 5 lines.");
 // TODO (R6): every line's Quantity is between 1 and 10. (Hint: RuleForEach.)
 // TODO (R6): no TicketTypeId appears twice in Lines.
 }
}
// TODO (R4): write CreateTicketTypeRequestValidator yourself, from scratch.