namespace Ceiralizer.Generator.Utils;

public static class NameUtils
{
    public static string Sanitize(string name)
    {
        return name
            .Replace(".", "_")
            .Replace("[", "_")
            .Replace("]", "");
    }
}