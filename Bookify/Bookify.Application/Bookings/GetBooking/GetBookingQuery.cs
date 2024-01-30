using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Bookings.GetBooking;

public sealed record class GetBookingQuery(Guid BookingId) : IQuery<BookingResponse>;