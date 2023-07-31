using System;

namespace TypeSharper.Model;

public class TsModelException : Exception
{
    public TsModelException(string message, object model) : base(message) => Model = model;
    public object Model { get; }

    public static void ThrowIfNot(bool value, string message, object model)
    {
        if (!value)
        {
            throw new TsModelException(message, model);
        }
    }
}
