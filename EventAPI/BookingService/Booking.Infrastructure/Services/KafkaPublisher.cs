using System.Text.Json;
using Booking.Application.Interfaces;
using Common.Contracts;
using Confluent.Kafka;

namespace Booking.Infrastructure.Services;

public sealed class KafkaPublisher : IKafkaPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaPublisher(KafkaSettings settings)
    {
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers
        }).Build();
    }

    public async Task PublishBookingConfirmedAsync(BookingConfirmedEvent @event, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(@event);

        await _producer.ProduceAsync(
            KafkaConstants.BookingConfirmedTopic,
            new Message<string, string>
            {
                Key = @event.EventId.ToString(),
                Value = json
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
