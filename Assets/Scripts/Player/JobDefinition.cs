using UnityEngine;

namespace FreelanceOdyssey.Player
{
    [CreateAssetMenu(fileName = "Job", menuName = "FreelanceOdyssey/Job", order = 0)]
    public class JobDefinition : ScriptableObject
    {
        public string jobId = "designer";
        public string displayName = "デザイナー";
        [TextArea] public string description = "デザインの魔術師。";
        [Tooltip("Idle XP bonus applied as percentage. Example: 0.05 = +5%")]
        public float idleXpBonus = 0f;
        [Tooltip("Puzzle score multiplier bonus. Example: 0.1 = +10% score")] public float puzzleScoreBonus = 0f;
    }
}
