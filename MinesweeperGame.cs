using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Minesweeper
{
    public class MinesweeperGame
    {
        private readonly Action _end;

        public int Width { get; }
        public int Height { get; }
        public int CountBombs { get; }

        public DateTime StartOfGame { get; set; }

        public MinesweeperGame(Action end, int width, int height, int countBombs)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (countBombs <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (countBombs > (width * height) * 0.9)
                throw new ArgumentOutOfRangeException(nameof(countBombs), "Too many bombs, at least 10% of the map has to be not a bomb");

            _end = end ?? Application.Exit;
            Width = width;
            Height = height;
            CountBombs = countBombs;
        }

        public interface IContainerUpdate
        {
            void SetCovered();
            void SetBomb();
            void SetOpen(int countNumber);
            void SetFlag(bool hasFlag);
        }

        public class Field
        {
            public IContainerUpdate Box { get; set; }
            public bool IsBomb { get; set; }
            public bool HasFlag { get; set; }
            public bool IsOpen { get; set; }
            public int Number { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public Field[] Neighbours { get; set; }
        }

        public Random Rnd { get; set; }
        public Field[,] Fields { get; set; }

        public void InitGame(Func<int, int, IContainerUpdate> createContainer)
        {
            Rnd = new Random();
            Fields = new Field[Height, Width];

            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    Fields[y, x] = new Field
                    {
                        Box = createContainer(x, y),
                        X = x,
                        Y = y,
                    };

            var bombIndexes = new HashSet<int>(CountBombs);
            for (int i = 0; i < CountBombs; i++)
            {
                int bombIndex;
                do bombIndex = Rnd.Next(0, Height * Width - 1);
                while (!bombIndexes.Add(bombIndex));

                Fields[bombIndex / Width, bombIndex % Width].IsBomb = true;
            }

            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var f = Fields[y, x];
                    var n = GetNeighbours(x, y);
                    f.Number = n.Count(z => z?.IsBomb ?? false);
                    f.Neighbours = n.Select(z => z != f ? z : null).ToArray();
                }

            StartOfGame = DateTime.Now;
        }

        private Field[] GetNeighbours(int x, int y, bool excludeCenter = false)
        {
            return new[] {
                !(y - 1 < 0 || x - 1 < 0) ? Fields[y - 1, x - 1]: null,
                !(y - 1 < 0) ? Fields[y - 1, x]: null,
                !(y - 1 < 0 || x + 1 >= Width) ? Fields[y - 1, x + 1]: null,

                !(x - 1 < 0) ? Fields[y, x - 1] : null,
                excludeCenter ? null : Fields[y, x],
                !(x + 1 >= Width) ? Fields[y, x + 1]: null,

                !(y + 1 >= Height || x - 1 < 0) ? Fields[y + 1, x - 1]: null,
                !(y + 1 >= Height) ? Fields[y + 1, x]: null,
                !(y + 1 >= Height || x + 1 >= Width) ? Fields[y + 1, x + 1]: null
            };
        }

        private void OpenField(Field p, bool returnFirst = false)
        {
            if (p.IsOpen)
                return;

            p.IsOpen = true;
            p.Box.SetOpen(p.Number);

            if (returnFirst || p.Number != 0)
                return;

            foreach (Field field in p.Neighbours.Where(x => x != null))
                OpenField(field, field.Number != 0);
        }

        private void ShowBombs()
        {
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    if (Fields[y, x].IsBomb)
                        Fields[y, x].Box.SetBomb();
        }

        public void BoxClick(int x, int y, bool isRight)
            => BoxClick(Fields[y, x], isRight);

        private void BoxClick(Field p, bool isRight)
        {
            if (!isRight)
            {
                if (p.IsBomb)
                {
                    ShowBombs();

                    MessageBox.Show("Bomb! 💣💣💣"
                                    + Environment.NewLine
                                    + (DateTime.Now - StartOfGame).TotalSeconds.ToString("N1") + "s",
                        "Mistake",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    _end.Invoke();
                    
                    return;
                }

                OpenField(p);

                for (var y = 0; y < Height; y++)
                    for (var x = 0; x < Width; x++)
                    {
                        var f = Fields[y, x];
                        if (!(f.IsOpen || f.IsBomb))
                            return;
                    }

                ShowBombs();

                MessageBox.Show("You won 🏆"
                                + Environment.NewLine
                                + (DateTime.Now - StartOfGame).TotalSeconds.ToString("N1") + "s",
                    "Victory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _end.Invoke();

                return;
            }

            if (isRight && !p.IsOpen)
            {
                p.HasFlag = !p.HasFlag;
                p.Box.SetFlag(p.HasFlag);
            }
        }
    }
}
