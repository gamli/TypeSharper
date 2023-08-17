using System;

namespace TypeSharper.Sample.SampleBaseTypes.BusinessObjects;

public record LineItem(
    Guid Id,
    Product Product,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime AddedToCartDate,
    bool IsGift,
    string GiftMessage,
    bool IsReturnable,
    DateTime ReturnByDate,
    bool IsReturned,
    string ReturnReason,
    bool IsReplaced,
    DateTime ReplacementDate,
    Order OrderReference);
