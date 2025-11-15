using System;
using System.Collections.Generic;
using UnityEngine;

namespace FreelanceOdyssey.Skill
{
    [CreateAssetMenu(fileName = "Skill", menuName = "FreelanceOdyssey/Skill", order = 0)]
    public class SkillDefinition : ScriptableObject
    {
        public string skillId = "skill_idle_1";
        public string displayName = "集中力アップ";
        [TextArea]
        public string description = "放置XPが増加する。";
        public Sprite icon;
        public List<SkillLevelData> levels = new() { new SkillLevelData() };
    }

    [Serializable]
    public class SkillLevelData
    {
        [Tooltip("XP multiplier bonus. Example: 0.1 = +10% idle XP")] public float idleXpBonus = 0f;
        [Tooltip("Puzzle score bonus per level")] public float puzzleScoreBonus = 0f;
        [Tooltip("Cooldown reduction for puzzle skills")]
        public float skillCooldownReduction = 0f;
        [Tooltip("Skill points required for this level")]
        public int cost = 1;
    }
}
