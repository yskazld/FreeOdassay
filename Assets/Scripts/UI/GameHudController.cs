using System.Text;
using FreelanceOdyssey.Core;
using FreelanceOdyssey.Player;
using UnityEngine;
using UnityEngine.UI;

namespace FreelanceOdyssey.UI
{
    public class GameHudController : MonoBehaviour
    {
        [Header("Panels")] public GameObject homePanel;
        public GameObject puzzlePanel;
        public GameObject resultPanel;
        public GameObject idlePopupPanel;

        [Header("Texts")] public Text statusText;
        public Text xpText;
        public Text coinsText;
        public Text bestScoreText;
        public Text idlePopupText;
        public Text resultScoreText;
        public Text resultRewardText;

        [Header("Buttons")] public Button startPuzzleButton;
        public Button backToHomeButton;

        private Puzzle.PuzzleController _puzzleController;
        private Font _defaultFont;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            EnsurePanels();
        }

        public void Initialize(Puzzle.PuzzleController puzzleController)
        {
            _puzzleController = puzzleController;
            if (startPuzzleButton != null)
            {
                startPuzzleButton.onClick.AddListener(StartPuzzle);
            }

            if (backToHomeButton != null)
            {
                backToHomeButton.onClick.AddListener(ShowHome);
            }

            if (_puzzleController != null)
            {
                _puzzleController.timerText = timerText;
                _puzzleController.scoreText = scoreText;
                _puzzleController.comboText = comboText;
                _puzzleController.finishButton = finishButton;
                _puzzleController.crossSkillButton = crossSkillButton;
                _puzzleController.lineSkillButton = lineSkillButton;
                _puzzleController.crossSkillCooldownText = crossSkillCooldownText;
                _puzzleController.lineSkillCooldownText = lineSkillCooldownText;
                _puzzleController.BindBoardParent(GetPuzzlePanelRect());
            }
        }

