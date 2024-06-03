using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minesweeper
{
    public class MinesweeperForm
    {

        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MinesweeperForm().Form);
        }

        public Form Form { get; set; }
        public Label GameText { get; set; }

        public Image BombImage = new Bitmap(Resource.bomb_solid, TileWidth / 2, TileHeight / 2);

        private const int TotalPadding = 10;
        private const int TilePadding = 2;
        private const int TileHeight = 50;
        private const int TileWidth = 50;

        public MinesweeperGame Game { get; set; }

        public MinesweeperForm()
        {
            Form = new Form
            {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                Text = "Simple C# Minesweeper 💣💣💣"
            };

            InitLobby();
        }

        private void InitLobby()
        {
            var easyBtn = new Button()
            {
                Text = "Easy",
                Size = new Size(200, 40),
                Location = new Point(TotalPadding, TotalPadding + 0),
            };
            easyBtn.Click += (sender, args) => InitGame(9, 9, 10);

            var mediumBtn = new Button()
            {
                Text = "Medium",
                Size = new Size(200, 40),
                Location = new Point(TotalPadding, TotalPadding + 50),
            };
            mediumBtn.Click += (sender, args) => InitGame(16, 16, 40);

            var hardBtn = new Button()
            {
                Text = "Hard",
                Size = new Size(200, 40),
                Location = new Point(TotalPadding, TotalPadding + 100),
            };
            hardBtn.Click += (sender, args) => InitGame(30, 16, 99);

            Form.ClientSize = new Size(200 + TotalPadding * 2, 160 + 37);

            Form.Controls.Clear();
            Form.Controls.Add(easyBtn);
            Form.Controls.Add(mediumBtn);
            Form.Controls.Add(hardBtn);

            Form.Visible = true;
        }

        private void InitGame(int width, int height, int countBombs)
        {
            Game = new MinesweeperGame(InitLobby, width, height, countBombs);
            Form.ClientSize = new Size(width * TileWidth + TotalPadding * 2, height * TileHeight + TotalPadding * 2 + TileHeight);
            Form.Controls.Clear();

            this.GameText = new Label()
            {
                Font = new Font(FontFamily.GenericMonospace, 20, FontStyle.Bold),
                Top = Form.ClientSize.Height - TileHeight - TotalPadding,
                Left = TotalPadding,
                Width = Form.ClientSize.Width - TotalPadding * 2,
                Height = TileHeight,
            };

            Form.Controls.Add(this.GameText);

            Game.InitGame(CreateContainer);

            UpdateText();
        }

        private class ContainerUpdate : Label, MinesweeperGame.IContainerUpdate
        {
            public MinesweeperForm MinesweeperForm { get; set; }

            public void SetCovered() => BackColor = Color.DarkGray;

            public void SetBomb()
            {
                BackColor = Color.Red;
                Image = MinesweeperForm.BombImage;
                ImageAlign = ContentAlignment.MiddleCenter;
            }

            public void SetOpen(int countNumber)
            {
                BackColor = Color.LightGray;
                Text = countNumber <= 0 ? "" : countNumber + "";
            }

            public void SetFlag(bool hasFlag) => BackColor = hasFlag ? Color.Fuchsia : Color.Gray;
        }

        private MinesweeperGame.IContainerUpdate CreateContainer(int x, int y)
        {
            var box = new ContainerUpdate
            {
                MinesweeperForm = this,
                BackColor = Color.Gray,
                Font = new Font(FontFamily.GenericMonospace, 20, FontStyle.Bold),
                Width = TileWidth - TilePadding,
                Height = TileHeight - TilePadding,
                Location = new Point(TotalPadding + x * TileWidth + TilePadding / 2, TotalPadding + y * TileHeight + TilePadding / 2),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            box.Click += (sender, args) =>
            {
                if (!(args is MouseEventArgs e))
                    return;
                if (e.Button == MouseButtons.Left)
                    HandleMouseClick(x, y, false);
                else if (e.Button == MouseButtons.Right)
                    HandleMouseClick(x, y, true);
            };

            Form.Controls.Add(box);

            return box;
        }

        private void HandleMouseClick(int x, int y, bool isRight)
        {
            Game.BoxClick(x, y, isRight);

            this.UpdateText();
        }

        private void UpdateText()
        {
            this.GameText.Text = $"Total bombs: {Game.CountBombs}x, Fields opened: {Game.CountOpenedFields} of {Game.Width * Game.Height}";
        }
    }
}
