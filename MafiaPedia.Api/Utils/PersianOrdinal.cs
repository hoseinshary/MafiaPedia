namespace MafiaPedia.Api.Utils;

public static class PersianOrdinal
{
    private static readonly Dictionary<int, string> Ordinals = new()
    {
        { 1, "اول" },
        { 2, "دوم" },
        { 3, "سوم" },
        { 4, "چهارم" },
        { 5, "پنجم" },
        { 6, "ششم" },
        { 7, "هفتم" },
        { 8, "هشتم" },
        { 9, "نهم" },
        { 10, "دهم" },
        { 11, "یازدهم" },
        { 12, "دوازدهم" },
        { 13, "سیزدهم" },
        { 14, "چهاردهم" },
        { 15, "پانزدهم" },
        { 16, "شانزدهم" },
        { 17, "هفدهم" },
        { 18, "هجدهم" },
        { 19, "نوزدهم" },
        { 20, "بیستم" }
    };

    public static string ToOrdinal(int number) =>
        Ordinals.GetValueOrDefault(number, number.ToString());
}
