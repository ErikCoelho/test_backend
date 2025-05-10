using DigiPay.Wallet.Api.Messaging.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DigiPay.Wallet.Api.Messaging.RabbitMQ
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

                // Declarar exchanges
                _channel.ExchangeDeclare(RabbitMQConstants.TransactionExchange, ExchangeType.Topic, durable: true);
                _channel.ExchangeDeclare(RabbitMQConstants.WalletExchange, ExchangeType.Topic, durable: true);

                // Declarar filas
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

                // Bindings
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

        public void PublishBalanceUpdated(BalanceUpdatedEvent @event)
        {
            try
            {
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: RabbitMQConstants.WalletExchange,
                    routingKey: RabbitMQConstants.BalanceUpdatedRoutingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogInformation("Evento BalanceUpdated publicado: {TransactionId}, {Success}", 
                    @event.TransactionId, @event.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento BalanceUpdated: {TransactionId}", @event.TransactionId);
                throw;
            }
        }

        public void SubscribeToTransferRequested(Func<TransferRequestedEvent, Task> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<TransferRequestedEvent>(message);

                    if (@event != null)
                    {
                        _logger.LogInformation("Evento TransferRequested recebido: {TransactionId}", @event.TransactionId);
                        await handler(@event);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar evento TransferRequested");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: RabbitMQConstants.TransferRequestedQueue,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Inscrito na fila {Queue} para eventos TransferRequested", 
                RabbitMQConstants.TransferRequestedQueue);
        }

        public void SubscribeToDepositRequested(Func<DepositRequestedEvent, Task> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<DepositRequestedEvent>(message);

                    if (@event != null)
                    {
                        _logger.LogInformation("Evento DepositRequested recebido: {TransactionId}", @event.TransactionId);
                        await handler(@event);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar evento DepositRequested");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: RabbitMQConstants.DepositRequestedQueue,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Inscrito na fila {Queue} para eventos DepositRequested", 
                RabbitMQConstants.DepositRequestedQueue);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
} 