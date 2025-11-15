using System;
using System.Collections.Generic;
using System.Linq;
using FreelanceOdyssey.Core;
using FreelanceOdyssey.Room;
using FreelanceOdyssey.Skill;
using UnityEngine;

namespace FreelanceOdyssey.Player
{
    public class PlayerProgress : MonoBehaviour
    {
        [Header("Data Sources")] public List<JobDefinition> jobs = new();
        public List<SkillDefinition> skills = new();
        public List<RoomLevelDefinition> roomLevels = new();
        public GameBalance balance;

        public PlayerSaveData Data { get; private set; } = new();

        public event Action OnDataChanged;

        public JobDefinition CurrentJob => jobs.FirstOrDefault(j => j.jobId == Data.jobId) ?? jobs.FirstOrDefault();

        private void Awake()
        {
            if (jobs.Count == 0)
            {
                jobs.AddRange(Resources.LoadAll<JobDefinition>("Jobs"));
            }

            if (skills.Count == 0)
            {
                skills.AddRange(Resources.LoadAll<SkillDefinition>("Skills"));
            }

            if (roomLevels.Count == 0)
            {
                roomLevels.AddRange(Resources.LoadAll<RoomLevelDefinition>("Room"));
                roomLevels = roomLevels.OrderBy(r => r.level).ToList();
            }

            if (balance == null)
            {
                balance = Resources.Load<GameBalance>("GameBalance");
            }
        }

        public void Load(PlayerSaveData saveData)
        {
            Data = saveData;
            EnsureIntegrity();
            OnDataChanged?.Invoke();
        }

        public void ApplyIdleXp(float minutes, float idleMultiplier)
        {
            var perMinute = balance.baseIdleXpPerMinute;
            var jobBonus = CurrentJob != null ? CurrentJob.idleXpBonus : 0f;
            var xp = minutes * perMinute * (1f + jobBonus + idleMultiplier);
            AddXp(xp);
        }

        public void AddXp(float amount)
        {
            Data.currentXp += amount;
            CheckLevelUp();
            OnDataChanged?.Invoke();
        }

        public void AddCoins(int amount)
        {
            Data.coins += amount;
            OnDataChanged?.Invoke();
        }

        public float GetXpRequiredForNextLevel()
        {
            var nextLevel = Data.level + 1;
            return balance.levelXpCurve.Evaluate(nextLevel);
        }

        public float GetTotalIdleSkillBonus()
        {
            float total = 0f;
            foreach (var skillId in Data.unlockedSkills)
            {
                var definition = skills.FirstOrDefault(s => s.skillId == skillId);
                if (definition == null)
                {
                    continue;
                }

                if (definition.levels.Count > 0)
                {
                    total += definition.levels.Last().idleXpBonus;
                }
            }

            if (Data.roomLevel > 0)
            {
                var room = roomLevels.FirstOrDefault(r => r.level == Data.roomLevel);
                if (room != null)
                {
                    total += Mathf.Max(0f, room.idleXpMultiplier - 1f);
                }
            }

            return total;
        }

        public float GetPuzzleScoreBonus()
        {
            float bonus = CurrentJob != null ? CurrentJob.puzzleScoreBonus : 0f;
            foreach (var skillId in Data.unlockedSkills)
            {
                var definition = skills.FirstOrDefault(s => s.skillId == skillId);
                if (definition != null && definition.levels.Count > 0)
                {
                    bonus += definition.levels.Last().puzzleScoreBonus;
                }
            }

            return bonus;
        }

        public float GetSkillCooldownReduction()
        {
            float reduction = 0f;
            foreach (var skillId in Data.unlockedSkills)
            {
                var definition = skills.FirstOrDefault(s => s.skillId == skillId);
                if (definition != null && definition.levels.Count > 0)
                {
                    reduction += definition.levels.Last().skillCooldownReduction;
                }
            }

            return Mathf.Clamp01(reduction);
        }

        public void UnlockSkill(string skillId)
        {
            if (Data.unlockedSkills.Contains(skillId))
            {
                return;
            }

            Data.unlockedSkills.Add(skillId);
            OnDataChanged?.Invoke();
        }

        private void EnsureIntegrity()
        {
            if (!jobs.Any(j => j.jobId == Data.jobId) && jobs.Count > 0)
            {
                Data.jobId = jobs[0].jobId;
            }

            Data.level = Mathf.Max(1, Data.level);
            Data.roomLevel = Mathf.Clamp(Data.roomLevel, 1, roomLevels.Count > 0 ? roomLevels.Count : 99);
        }

        private void CheckLevelUp()
        {
            bool leveled = false;
            while (Data.currentXp >= GetXpRequiredForNextLevel())
            {
                Data.currentXp -= GetXpRequiredForNextLevel();
                Data.level++;
                Data.skillPoints++;
                leveled = true;
            }

            if (leveled)
            {
                Debug.Log($"Level Up! New level: {Data.level}");
            }
        }
    }
}
