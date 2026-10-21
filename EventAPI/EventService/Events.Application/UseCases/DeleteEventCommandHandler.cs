using Events.Application.Contracts.Commands;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using MediatR;

namespace Events.Application.UseCases;

public class DeleteEventCommandHandler(IEventRepository repository, IEventCache cache) : IRequestHandler<DeleteEventCommand>
{
    public async Task Handle(DeleteEventCommand command, CancellationToken cancellationToken)
    {
        var eventToDelete = await repository.FindAsync(command.EventId, cancellationToken);

        if (eventToDelete is null)
        {
            throw new EntityNotFoundException("Событие", command.EventId);
        }

        repository.Delete(eventToDelete);

        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync($"event:{command.EventId}", cancellationToken);
    }
}