using UnityEngine;

namespace FreelanceOdyssey.Core
{
    [CreateAssetMenu(fileName = "GameBalance", menuName = "FreelanceOdyssey/Game Balance", order = 0)]
    public class GameBalance : ScriptableObject
    {
        [Header("Idle XP")] public float baseIdleXpPerMinute = 50f;
        [Tooltip("Maximum hours that can be accumulated while offline")] public float offlineCapHours = 24f;
        [Header("Puzzle Rewards")] public int basePuzzleXp = 150;
        public int basePuzzleCoins = 100;
        [Range(0f, 2f)] public float comboRewardMultiplier = 0.1f;
        [Header("Level Progression")] public AnimationCurve levelXpCurve = AnimationCurve.Linear(1, 0, 50, 5000);
    }
}
