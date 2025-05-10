using DigiPay.Transaction.Api.Messaging.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DigiPay.Transaction.Api.Messaging.RabbitMQ
{
    public class RabbitMQService : IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(RabbitMQConstants.TransactionExchange, ExchangeType.Topic, durable: true);
                _channel.ExchangeDeclare(RabbitMQConstants.WalletExchange, ExchangeType.Topic, durable: true);

                _channel.QueueDeclare(
                    queue: RabbitMQConstants.TransferRequestedQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.QueueDeclare(
                    queue: RabbitMQConstants.DepositRequestedQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.QueueDeclare(
                    queue: RabbitMQConstants.BalanceUpdatedQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.QueueDeclare(
                    queue: RabbitMQConstants.TransactionCompletedQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.QueueBind(
                    queue: RabbitMQConstants.TransferRequestedQueue,
                    exchange: RabbitMQConstants.TransactionExchange,
                    routingKey: RabbitMQConstants.TransferRequestedRoutingKey);

                _channel.QueueBind(
                    queue: RabbitMQConstants.DepositRequestedQueue,
                    exchange: RabbitMQConstants.TransactionExchange,
                    routingKey: RabbitMQConstants.DepositRequestedRoutingKey);

                _channel.QueueBind(
                    queue: RabbitMQConstants.BalanceUpdatedQueue,
                    exchange: RabbitMQConstants.WalletExchange,
                    routingKey: RabbitMQConstants.BalanceUpdatedRoutingKey);

                _channel.QueueBind(
                    queue: RabbitMQConstants.TransactionCompletedQueue,
                    exchange: RabbitMQConstants.TransactionExchange,
                    routingKey: RabbitMQConstants.TransactionCompletedRoutingKey);

                _logger.LogInformation("RabbitMQ configurado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar RabbitMQ");
                throw;
            }
        }

        public void PublishTransferRequested(TransferRequestedEvent @event)
        {
            try
            {
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: RabbitMQConstants.TransactionExchange,
                    routingKey: RabbitMQConstants.TransferRequestedRoutingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogInformation("Evento TransferRequested publicado: {TransactionId}", @event.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento TransferRequested: {TransactionId}", @event.TransactionId);
                throw;
            }
        }

        public void PublishDepositRequested(DepositRequestedEvent @event)
        {
            try
            {
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: RabbitMQConstants.TransactionExchange,
                    routingKey: RabbitMQConstants.DepositRequestedRoutingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogInformation("Evento DepositRequested publicado: {TransactionId}", @event.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento DepositRequested: {TransactionId}", @event.TransactionId);
                throw;
            }
        }

        public void PublishTransactionCompleted(TransactionCompletedEvent @event)
        {
            try
            {
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: RabbitMQConstants.TransactionExchange,
                    routingKey: RabbitMQConstants.TransactionCompletedRoutingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogInformation("Evento TransactionCompleted publicado: {TransactionId}", @event.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento TransactionCompleted: {TransactionId}", @event.TransactionId);
                throw;
            }
        }

        public void SubscribeToBalanceUpdated(Func<BalanceUpdatedEvent, Task> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<BalanceUpdatedEvent>(message);

                    if (@event != null)
                    {
                        await handler(@event);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar evento BalanceUpdated");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: RabbitMQConstants.BalanceUpdatedQueue,
                autoAck: false,
                consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
} 