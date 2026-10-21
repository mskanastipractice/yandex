using Events.Domain;
using MediatR;

namespace Events.Application.QueryHandlers;

public record GetTopEventsQuery : IRequest<IReadOnlyList<Event>>;
