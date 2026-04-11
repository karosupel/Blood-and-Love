using UnityEngine;

public static class BossFightRestartState
{
    static int? preFightHearts;
    static bool hasPendingHeartRestore;
    static int pendingRestoredHearts;

    public static void SavePreFightHearts(int hearts)
    {
        preFightHearts = Mathf.Max(0, hearts);
    }

    public static int GetPreFightHeartsOrDefault(int fallbackHearts)
    {
        if (preFightHearts.HasValue)
        {
            return preFightHearts.Value;
        }

        return Mathf.Max(0, fallbackHearts);
    }

    public static void ScheduleHeartRestore(int hearts)
    {
        pendingRestoredHearts = Mathf.Max(0, hearts);
        hasPendingHeartRestore = true;
    }

    public static bool TryConsumeHeartRestore(out int hearts)
    {
        if (!hasPendingHeartRestore)
        {
            hearts = 0;
            return false;
        }

        hearts = pendingRestoredHearts;
        hasPendingHeartRestore = false;
        pendingRestoredHearts = 0;
        return true;
    }
}