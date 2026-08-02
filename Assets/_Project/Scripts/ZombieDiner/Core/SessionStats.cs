using UnityEngine;

namespace ZombieDiner.Core
{
    public static class SessionStats
    {
        public static int ServedPeopleCount { get; private set; } = 0;
        public static string WaveReached { get; set; } = "Stage 1";
        public static int CollectedCoins { get; private set; } = 0;

        public static void AddServedPerson(int rewardAmount)
        {
            ServedPeopleCount++;
            CollectedCoins += Mathf.Max(0, rewardAmount);
        }

        public static void ResetStats()
        {
            ServedPeopleCount = 0;
            CollectedCoins = 0;
            WaveReached = "Stage 1";
        }
    }
}
