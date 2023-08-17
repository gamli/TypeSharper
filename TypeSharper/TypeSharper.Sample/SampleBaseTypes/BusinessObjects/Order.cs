using System;
using System.Collections.Generic;

namespace TypeSharper.Sample.SampleBaseTypes.BusinessObjects;

public record Order(
    Guid Id,
    Customer CustomerReference,
    DateTime OrderDate,
    decimal TotalAmount,
    string OrderStatus,
    string OrderNotes,
    Address BillingAddress,
    string BillingName,
    bool IsGiftOrder,
    DateTime EstimatedDeliveryDate,
    DateTime ActualDeliveryDate,
    bool IsCancelled,
    DateTime CancelledDate,
    string CancelReason,
    List<LineItem> LineItems,
    List<Shipment> Shipments,
    List<Payment> Payments,
    string PromoCode);
