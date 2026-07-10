namespace MafiaPedia.Api.Utils;

public static class PersianTextNormalizer
{
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input
            .Replace('ي', 'ی')
            .Replace('ك', 'ک');
    }
}
