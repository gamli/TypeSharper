using System;

namespace TypeSharper.Sample.SampleBaseTypes.BusinessObjects;

public record Shipment(
    Guid Id,
    DateTime ShipmentDate,
    string TrackingNumber,
    Address ShippingAddress,
    string Carrier,
    decimal ShippingCost,
    string ShipmentStatus,
    DateTime EstimatedDeliveryDate,
    bool IsDelivered,
    DateTime ActualDeliveryDate,
    bool RequiresSignature,
    string SignatoryName,
    bool IsReturnShipment,
    DateTime ReturnShipmentDate,
    string ReturnReason,
    bool IsDamaged,
    string DamageDescription,
    byte[] SignatureImage,
    string DeliveryNotes,
    Order OrderReference);
