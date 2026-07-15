namespace MafiaPedia.Api.Utils;

public static class ClubPlayStatusResolver
{
    public static string Resolve(
        string currentStatus,
        string newPlayType,
        int? finalWinnersideId,
        IEnumerable<int?> finalPlayerRanks)
    {
        if (currentStatus == "pending")
            return "pending";

        bool needsWinnerside = newPlayType is "rank" or "superrank";
        bool needsRank = newPlayType == "superrank";

        if (needsWinnerside && finalWinnersideId is null)
            return "notwinside";

        if (needsRank && finalPlayerRanks.Any(r => r is null))
            return "notrank";

        return "done";
    }
}
