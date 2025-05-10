# Implementação do Consumo de Eventos no DigiPay.Wallet.Api

## Visão Geral

Este documento descreve a implementação do consumo de eventos do RabbitMQ no microserviço DigiPay.Wallet.Api, mostrando como ele processa as solicitações de transações vindas do DigiPay.Transaction.Api.

## Fluxo de Processos

1. O **Transaction.API** publica eventos de transação:
   - `TransferRequestedEvent` para transferências
   - `DepositRequestedEvent` para depósitos

2. O **Wallet.API** consome esses eventos:
   - Assina as filas `transaction.transfer.requested` e `transaction.deposit.requested`
   - Processa as solicitações usando o `WalletService`
   - Atualiza os saldos das carteiras

3. O **Wallet.API** publica eventos de confirmação:
   - `BalanceUpdatedEvent` contendo o resultado da operação
   - Inclui informações como o novo saldo da carteira

4. O **Transaction.API** consome os eventos de confirmação:
   - Finaliza o processo de transação
   - Informa o usuário sobre o resultado

## Componentes Implementados

### Eventos
- Definição dos mesmos modelos de eventos usados no Transaction.API
- Adição de construtores vazios para desserialização

### Serviço de RabbitMQ
- Configuração idêntica à do Transaction.API para garantir compatibilidade
- Métodos específicos para assinar eventos de transação
- Método para publicar eventos de atualização de saldo

### Manipulador de Eventos de Transação
- `TransactionEventHandler`: responsável por processar eventos e interagir com o WalletService
- Lógica para tratamento de `TransferRequestedEvent` e `DepositRequestedEvent`
- Publicação de respostas através de `BalanceUpdatedEvent`

## Tratamento de Erros

1. **Reconhecimento manual de mensagens**:
   - `BasicAck` para confirmar processamento bem-sucedido
   - `BasicNack` para rejeitar mensagens e reenfileirá-las em caso de erro

2. **Tratamento de exceções**:
   - Captura de exceções durante o processamento
   - Envio de eventos de falha informando o problema

3. **Logging extensivo**:
   - Registro de todas as etapas do processo
   - Informações detalhadas de erros para diagnóstico

## Inicialização

A configuração do RabbitMQ é feita no startup da aplicação:

1. Registro dos serviços:
   ```csharp
   builder.Services.Configure<RabbitMQSettings>(
       builder.Configuration.GetSection("RabbitMQ"));
   builder.Services.AddSingleton<RabbitMQService>();
   builder.Services.AddSingleton<TransactionEventHandler>();
   ```

2. Inicialização do handler no pipeline:
   ```csharp
   app.Services.GetRequiredService<TransactionEventHandler>();
   ```

## Configuração

As configurações do RabbitMQ são especificadas no `appsettings.json`:

```json
"RabbitMQ": {
  "HostName": "localhost",
  "Port": 5672,
  "UserName": "guest",
  "Password": "guest",
  "VirtualHost": "/"
}
```

## Considerações

1. **Isolamento de responsabilidades**:
   - O manipulador de eventos é responsável apenas por orquestrar as chamadas
   - A lógica de negócio permanece encapsulada no WalletService

2. **Consumo eficiente de mensagens**:
   - Processamento assíncrono para evitar bloqueios
   - Reconhecimento de mensagens apenas após processamento completo

3. **Consistência eventual**:
   - O sistema aceita que haverá um período de inconsistência entre serviços
   - Garante que eventualmente os dados estarão consistentes em todos os serviços 