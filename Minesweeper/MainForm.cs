using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class MainForm : Form
    {
        int MinesNumber = 10;
        int GameSize = 8;

        List<Point> MinePonits = new List<Point>();
        List<Point> EmptyPoints = new List<Point>();
        Dictionary<Point, int> PointAndValueDictionary = new Dictionary<Point, int>();
        
        Random rnd = new Random();

        bool GameJustStarted = true;

        /// <summary>
        /// Program starts here
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            AskForGameMode();

            for (int x = 0; x < GameSize; x++)
            {
                for (int y = 0; y < GameSize; y++)
                {
                    var ThisCellPoint = new Point(x, y);
                    // create buttons
                    Button CellButton = new Button();
                    CellButton.Name = PointToButtonName(ThisCellPoint);
                    CellButton.Size = new Size(40, 40);
                    CellButton.Location = new Point(3 + 40 * x, 3 + 40 * y); // 3+ to make a little bit more space on top and left sides of form
                    CellButton.FlatStyle = FlatStyle.Flat;
                    // ignore mouse over (do not change back color)
                    CellButton.FlatAppearance.MouseOverBackColor = CellButton.BackColor;
                    CellButton.BackColorChanged += (s, e) => {
                        CellButton.FlatAppearance.MouseOverBackColor = CellButton.BackColor;
                    };
                    Controls.Add(CellButton);
                }
            }

            // to make sure that all our buttons will be formatted correctly
            RefreshButtons();
        }

        /// <summary>
        /// Creates and shows form that asks user to choose difficulty
        /// </summary>
        void AskForGameMode()
        {
            Form DialogForm = new Form();

            Button BeginnerButton = new Button();
            BeginnerButton.Text = "Beginner (8x8, 10 mines)";
            BeginnerButton.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
            BeginnerButton.Click += (sender, e)=>{
                GameSize = 8;
                MinesNumber = 10;
                DialogForm.Close();
            };
            BeginnerButton.Location = new Point(5, 5);
            BeginnerButton.Size = new Size(300, 40);
            BeginnerButton.FlatStyle = FlatStyle.Flat;

            Button IntermediateButton = new Button();
            IntermediateButton.Text = "Intermediate (16x16, 40 mines)";
            IntermediateButton.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
            IntermediateButton.Click += (sender, e) => {
                GameSize = 16;
                MinesNumber = 40;
                DialogForm.Close();
            };
            IntermediateButton.Location = new Point(5, 50);
            IntermediateButton.Size = new Size(300, 40);
            IntermediateButton.FlatStyle = FlatStyle.Flat;

            Button ExpertButton = new Button();
            ExpertButton.Text = "Expert (24x24, 99 mines)";
            ExpertButton.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
            ExpertButton.Click += (sender, e) => {
                GameSize = 24;
                MinesNumber = 99;
                DialogForm.Close();
            };
            ExpertButton.Location = new Point(5, 95);
            ExpertButton.Size = new Size(300, 40);
            ExpertButton.FlatStyle = FlatStyle.Flat;

            DialogForm.AutoSize = true;
            DialogForm.Size = new Size(10, 10);
            DialogForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            DialogForm.MinimizeBox = false;
            DialogForm.MaximizeBox = false;
            DialogForm.ShowIcon = false;
            DialogForm.Text = "Minesweeper level";
            DialogForm.Controls.Add(BeginnerButton);
            DialogForm.Controls.Add(IntermediateButton);
            DialogForm.Controls.Add(ExpertButton);
            DialogForm.ShowDialog();
        }

        /// <summary>
        /// Reset all properties of buttons to start game again
        /// </summary>
        void RefreshButtons()
        {
            GameJustStarted = true;
            for (int x = 0; x < GameSize; x++)
            {
                for (int y = 0; y < GameSize; y++)
                {
                    var ThisCellPoint = new Point(x, y);
                    var CellButton = Controls[PointToButtonName(ThisCellPoint)];
                    CellButton.Text = "";
                    CellButton.Enabled = true;
                    CellButton.BackColor = Control.DefaultBackColor;
                    CellButton.ForeColor = Color.Black;
                    CellButton.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
                    CellButton.MouseUp -= Cell_MouseUp;
                    CellButton.MouseUp += Cell_MouseUp;
                }
            }
        }

        /// <summary>
        /// Spawns mines, calculates empty spaces and numbers for cells. 
        /// </summary>
        /// <param name="StartButtonName">Name of first clicked button. Only after clicking to the first button program starts to create mines so user will never loose at the beginning.</param>
        void StartTheGame(string StartButtonName)
        {
            var StartPoint = ButtonNameToPoint(StartButtonName);

            MinePonits = new List<Point>();
            EmptyPoints = new List<Point>();
            PointAndValueDictionary = new Dictionary<Point, int>();

            for (int i = 0; i < MinesNumber; i++)
            {
                Point Mine = new Point(rnd.Next(0, GameSize), rnd.Next(0, GameSize));
                if (MinePonits.Contains(Mine) || Mine == StartPoint) // "Mine == StartPoint" to avoid loose at the very first try
                    i--;
                else
                    MinePonits.Add(Mine);
            }

            for (int x = 0; x < GameSize; x++)
            {
                for (int y = 0; y < GameSize; y++)
                {
                    var ThisCellPoint = new Point(x, y);

                    // calculate all numbers
                    if (!MinePonits.Contains(ThisCellPoint))
                    {
                        var CellNumber = GetCellNumber(ThisCellPoint);
                        if (CellNumber == 0)
                            EmptyPoints.Add(ThisCellPoint);
                        else
                            PointAndValueDictionary.Add(ThisCellPoint, CellNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates how many mines around the cell.
        /// </summary>
        /// <param name="Point">Coordinates of cell.</param>
        /// <returns>Number of mines around the point</returns>
        int GetCellNumber(Point Point)
        {
            int MinX = Point.X > 0 ? Point.X - 1 : Point.X;
            int MaxX = Point.X < GameSize - 1 ? Point.X + 1 : Point.X;
            int MinY = Point.Y > 0 ? Point.Y - 1 : Point.Y;
            int MaxY = Point.Y < GameSize - 1 ? Point.Y + 1 : Point.Y;

            int Mines = 0;

            for (int x = MinX; x <= MaxX; x++)
            {
                for (int y = MinY; y <= MaxY; y++)
                {
                    var CheckPoint = new Point(x, y);
                    if (MinePonits.Contains(CheckPoint))
                        Mines++;
                }
            }

            return Mines;
        }

        /// <summary>
        /// Gets coordinates of button using its name
        /// </summary>
        /// <param name="ButtonName">Button name to get coordinates</param>
        /// <returns>Coordinates of the button</returns>
        Point ButtonNameToPoint(string ButtonName)
        {
            var Splitted = ButtonName.Split('_');
            var x = Convert.ToInt32(Splitted[1]);
            var y = Convert.ToInt32(Splitted[2]);
            var Point = new Point(x, y);
            return Point;
        }

        /// <summary>
        /// Gets name of the button using its coordinates
        /// </summary>
        /// <param name="ButtonPoint">Coordinates of the button</param>
        /// <returns>Button name</returns>
        string PointToButtonName(Point ButtonPoint)
        {
            return $"Cell_{ButtonPoint.X}_{ButtonPoint.Y}";
        }

        /// <summary>
        /// If user clicked to empty space this function will open all empty space around.
        /// </summary>
        /// <param name="StartPoint">Coordinates of button that was clicked by the user</param>
        void OpenEmptySpace(Point StartPoint)
        {
            var CellButton = this.Controls[PointToButtonName(StartPoint)];
            CellButton.Enabled = false;
            CellButton.BackColor = Color.LightGray;
            CellButton.Text = "";

            int MinX = StartPoint.X > 0 ? StartPoint.X - 1 : StartPoint.X;
            int MaxX = StartPoint.X < GameSize - 1 ? StartPoint.X + 1 : StartPoint.X;
            int MinY = StartPoint.Y > 0 ? StartPoint.Y - 1 : StartPoint.Y;
            int MaxY = StartPoint.Y < GameSize - 1 ? StartPoint.Y + 1 : StartPoint.Y;

            for (int x = MinX; x <= MaxX; x++)
            {
                for (int y = MinY; y <= MaxY; y++)
                {
                    var CheckPoint = new Point(x, y);
                    Button CheckButton = (Button)this.Controls[PointToButtonName(CheckPoint)];
                    if (CheckButton.Enabled)
                    {
                        if (EmptyPoints.Contains(CheckPoint))
                            OpenEmptySpace(CheckPoint);
                        else
                        {
                            SetNumberToCell(CheckButton);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If user clicked to a mine - game is over.
        /// This function will show all mines, 
        /// disable all buttons and ask user if he wants to start again.
        /// </summary>
        /// <param name="FailedButtonName">Button that user clicked with the mine inside</param>
        void GameOver(string FailedButtonName)
        {
            foreach (var MinePoint in MinePonits)
            {
                var ButtonName = PointToButtonName(MinePoint);
                var Button = this.Controls[ButtonName];
                if (Button.Text == "M")
                    Button.BackColor = Color.Green;
                else
                    Button.BackColor = Color.Yellow;
                Button.Font = new Font(FontFamily.GenericSansSerif, 4.5f);
                Button.Text = "MINE";
            }

            this.Controls[FailedButtonName].BackColor = Color.Red;
            this.Controls[FailedButtonName].Font = new Font(FontFamily.GenericSansSerif, 4.5f);
            this.Controls[FailedButtonName].Text = "MINE";

            for (int x = 0; x < GameSize; x++)
            {
                for (int y = 0; y < GameSize; y++)
                {
                    var Button = this.Controls[PointToButtonName(new Point(x, y))];
                    Button.Enabled = false;
                }
            }

            var Answer = MessageBox.Show("BOOOOM!\r\nDo you want to try again?", "Game over", MessageBoxButtons.YesNo);
            if (Answer == DialogResult.Yes)
                RefreshButtons();
        }
        
        /// <summary>
        /// When user clicked to a button - this function shows the number of mines around or opens empty space.
        /// </summary>
        /// <param name="CellButton">Button that was clicked by the user</param>
        void MouseLeftClick(Button CellButton)
        {
            if (CellButton.Text != "M") // to prevent missclicks
            {
                //CellButton.Enabled = true;
                CellButton.MouseUp -= Cell_MouseUp;
                CellButton.BackColor = Color.LightGray;
                var BtnPoint = ButtonNameToPoint(CellButton.Name);
                if (MinePonits.Contains(BtnPoint))
                    GameOver(CellButton.Name);
                else if (EmptyPoints.Contains(BtnPoint))
                    OpenEmptySpace(BtnPoint);
                else
                    SetNumberToCell(CellButton);
            }
        }

        /// <summary>
        /// Sets number of mines around a cell and changes its color according to a value
        /// </summary>
        /// <param name="CellButton">Button that should be changed to number of mines</param>
        void SetNumberToCell(Button CellButton)
        {
            var BtnPoint = ButtonNameToPoint(CellButton.Name);
            var PointValue = PointAndValueDictionary[BtnPoint];
            CellButton.Text = PointValue.ToString();
            CellButton.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
            CellButton.BackColor = Color.LightGray;
            CellButton.FlatAppearance.BorderColor = Color.Black;
            switch (PointValue)
            {
                case 1:
                    CellButton.ForeColor = Color.Blue;
                    break;
                case 2:
                    CellButton.ForeColor = Color.Green;
                    break;
                case 3:
                    CellButton.ForeColor = Color.Red;
                    break;
                case 4:
                    CellButton.ForeColor = Color.Purple;
                    break;
                case 5:
                    CellButton.ForeColor = Color.Maroon;
                    break;
                case 6:
                    CellButton.ForeColor = Color.Turquoise;
                    break;
                case 7:
                    CellButton.ForeColor = Color.Black;
                    break;
                case 8:
                    CellButton.ForeColor = Color.Gray;
                    break;
            }
        }

        /// <summary>
        /// Checks if user won the game. 
        /// User won if he marked all mines and there is no extra marks.
        /// </summary>
        void CheckForVictory()
        {
            int MPoints = 0;
            int MinesDetected = 0;
            for (int x = 0; x < GameSize; x++)
            {
                for (int y = 0; y < GameSize; y++)
                {
                    var ThisPoint = new Point(x, y);
                    var ThisButtonText = this.Controls[PointToButtonName(ThisPoint)].Text;
                    if (ThisButtonText == "M")
                    {
                        MPoints++;
                        if (MinePonits.Contains(ThisPoint))
                            MinesDetected++;
                    }
                }
            }
            if (MPoints == MinesDetected && MinesDetected == MinesNumber)
            {
                var Answer = MessageBox.Show("Congratulations!\r\nDo you want to play again?", "You win!", MessageBoxButtons.YesNo);
                if (Answer == DialogResult.Yes)
                    RefreshButtons();
            }
        }

        /// <summary>
        /// When user click to button with mouse left button - calls MouseLeftClick().
        /// When with right - marks as mine, sets question mark or remove mark.
        /// If the game is not started yet - calls StartTheGame() to create mines, numbers etc.
        /// </summary>
        private void Cell_MouseUp(object sender, MouseEventArgs e)
        {
            Button CellButton = (Button)sender;

            if (GameJustStarted)
            {
                GameJustStarted = false;
                StartTheGame(CellButton.Name);
            }

            if (CellButton.Enabled)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        MouseLeftClick(CellButton);
                        break;
                    case MouseButtons.Right:
                        switch (CellButton.Text)
                        {
                            case "M":
                                CellButton.Text = "?";
                                CheckForVictory();
                                break;
                            case "?":
                                CellButton.Text = "";
                                break;
                            default:
                                CellButton.Text = "M";
                                CheckForVictory();
                                break;
                        }
                        break;
                }
            }
        }
    }
}
