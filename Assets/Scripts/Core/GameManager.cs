using System;
using FreelanceOdyssey.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FreelanceOdyssey.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")] public PlayerProgress playerProgress;
        public TimeService timeService;
        public SaveDataManager saveDataManager;
        public GameBalance balance;

        [Header("UI")] public UI.GameHudController gameHud;
        public Puzzle.PuzzleController puzzleController;

        private void Awake()
        {
            if (balance == null)
            {
                balance = Resources.Load<GameBalance>("GameBalance");
            }

            if (playerProgress != null)
            {
                if (playerProgress.balance == null)
                {
                    playerProgress.balance = balance;
                }
                playerProgress.balance = balance;
            }
        }

        private void Start()
        {
            gameHud?.Initialize(puzzleController);
            if (puzzleController != null)
            {
                puzzleController.playerProgress = playerProgress;
                puzzleController.balance = balance;
                puzzleController.InitializeController();
            }

            var existingEventSystem =
#if UNITY_2023_1_OR_NEWER
                FindFirstObjectByType<EventSystem>();
#else
                FindObjectOfType<EventSystem>();
#endif

            if (EventSystem.current == null && existingEventSystem == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystem.transform.SetParent(transform);
            }
            LoadProgress();
            ApplyOfflineRewards();
            HookEvents();
            gameHud?.ShowHome();
        }

        private void OnApplicationQuit()
        {
            PersistData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PersistData();
            }
        }

        private void HookEvents()
        {
            if (playerProgress != null)
            {
                playerProgress.OnDataChanged += HandleDataChanged;
                HandleDataChanged();
            }

            if (puzzleController != null)
            {
                puzzleController.OnPuzzleFinished += HandlePuzzleFinished;
            }
        }

        private void HandleDataChanged()
        {
            gameHud?.UpdateStatus(playerProgress);
        }

        private void HandlePuzzleFinished(Puzzle.PuzzleResult result)
        {
            var bonus = 1f + playerProgress.GetPuzzleScoreBonus();
            var xp = Mathf.RoundToInt(result.totalXp * bonus);
            var coins = Mathf.RoundToInt(result.totalCoins * bonus);
            playerProgress.AddXp(xp);
            playerProgress.AddCoins(coins);
            playerProgress.Data.bestPuzzleScore = Mathf.Max(playerProgress.Data.bestPuzzleScore, result.score);
            gameHud?.ShowResult(result, xp, coins);
            PersistData();
        }

        private void LoadProgress()
        {
            if (playerProgress == null)
            {
                return;
            }

            if (saveDataManager != null && saveDataManager.TryLoad(out var data))
            {
                playerProgress.Load(data);
            }
            else
            {
                playerProgress.Load(new PlayerSaveData());
            }
        }

        private void ApplyOfflineRewards()
        {
            if (playerProgress == null || balance == null || timeService == null)
            {
                return;
            }

            var lastExit = new DateTime(playerProgress.Data.lastExitTicks, DateTimeKind.Utc);
            var now = timeService.Now;
            var minutes = timeService.CalculateOfflineMinutes(lastExit, now, balance.offlineCapHours);
            if (minutes <= 0f)
            {
                return;
            }

            playerProgress.ApplyIdleXp(minutes, playerProgress.GetTotalIdleSkillBonus());
            gameHud?.ShowIdlePopup(minutes, balance.baseIdleXpPerMinute, playerProgress.GetTotalIdleSkillBonus());
        }

        private void PersistData()
        {
            if (saveDataManager == null || playerProgress == null)
            {
                return;
            }

            playerProgress.Data.lastExitTicks = DateTime.UtcNow.Ticks;
            saveDataManager.Save(playerProgress.Data);
        }
    }
}