        public void UpdateStatus(PlayerProgress progress)
        {
            if (progress == null)
            {
                return;
            }

            if (statusText != null)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"職業: {progress.CurrentJob?.displayName ?? "???"}");
                builder.AppendLine($"レベル: {progress.Data.level}");
                builder.AppendLine($"次のレベルまで: {Mathf.CeilToInt(progress.GetXpRequiredForNextLevel() - progress.Data.currentXp)} XP");
                statusText.text = builder.ToString();
            }

            if (xpText != null)
            {
                xpText.text = $"XP: {Mathf.FloorToInt(progress.Data.currentXp)}";
            }

            if (coinsText != null)
            {
                coinsText.text = $"コイン: {progress.Data.coins}";
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = $"ベストスコア: {progress.Data.bestPuzzleScore}";
            }
        }

        public void ShowHome()
        {
            homePanel?.SetActive(true);
            puzzlePanel?.SetActive(false);
            resultPanel?.SetActive(false);
        }

        public void ShowPuzzle()
        {
            homePanel?.SetActive(false);
            puzzlePanel?.SetActive(true);
            resultPanel?.SetActive(false);
        }

        public void ShowResult(Puzzle.PuzzleResult result, int xp, int coins)
        {
            homePanel?.SetActive(false);
            puzzlePanel?.SetActive(false);
            resultPanel?.SetActive(true);

            if (resultScoreText != null)
            {
                resultScoreText.text = $"スコア: {result.score}";
            }

            if (resultRewardText != null)
            {
                resultRewardText.text = $"XP: {xp}\nコイン: {coins}";
            }
        }

        public void ShowIdlePopup(float minutes, float baseRate, float bonus)
        {
            if (idlePopupPanel == null || idlePopupText == null)
            {
                return;
            }

            idlePopupPanel.SetActive(true);
            var total = minutes * baseRate * (1f + bonus);
            idlePopupText.text = $"{minutes:F0}分ぶんのXPを獲得!\n合計XP: {total:F0}";
            CancelInvoke(nameof(HideIdlePopup));
            Invoke(nameof(HideIdlePopup), 4f);
        }

        private void HideIdlePopup()
        {
            idlePopupPanel?.SetActive(false);
        }

        private void StartPuzzle()
        {
            if (_puzzleController == null)
            {
                _puzzleController = FindObjectOfType<Puzzle.PuzzleController>();
            }

            if (_puzzleController == null)
            {
                Debug.LogWarning("PuzzleController not found");
                return;
            }

            ShowPuzzle();
            _puzzleController.BeginPuzzle();
        }

        private void EnsurePanels()
        {
            if (homePanel == null)
            {
                homePanel = CreatePanel("HomePanel");
                statusText = CreateText(homePanel.transform, "StatusText", new Vector2(0.1f, 0.6f));
                xpText = CreateText(homePanel.transform, "XpText", new Vector2(0.1f, 0.5f));
                coinsText = CreateText(homePanel.transform, "CoinsText", new Vector2(0.1f, 0.4f));
                bestScoreText = CreateText(homePanel.transform, "BestScoreText", new Vector2(0.1f, 0.3f));
                startPuzzleButton = CreateButton(homePanel.transform, "StartPuzzleButton", "パズルを開始");
                startPuzzleButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -200f);
            }

            if (puzzlePanel == null)
            {
                puzzlePanel = CreatePanel("PuzzlePanel");
                timerText = CreateText(puzzlePanel.transform, "TimerText", new Vector2(0.05f, 0.9f));
                scoreText = CreateText(puzzlePanel.transform, "ScoreText", new Vector2(0.35f, 0.9f));
                comboText = CreateText(puzzlePanel.transform, "ComboText", new Vector2(0.65f, 0.9f));
                crossSkillButton = CreateButton(puzzlePanel.transform, "CrossSkillButton", "クロス");
                lineSkillButton = CreateButton(puzzlePanel.transform, "LineSkillButton", "ライン");
                crossSkillButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, -350f);
                lineSkillButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(200f, -350f);
                crossSkillCooldownText = CreateText(crossSkillButton.transform, "CrossCooldownText", new Vector2(0.5f, 0.5f));
                crossSkillCooldownText.rectTransform.anchoredPosition = new Vector2(0f, -50f);
                lineSkillCooldownText = CreateText(lineSkillButton.transform, "LineCooldownText", new Vector2(0.5f, 0.5f));
                lineSkillCooldownText.rectTransform.anchoredPosition = new Vector2(0f, -50f);
                finishButton = CreateButton(puzzlePanel.transform, "FinishButton", "リタイア");
                finishButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -450f);
            }

            if (resultPanel == null)
            {
                resultPanel = CreatePanel("ResultPanel");
                resultScoreText = CreateText(resultPanel.transform, "ResultScoreText", new Vector2(0.5f, 0.6f));
                resultRewardText = CreateText(resultPanel.transform, "ResultRewardText", new Vector2(0.5f, 0.5f));
                backToHomeButton = CreateButton(resultPanel.transform, "BackButton", "ホームに戻る");
                backToHomeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -200f);
            }

            if (idlePopupPanel == null)
            {
                idlePopupPanel = CreatePanel("IdlePopupPanel");
                idlePopupText = CreateText(idlePopupPanel.transform, "IdlePopupText", new Vector2(0.5f, 0.5f));
                idlePopupPanel.SetActive(false);
            }

            homePanel?.SetActive(true);
            puzzlePanel?.SetActive(false);
            resultPanel?.SetActive(false);
        }

        private GameObject CreatePanel(string name)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            var rect = panel.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return panel;
        }

        private Text CreateText(Transform parent, string name, Vector2 anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400f, 60f);
            var text = go.GetComponent<Text>();
            text.text = name;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 28;
            text.color = Color.white;
            if (_defaultFont != null)
            {
                text.font = _defaultFont;
            }

            return text;
        }

        private Button CreateButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.2f);
            rect.anchorMax = new Vector2(0.5f, 0.2f);
            rect.sizeDelta = new Vector2(240f, 70f);
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.23f, 0.42f, 0.85f);
            var text = CreateText(go.transform, "Label", new Vector2(0.5f, 0.5f));
            text.text = label;
            text.fontSize = 30;
            return go.GetComponent<Button>();
        }

        public RectTransform GetPuzzlePanelRect()
        {
            return puzzlePanel != null ? puzzlePanel.GetComponent<RectTransform>() : null;
        }
    }
}
