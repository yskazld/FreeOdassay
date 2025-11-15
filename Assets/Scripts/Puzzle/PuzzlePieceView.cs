using UnityEngine;
using UnityEngine.UI;

namespace FreelanceOdyssey.Puzzle
{
    public class PuzzlePieceView : MonoBehaviour
    {
        public Image image;
        public Button button;
        [HideInInspector] public int x;
        [HideInInspector] public int y;
        [HideInInspector] public int type;

        private PuzzleBoard _board;

        private void Awake()
        {
            if (image == null)
            {
                image = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            }

            if (button == null)
            {
                button = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            }
        }

        public void Initialize(PuzzleBoard board, int x, int y)
        {
            _board = board;
            this.x = x;
            this.y = y;
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
        }

        public void SetColor(Color color)
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (image != null)
            {
                image.color = color;
            }
        }

        private void OnClick()
        {
            _board?.HandlePieceClicked(this);
        }
    }
}
