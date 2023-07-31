// using System;
// using TypeSharper.Attributes;
//
// namespace TypeSharper.Sample;
//
// [TypeSharperUnion<string, int, object>("Str", "Int", "Obj", "without")]
// public abstract class StringOrInt32OrObject { }
//
// public static class PickSample
// {
//     public static void Moo(ISecondPickTarget spt) { Console.WriteLine(spt.Name); }
//
//     public static void SampleCode()
//     {
//         var psc1 = new PickedSampleClass1 { IsSample = true };
//         var psc2 = new PickedSampleClass2 { Name = "Two", Count = 12 };
//         var psc2_1 = new PickedSampleClass2_1 { Count = 12 };
//         var sioi = StringOrInt32OrObject.CreateInt(1234);
//         var sios = StringOrInt32OrObject.CreateStr("45");
//
//
//         sioi.Match(
//             Console.WriteLine,
//             i => Console.WriteLine(3 * i),
//             obj => Console.WriteLine(obj.GetType()),
//             o => Console.WriteLine("without {0}", o));
//     }
// }
//
// public interface IPickSource
// {
//     public string Name { get; set; }
// }
//
// [TypeSharperPickAttribute<IPickSource>(nameof(IPickSource.Name))]
// public interface IFirstPickTarget { }
//
// [TypeSharperPickAttribute<IFirstPickTarget>(nameof(IFirstPickTarget.Name))]
// public interface ISecondPickTarget { }
//
// [TypeSharperPick<SampleClass>(nameof(SampleClass.IsSample))]
// public class PickedSampleClass1 { }
//
// [TypeSharperPick<SampleClass>("Name", "Count")]
// public class PickedSampleClass2 { }
//
// [TypeSharperPick<PickedSampleClass2>(nameof(PickedSampleClass2.Count))]
// public class PickedSampleClass2_1 { }
//
// public class SampleClass
// {
//     public int Count { get; set; }
//     public bool IsSample { get; set; }
//     public string Name { get; set; }
// }



