using System.Collections.Generic;
using System.Linq;
using TypeSharper.Attributes;
using TypeSharper.Sample.SampleBaseTypes.BusinessObjects;

namespace TypeSharper.Sample;

[TypeSharperPick<Customer>("Id", "CreditLimit")]
public partial record CustomerLimit;

public static class PickSample
{
    public static CustomerLimit Ctor(Customer customer) => new(customer);
    public static CustomerLimit ImplicitCast(Customer customer) => customer;

    public static decimal SumOfCreditLimits(IEnumerable<CustomerLimit> customerLimits)
        => customerLimits.Aggregate(0m, (acc, limit) => acc + limit.CreditLimit);
}
