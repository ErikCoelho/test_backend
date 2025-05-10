# Implementação da Comunicação com RabbitMQ

## Visão Geral

Este documento descreve a implementação da comunicação entre os microserviços DigiPay.Transaction.Api e DigiPay.Wallet.Api utilizando RabbitMQ.

## Arquitetura de Eventos

A comunicação segue o padrão Event-Driven (Orientado a Eventos) com o fluxo:

1. **TransactionAPI** publica eventos como "TransactionRequested"
2. **WalletAPI** assina e processa as mudanças de saldo
3. **WalletAPI** publica eventos "BalanceUpdated"
4. **TransactionAPI** finaliza a transação quando os saldos são confirmados

## Eventos Implementados

- `TransferRequestedEvent`: Enviado quando uma transferência é solicitada
- `DepositRequestedEvent`: Enviado quando um depósito é solicitado
- `BalanceUpdatedEvent`: Recebido do Wallet.API quando um saldo é atualizado
- `TransactionCompletedEvent`: Enviado quando uma transação é finalizada

## Filas e Exchanges

- **Transaction Exchange**: `transaction.events` - Para eventos relacionados a transações
- **Wallet Exchange**: `wallet.events` - Para eventos relacionados a carteiras

Filas:
- `transaction.transfer.requested`
- `transaction.deposit.requested`
- `wallet.balance.updated`
- `transaction.completed`

## Implementação no Transaction.API

1. **Publicação de eventos**: Quando uma solicitação de transferência ou depósito é recebida, o serviço cria a transação em estado pendente e publica um evento correspondente.

2. **Recebimento de confirmações**: O serviço assina a fila `wallet.balance.updated` para receber atualizações sobre operações de saldo processadas pelo Wallet.API.

3. **Finalização de transações**: Quando uma confirmação é recebida, o serviço finaliza a transação e publica um evento `TransactionCompletedEvent`.

## Implementação no Wallet.API (a ser feita)

1. **Assinatura de eventos**: O Wallet.API deve assinar as filas `transaction.transfer.requested` e `transaction.deposit.requested`.

2. **Processamento de solicitações**: Ao receber um evento, o Wallet.API deve atualizar os saldos apropriados.

3. **Publicação de confirmações**: Após atualizar os saldos, o Wallet.API deve publicar um evento `BalanceUpdatedEvent` informando o resultado da operação.

## Resiliência e Gestão de Erros

- O sistema usa reconhecimento manual de mensagens (manual acknowledgment)
- Timeout para operações pendentes (30 segundos)
- Gerenciamento de transações pendentes através de `ConcurrentDictionary`
- Registro de logs detalhados para todas as operações

## Transações Distribuídas vs. Padrão Saga

### Transações Distribuídas (2PC - Two-Phase Commit)

**Transações Distribuídas** são baseadas no protocolo de confirmação em duas fases (2PC), onde um coordenador global garante que todos os participantes concordem em confirmar ou abortar uma transação:

1. **Fase de Preparação**: O coordenador pergunta a todos os participantes se podem confirmar a transação
2. **Fase de Confirmação**: Se todos concordarem, o coordenador solicita a todos que confirmem; caso contrário, solicita que abortem

**Prós**:
- ACID completo (Atomicidade, Consistência, Isolamento, Durabilidade)
- Mais simples de entender conceitualmente

**Contras**:
- Bloqueio de recursos durante a transação
- Baixa performance e escalabilidade
- Ponto único de falha (coordenador)
- Não funciona bem em ambientes distribuídos com alta latência

### Padrão Saga

O **Padrão Saga** divide uma transação em uma sequência de transações locais, onde cada transação publica eventos que acionam a próxima transação:

1. Cada serviço executa sua transação local e publica um evento
2. Outros serviços reagem a esse evento
3. Se um passo falhar, transações compensatórias são executadas para desfazer alterações

**Tipos de Implementação**:
- **Coreografia**: Cada serviço sabe como reagir a eventos (o que usamos aqui)
- **Orquestração**: Um serviço central coordena os passos

**Prós**:
- Melhor desempenho e escalabilidade
- Sem bloqueios longos de recursos
- Melhor isolamento de falhas
- Ideal para microsserviços

**Contras**:
- Consistência eventual (não ACID)
- Maior complexidade ao implementar transações compensatórias
- Lógica de negócios mais distribuída

### Recomendação

**O padrão Saga é mais recomendado para arquiteturas de microsserviços** pelos seguintes motivos:

1. **Melhor escalabilidade**: Permite que cada serviço escale independentemente
2. **Maior resiliência**: Funciona melhor em redes não confiáveis
3. **Desacoplamento**: Serviços podem evoluir independentemente
4. **Melhor desempenho**: Não requer bloqueios de recursos entre serviços

A implementação adotada neste projeto segue o padrão Saga com coreografia, onde:
- Transaction.API publica eventos solicitando operações
- Wallet.API processa essas solicitações e publica o resultado
- Transaction.API completa o fluxo com base nas respostas

Esta abordagem oferece um bom equilíbrio entre consistência, desempenho e resiliência. 