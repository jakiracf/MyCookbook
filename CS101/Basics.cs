namespace CS101;

public class Basics
{
    // TODO-1: return the sum of a and b
    public static int Add(int a, int b)
    {
        return a + b;
        throw new NotImplementedException();
    }
    public static string FormatTitle (string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return title;
        }
        title=title.Trim().ToLower();
        return char.ToUpper(title[0]) + title.Substring(1);

        throw new NotImplementedException();
    }
}