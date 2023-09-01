namespace TypeSharper;

public record Void
{
    public static Void Instance = new();

    #region Private

    private Void() { }

    #endregion
}
