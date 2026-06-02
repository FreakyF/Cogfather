namespace Cogfather.HQ.UI.Helpers;

public static class DisplayHelper
{
    public static string FormatId(string id) =>
        string.IsNullOrEmpty(id) ? id : char.ToUpper(id[0]) + id[1..].Replace('-', ' ');
}
