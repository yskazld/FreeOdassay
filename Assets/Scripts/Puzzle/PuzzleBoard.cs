using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FreelanceOdyssey.Puzzle
{
    public class PuzzleBoard : MonoBehaviour
    {
        [Header("Board Settings")] public int width = 6;
        public int height = 6;
        public int pieceTypes = 6;
        public RectTransform gridContainer;
        public float swapAnimationDuration = 0.1f;

        [Header("Colors")] public List<Color> palette = new()
        {
            new(0.96f, 0.35f, 0.35f),
            new(0.95f, 0.67f, 0.2f),
            new(0.99f, 0.93f, 0.3f),
            new(0.32f, 0.77f, 0.36f),
            new(0.31f, 0.64f, 0.95f),
            new(0.74f, 0.46f, 0.95f)
        };

        private readonly List<PuzzlePieceView> _pieces = new();
        private readonly List<PuzzlePieceView> _selected = new();
        private int[,] _board;
        private PuzzleController _controller;

        public void Initialize(PuzzleController controller)
        {
            _controller = controller;
            BuildBoard();
            GenerateBoard();
        }

        public void ResetBoard()
        {
            foreach (var piece in _pieces)
            {
                Destroy(piece.gameObject);
            }

            _pieces.Clear();
            _selected.Clear();
            BuildBoard();
            GenerateBoard();
        }

        public void EnableInput(bool enabled)
        {
            foreach (var piece in _pieces)
            {
                if (piece.button != null)
                {
                    piece.button.interactable = enabled;
                }
            }
        }

        public void HandlePieceClicked(PuzzlePieceView piece)
        {
            if (!_pieces.Contains(piece))
            {
                return;
            }

            if (_selected.Contains(piece))
            {
                _selected.Remove(piece);
                Highlight(piece, false);
                return;
            }

            _selected.Add(piece);
            Highlight(piece, true);

            if (_selected.Count == 2)
            {
                TrySwap(_selected[0], _selected[1]);
                Highlight(_selected[0], false);
                Highlight(_selected[1], false);
                _selected.Clear();
            }
        }

        private void BuildBoard()
        {
            _board = new int[width, height];
            if (gridContainer == null)
            {
                gridContainer = GetComponent<RectTransform>();
            }

            var layout = gridContainer.GetComponent<GridLayoutGroup>();
            if (layout == null)
            {
                layout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
            }
            if (layout != null)
            {
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = width;
                if (layout.cellSize == Vector2.zero)
                {
                    layout.cellSize = new Vector2(100f, 100f);
                }
                if (layout.spacing == Vector2.zero)
                {
                    layout.spacing = new Vector2(4f, 4f);
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var piece = CreatePiece(x, y);
                    _pieces.Add(piece);
                }
            }
        }

        private PuzzlePieceView CreatePiece(int x, int y)
        {
            var go = new GameObject($"Piece_{x}_{y}", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(gridContainer, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            var piece = go.AddComponent<PuzzlePieceView>();
            piece.Initialize(this, x, y);
            return piece;
        }

        private void GenerateBoard()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int type;
                    do
                    {
                        type = Random.Range(0, pieceTypes);
                    } while (CreatesMatchAt(x, y, type));

                    SetPiece(x, y, type);
                }
            }
        }

        private void TrySwap(PuzzlePieceView a, PuzzlePieceView b)
        {
            if (!AreAdjacent(a, b))
            {
                return;
            }

            SwapPieces(a.x, a.y, b.x, b.y);
            var matches = FindMatches();
            if (matches.Count == 0)
            {
                SwapPieces(a.x, a.y, b.x, b.y);
                return;
            }

            ResolveMatches(matches, 1);
        }

        private void ResolveMatches(List<List<Vector2Int>> matches, int combo)
        {
            var cleared = 0;
            foreach (var group in matches)
            {
                foreach (var cell in group)
                {
                    SetPiece(cell.x, cell.y, -1, false);
                    cleared++;
                }
            }

            if (cleared > 0)
            {
                _controller?.OnPiecesCleared(cleared, combo);
                CollapseBoard();
                var followUp = FindMatches();
                if (followUp.Count > 0)
                {
                    ResolveMatches(followUp, combo + 1);
                }
            }
        }

        public void ClearCrossAt(int centerX, int centerY)
        {
            var clearedCells = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                clearedCells.Add(new Vector2Int(x, centerY));
            }

            for (int y = 0; y < height; y++)
            {
                if (y == centerY)
                {
                    continue;
                }

                clearedCells.Add(new Vector2Int(centerX, y));
            }

            ClearCells(clearedCells, 1);
        }

        public void ClearRow(int row)
        {
            var clearedCells = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                clearedCells.Add(new Vector2Int(x, row));
            }

            ClearCells(clearedCells, 1);
        }

        private void ClearCells(List<Vector2Int> cells, int combo)
        {
            foreach (var cell in cells)
            {
                if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
                {
                    continue;
                }

                SetPiece(cell.x, cell.y, -1, false);
            }

            _controller?.OnPiecesCleared(cells.Count, combo);
            CollapseBoard();
            var followUp = FindMatches();
            if (followUp.Count > 0)
            {
                ResolveMatches(followUp, combo + 1);
            }
        }

        private void CollapseBoard()
        {
            for (int x = 0; x < width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < height; y++)
                {
                    if (_board[x, y] >= 0)
                    {
                        if (writeY != y)
                        {
                            _board[x, writeY] = _board[x, y];
                            _board[x, y] = -1;
                            UpdatePieceVisual(x, writeY);
                            UpdatePieceVisual(x, y);
                        }
                        writeY++;
                    }
                }

                for (int y = writeY; y < height; y++)
                {
                    int type = Random.Range(0, pieceTypes);
                    SetPiece(x, y, type);
                }
            }
        }

        private void SwapPieces(int ax, int ay, int bx, int by)
        {
            var temp = _board[ax, ay];
            _board[ax, ay] = _board[bx, by];
            _board[bx, by] = temp;
            UpdatePieceVisual(ax, ay);
            UpdatePieceVisual(bx, by);
        }

        private void SetPiece(int x, int y, int type, bool updateVisual = true)
        {
            _board[x, y] = type;
            if (updateVisual)
            {
                UpdatePieceVisual(x, y);
            }
        }

        private void UpdatePieceVisual(int x, int y)
        {
            var piece = GetPiece(x, y);
            if (piece == null)
            {
                return;
            }

            piece.type = _board[x, y];
            piece.x = x;
            piece.y = y;

            var color = Color.black;
            if (piece.type >= 0 && piece.type < palette.Count)
            {
                color = palette[piece.type];
            }

            piece.SetColor(color);
        }

        private PuzzlePieceView GetPiece(int x, int y)
        {
            return _pieces.Find(p => p.x == x && p.y == y);
        }

        private List<List<Vector2Int>> FindMatches()
        {
            var matches = new List<List<Vector2Int>>();

            for (int y = 0; y < height; y++)
            {
                int run = 1;
                for (int x = 1; x < width; x++)
                {
                    if (_board[x, y] == _board[x - 1, y] && _board[x, y] >= 0)
                    {
                        run++;
                    }
                    else
                    {
                        if (run >= 3)
                        {
                            var group = new List<Vector2Int>();
                            for (int i = 0; i < run; i++)
                            {
                                group.Add(new Vector2Int(x - 1 - i, y));
                            }
                            matches.Add(group);
                        }
                        run = 1;
                    }
                }

                if (run >= 3)
                {
                    var group = new List<Vector2Int>();
                    for (int i = 0; i < run; i++)
                    {
                        group.Add(new Vector2Int(width - 1 - i, y));
                    }
                    matches.Add(group);
                }
            }

            for (int x = 0; x < width; x++)
            {
                int run = 1;
                for (int y = 1; y < height; y++)
                {
                    if (_board[x, y] == _board[x, y - 1] && _board[x, y] >= 0)
                    {
                        run++;
                    }
                    else
                    {
                        if (run >= 3)
                        {
                            var group = new List<Vector2Int>();
                            for (int i = 0; i < run; i++)
                            {
                                group.Add(new Vector2Int(x, y - 1 - i));
                            }
                            matches.Add(group);
                        }
                        run = 1;
                    }
                }

                if (run >= 3)
                {
                    var group = new List<Vector2Int>();
                    for (int i = 0; i < run; i++)
                    {
                        group.Add(new Vector2Int(x, height - 1 - i));
                    }
                    matches.Add(group);
                }
            }

            return matches;
        }

        private bool CreatesMatchAt(int x, int y, int type)
        {
            if (x >= 2)
            {
                if (_board[x - 1, y] == type && _board[x - 2, y] == type)
                {
                    return true;
                }
            }

            if (y >= 2)
            {
                if (_board[x, y - 1] == type && _board[x, y - 2] == type)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AreAdjacent(PuzzlePieceView a, PuzzlePieceView b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        }

        private void Highlight(PuzzlePieceView piece, bool highlight)
        {
            if (piece.image != null)
            {
                piece.image.color = highlight ? Color.white : palette[piece.type];
            }
        }
    }
}
