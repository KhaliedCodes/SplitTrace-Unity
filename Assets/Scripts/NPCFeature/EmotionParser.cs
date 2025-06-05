using System.Text.RegularExpressions;

public static class EmotionParser
{
    private static readonly Regex emotionRegex = 
        new Regex(@"{\s*""emotion""\s*:\s*""(\w+)""\s*}", RegexOptions.Compiled);

    public static (string emotion, string cleanText) Parse(string text)
    {
        if (string.IsNullOrEmpty(text))
            return ("neutral", text);

        var match = emotionRegex.Match(text);
        if (!match.Success)
            return ("neutral", text);

        string emotion = match.Groups[1].Value.ToLower();
        string cleanText = emotionRegex.Replace(text, "").Trim();
        return (emotion, cleanText);
    }
}