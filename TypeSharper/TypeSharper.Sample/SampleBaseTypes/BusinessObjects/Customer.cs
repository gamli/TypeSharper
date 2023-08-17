using System;
using System.Collections.Generic;

namespace TypeSharper.Sample.SampleBaseTypes.BusinessObjects;

public record Customer(
    Guid Id,
    string FirstName,
    string LastName,
    Address Address,
    string Email,
    string Phone,
    DateTime DateRegistered,
    string PreferredPaymentMethod,
    string AlternatePhone,
    DateTime DateOfBirth,
    string Gender,
    string Occupation,
    string CompanyName,
    string Website,
    string SocialSecurityNumber,
    string PassportNumber,
    string DriversLicense,
    string MembershipId,
    string PreferredCommunication,
    bool IsEmailVerified,
    bool IsPhoneVerified,
    bool IsActive,
    string Notes,
    decimal CreditLimit,
    List<Order> Orders);
