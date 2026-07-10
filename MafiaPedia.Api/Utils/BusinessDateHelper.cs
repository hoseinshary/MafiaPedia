namespace MafiaPedia.Api.Utils;

public static class BusinessDateHelper
{
    public static DateOnly Today() => Compute(DateTime.Now);

    public static DateOnly Compute(DateTime dt) =>
        dt.TimeOfDay < TimeSpan.FromHours(12)
            ? DateOnly.FromDateTime(dt.Date.AddDays(-1))
            : DateOnly.FromDateTime(dt.Date);
}
