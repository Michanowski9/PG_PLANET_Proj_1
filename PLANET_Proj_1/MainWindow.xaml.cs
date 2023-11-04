using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace PLANET_Proj_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // settings
        const int FIELD_SIZE = 10;
        private readonly Brush EMPTY_FIELD_COLOR = Brushes.Gray;
        private readonly Brush OCCUPIED_FIELD_COLOR = Brushes.Red;
        private readonly Brush FIELD_BORDER_COLOR = Brushes.LightGray;

        // variables
        List<bool[,]> moves = new List<bool[,]>();
        Rectangle[,] arena = null;
        const int WIDTH = FIELD_SIZE;
        const int HEIGHT = FIELD_SIZE;

        int currentFrame = 0;

        int arenaSizeX = 0;
        int arenaSizeY = 0;

        // functions
        public MainWindow()
        {
            InitializeComponent();
            Trace.WriteLine("Created main window.");
        }
        private void arena_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(Arena);

            int x = Convert.ToInt32(p.X) / WIDTH;
            int y = Convert.ToInt32(p.Y) / HEIGHT;

            FieldClick(x, y);

            Trace.WriteLine("pressed: x=" + x.ToString() + " y=" + y.ToString());
        }
        private void FieldClick(in int x, in int y)
        {
            var currentMove = moves[currentFrame];
            if (currentMove[x, y] == false)
            {
                currentMove[x, y] = true;
                arena[x, y].Fill = OCCUPIED_FIELD_COLOR;
            }
            else
            {
                currentMove[x, y] = false;
                arena[x, y].Fill = EMPTY_FIELD_COLOR;
            }
        }
        private void CreateArena_Click(object sender, RoutedEventArgs e)
        {
            SetInputSizeX();
            SetInputSizeY();

            CreateFields();
            CreateArena.IsEnabled = false;
            NextFrame.IsEnabled = true;

            Trace.WriteLine("Created arena. Button IsEnabled=false");
        }
        private void CreateFields()
        {
            moves.Add(new bool[arenaSizeX, arenaSizeY]);

            arena = new Rectangle[arenaSizeX, arenaSizeY];
            for (int y = 0; y < arenaSizeY; y++)
            {
                for (int x = 0; x < arenaSizeX; x++)
                {
                    var field = new Rectangle();

                    field.Name = "Field_" + x.ToString() + "_" + y.ToString();

                    // size
                    field.Width = WIDTH;
                    field.Height = HEIGHT;

                    // colors
                    field.Fill = EMPTY_FIELD_COLOR;
                    field.Stroke = FIELD_BORDER_COLOR;

                    // border
                    field.StrokeThickness = 1;
                    field.RadiusX = 2;
                    field.RadiusY = 2;

                    // position
                    Canvas.SetTop(field, y * HEIGHT);
                    Canvas.SetLeft(field, x * WIDTH);

                    arena[x, y] = field;
                    Arena.Children.Add(field);
                }
            }
        }
        private void SetInputSizeX()
        {
            if (int.TryParse(InputSizeX.Text.ToString(), out arenaSizeX))
            { }
            else
            {
                arenaSizeX = 10;
            }
        }
        private void SetInputSizeY()
        {
            if (int.TryParse(InputSizeY.Text.ToString(), out arenaSizeY))
            { }
            else
            {
                arenaSizeY = 10;
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {

        }
        private void PrevFrame_Click(object sender, RoutedEventArgs e)
        {
            if (currentFrame <= 0)
            {
                return;
            }
            currentFrame--;
            if(currentFrame <=0)
            {
                PrevFrame.IsEnabled = false;
            }
            moves.RemoveAt(moves.Count - 1);
            RefreshArena();
        }
        private void MakeMove()
        {
            var nextMove = new bool[arenaSizeX, arenaSizeY];

            for (int y = 0; y < arenaSizeY; y++)
            {
                for (int x = 0; x < arenaSizeX; x++)
                {
                    if (IsAlive(x, y) &&
                        (GetLiveNeighborsCount(x, y) == 2 || GetLiveNeighborsCount(x, y) == 3))
                    {
                        nextMove[x, y] = true;
                    }
                    else if (!IsAlive(x, y) && GetLiveNeighborsCount(x, y) == 3)
                    {
                        nextMove[x, y] = true;
                    }
                    else
                    {
                        nextMove[x, y] = false;
                    }
                }
            }
            moves.Add(nextMove);
            currentFrame++;
        }
        private void RefreshArena()
        {
            Trace.WriteLine("Refreshing. Frame:" + currentFrame.ToString());
            var current = moves[currentFrame];
            for (int y = 0; y < arenaSizeY; y++)
            {
                for (int x = 0; x < arenaSizeX; x++)
                {
                    if (current[x,y])
                    {
                        arena[x, y].Fill = OCCUPIED_FIELD_COLOR;
                    }
                    else
                    {
                        arena[x, y].Fill = EMPTY_FIELD_COLOR;
                    }
                }
            }
        }
        private void NextFrame_Click(object sender, RoutedEventArgs e)  
        {
            MakeMove();
            RefreshArena();
            PrevFrame.IsEnabled = true;
        }
        private bool IsAlive(in int x, in int y)
        {
            var current = moves[currentFrame];
            if (x < 0) return false;
            if (y < 0) return false;
            if (x >= arenaSizeX) return false;
            if (y >= arenaSizeY) return false;
            return current[x, y];
        }
        private int GetLiveNeighborsCount(in int x, in int y)
        {
            Point[] neighbors =
           {
                new Point(x - 1, y - 1),
                new Point(x - 1, y),
                new Point(x - 1, y + 1),
                new Point(x    , y - 1),
                new Point(x    , y + 1),
                new Point(x + 1, y - 1),
                new Point(x + 1, y),
                new Point(x + 1, y + 1)
            };

            int result = 0;
            foreach (var neighbor in neighbors)
            {
                if (IsAlive(Convert.ToInt32(neighbor.X), Convert.ToInt32(neighbor.Y)))
                {
                    result++;
                }
            }
            return result;
        }
    }
}