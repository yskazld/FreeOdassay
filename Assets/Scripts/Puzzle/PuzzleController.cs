using System;
using FreelanceOdyssey.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FreelanceOdyssey.Puzzle
{
    public class PuzzleController : MonoBehaviour
    {
        [Header("References")] public PuzzleBoard board;
        public Player.PlayerProgress playerProgress;
        public GameBalance balance;
        public Text timerText;
        public Text scoreText;
        public Text comboText;
        public Button finishButton;
        public Button crossSkillButton;
        public Button lineSkillButton;
        public Text crossSkillCooldownText;
        public Text lineSkillCooldownText;
        public RectTransform boardParent;

        [Header("Gameplay")] public float puzzleDuration = 60f;
        public int xpPerPiece = 5;
        public int coinsPerPiece = 3;
        public float crossSkillCooldown = 20f;
        public float lineSkillCooldown = 25f;

        public event Action<PuzzleResult> OnPuzzleFinished;

        private float _timeLeft;
        private int _score;
        private int _comboCount;
        private bool _isRunning;
        private float _crossCooldown;
        private float _lineCooldown;

        public void InitializeController()
        {
            if (board == null)
            {
                CreateBoard();
            }

            if (boardParent != null && board != null)
            {
                board.transform.SetParent(boardParent, false);
            }

            if (board != null)
            {
                board.Initialize(this);
            }

            finishButton?.onClick.RemoveListener(ForceFinish);
            finishButton?.onClick.AddListener(ForceFinish);
            crossSkillButton?.onClick.RemoveListener(UseCrossSkill);
            crossSkillButton?.onClick.AddListener(UseCrossSkill);
            lineSkillButton?.onClick.RemoveListener(UseLineSkill);
            lineSkillButton?.onClick.AddListener(UseLineSkill);
        }

        private void Update()
        {
            if (!_isRunning)
            {
                UpdateCooldowns();
                return;
            }

            _timeLeft -= Time.deltaTime;
            UpdateTimerLabel();
            UpdateCooldowns();

            if (_timeLeft <= 0f)
            {
                CompletePuzzle();
            }
        }

        public void BeginPuzzle()
        {
            if (board == null)
            {
                CreateBoard();
                board.Initialize(this);
            }

            _timeLeft = puzzleDuration;
            _score = 0;
            _comboCount = 0;
            _crossCooldown = 0f;
            _lineCooldown = 0f;
            _isRunning = true;
            board.ResetBoard();
            board.EnableInput(true);
            UpdateTimerLabel();
            UpdateScoreLabel();
            UpdateComboLabel(0);
            UpdateCooldownLabels();
        }

        public void OnPiecesCleared(int cleared, int combo)
        {
            var comboMultiplier = 1f + (combo - 1) * balance.comboRewardMultiplier;
            _comboCount = Mathf.Max(_comboCount, combo);
            var gainedScore = Mathf.RoundToInt(cleared * 10 * comboMultiplier);
            _score += gainedScore;
            UpdateScoreLabel();
            UpdateComboLabel(combo);
        }

        public void ForceFinish()
        {
            if (!_isRunning)
            {
                OnPuzzleFinished?.Invoke(BuildResult());
                return;
            }

            CompletePuzzle();
        }

        private void CompletePuzzle()
        {
            _isRunning = false;
            board.EnableInput(false);
            OnPuzzleFinished?.Invoke(BuildResult());
        }

        private PuzzleResult BuildResult()
        {
            var result = new PuzzleResult
            {
                score = _score,
                totalCombos = _comboCount
            };

            var xp = balance.basePuzzleXp + Mathf.RoundToInt(_score * 0.1f);
            var coins = balance.basePuzzleCoins + Mathf.RoundToInt(_score * 0.05f);
            result.totalXp = xp;
            result.totalCoins = coins;
            return result;
        }

        private void UpdateTimerLabel()
        {
            if (timerText != null)
            {
                timerText.text = $"残り: {Mathf.Max(0, Mathf.CeilToInt(_timeLeft))} 秒";
            }
        }

        private void UpdateScoreLabel()
        {
            if (scoreText != null)
            {
                scoreText.text = $"スコア: {_score}";
            }
        }

        private void UpdateComboLabel(int combo)
        {
            if (comboText != null)
            {
                comboText.text = combo <= 1 ? "コンボ: 1" : $"コンボ: x{combo}";
            }
        }

        private void UpdateCooldowns()
        {
            if (_crossCooldown > 0f)
            {
                _crossCooldown -= Time.deltaTime;
                if (_crossCooldown <= 0f)
                {
                    _crossCooldown = 0f;
                    if (crossSkillButton != null)
                    {
                        crossSkillButton.interactable = true;
                    }
                }
            }

            if (_lineCooldown > 0f)
            {
                _lineCooldown -= Time.deltaTime;
                if (_lineCooldown <= 0f)
                {
                    _lineCooldown = 0f;
                    if (lineSkillButton != null)
                    {
                        lineSkillButton.interactable = true;
                    }
                }
            }

            UpdateCooldownLabels();
        }

        private void UpdateCooldownLabels()
        {
            if (crossSkillCooldownText != null)
            {
                crossSkillCooldownText.text = _crossCooldown > 0 ? $"{_crossCooldown:F0}s" : "READY";
            }

            if (lineSkillCooldownText != null)
            {
                lineSkillCooldownText.text = _lineCooldown > 0 ? $"{_lineCooldown:F0}s" : "READY";
            }
        }

        private void UseCrossSkill()
        {
            if (_crossCooldown > 0f || !_isRunning || board == null)
            {
                return;
            }

            var reduction = playerProgress != null ? playerProgress.GetSkillCooldownReduction() : 0f;
            _crossCooldown = crossSkillCooldown * Mathf.Max(0.1f, 1f - reduction);
            if (crossSkillButton != null)
            {
                crossSkillButton.interactable = false;
            }

            UpdateCooldownLabels();
            var centerX = board.width / 2;
            var centerY = board.height / 2;
            board.ClearCrossAt(centerX, centerY);
        }

        private void UseLineSkill()
        {
            if (_lineCooldown > 0f || !_isRunning || board == null)
            {
                return;
            }

            var reduction = playerProgress != null ? playerProgress.GetSkillCooldownReduction() : 0f;
            _lineCooldown = lineSkillCooldown * Mathf.Max(0.1f, 1f - reduction);
            if (lineSkillButton != null)
            {
                lineSkillButton.interactable = false;
            }

            UpdateCooldownLabels();
            var row = board.height / 2;
            board.ClearRow(row);
        }

        public void BindBoardParent(RectTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            boardParent = parent;
            if (board == null)
            {
                CreateBoard();
            }

            board.transform.SetParent(boardParent, false);
            var rect = board.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600f, 600f);
            rect.anchoredPosition = new Vector2(0f, 100f);
        }

        private void CreateBoard()
        {
            var parent = boardParent != null ? boardParent : transform as RectTransform;
            if (parent == null)
            {
                parent = gameObject.AddComponent<RectTransform>();
            }

            var boardObject = new GameObject("PuzzleBoard", typeof(RectTransform), typeof(PuzzleBoard));
            var rect = boardObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600f, 600f);
            rect.anchoredPosition = new Vector2(0f, 100f);
            board = boardObject.GetComponent<PuzzleBoard>();
        }
    }
}
