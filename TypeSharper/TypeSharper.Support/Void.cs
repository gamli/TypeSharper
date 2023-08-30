namespace TypeSharper;

public record Void
{
    private Void() { }
    public static Void Instance = new ();
}
