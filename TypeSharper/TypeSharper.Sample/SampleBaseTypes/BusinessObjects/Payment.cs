using System;

namespace TypeSharper.Sample.SampleBaseTypes.BusinessObjects;

public record Payment(
    Guid Id,
    DateTime PaymentDate,
    decimal Amount,
    string PaymentMethod,
    string TransactionId,
    string Status,
    bool IsRefunded,
    DateTime RefundDate,
    decimal RefundAmount,
    string RefundReason,
    string BankName,
    string AccountNumber,
    string CardType,
    DateTime CardExpiryDate,
    Order OrderReference);
