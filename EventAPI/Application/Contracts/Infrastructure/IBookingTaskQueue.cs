namespace Application.Contracts.Infrastructure;

public interface IBookingTaskQueue
{
	void Enqueue(BookingTask task);
	bool TryDequeue(out BookingTask task);
}