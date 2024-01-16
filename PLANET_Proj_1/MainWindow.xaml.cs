using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace PLANET_Proj_1
{
	public partial class MainWindow : Window
	{
		private PerformanceCounter makeMoveCounter;
		private PerformanceCounter refreshArenaCounter;
		private PerformanceCounter bornCounter;
		private PerformanceCounter deadCounter;

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

		ILogger logger;
		ITimer timer;

		// functions
		public MainWindow(ILogger logger, ITimer timer)
		{
			this.logger = logger;
			this.timer = timer;

			this.timer.AddFunc(timer_Tick);

			InitializeComponent();
			logger.Print("Created main window.");

			InitializeCounters();
		}
		
		private void InitializeCounters()
		{

			if (PerformanceCounterCategory.Exists("PLANET_Proj_1 Counters"))
			{
				PerformanceCounterCategory.Delete("PLANET_Proj_1 Counters");
			}
			CounterCreationDataCollection counters = new CounterCreationDataCollection();

			CounterCreationData makeMoveCounterData = new CounterCreationData();
			makeMoveCounterData.CounterName = "MakeMoveDuration";
			makeMoveCounterData.CounterType = PerformanceCounterType.ElapsedTime;
			counters.Add(makeMoveCounterData);

			CounterCreationData refreshArenaCounterData = new CounterCreationData();
			refreshArenaCounterData.CounterName = "RefreshArenaDuration";
			refreshArenaCounterData.CounterType = PerformanceCounterType.ElapsedTime;
			counters.Add(refreshArenaCounterData);

			CounterCreationData bornCounterData = new CounterCreationData();
			bornCounterData.CounterName = "BornCounter";
			bornCounterData.CounterType = PerformanceCounterType.NumberOfItems32;
			counters.Add(bornCounterData);

			CounterCreationData deadCounterData = new CounterCreationData();
			deadCounterData.CounterName = "DeadCounter";
			deadCounterData.CounterType = PerformanceCounterType.NumberOfItems32;
			counters.Add(deadCounterData);

			PerformanceCounterCategory.Create("PLANET_Proj_1 Counters", "Liczniki dla aplikacji PLANET_Proj_1", PerformanceCounterCategoryType.SingleInstance, counters);

			makeMoveCounter = new PerformanceCounter("PLANET_Proj_1 Counters", "MakeMoveDuration", false);
			refreshArenaCounter = new PerformanceCounter("PLANET_Proj_1 Counters", "RefreshArenaDuration", false);
			bornCounter = new PerformanceCounter("PLANET_Proj_1 Counters", "BornCounter", false);
			deadCounter = new PerformanceCounter("PLANET_Proj_1 Counters", "DeadCounter", false);

			logger.Print("Counters initialized.");
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
		private void timer_Tick(object sender, EventArgs e)
		{
			NextFrame();
		}
		private void NextFrame()
		{
			MakeMove();
			RefreshArena();
			PrevFrame.IsEnabled = true;
		}
		private void MakeMove()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			var nextMove = new bool[arenaSizeX, arenaSizeY];

			int born = 0;
			int dead = 0;
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

					if (moves[currentFrame][x, y] == true && nextMove[x, y] == false)
					{
						dead++;
					}
					else if (moves[currentFrame][x, y] == false && nextMove[x, y] == true)
					{
						born++;
					}
				}
			}
			Born.Content = born.ToString();
			Dead.Content = dead.ToString();
			bornCounter.RawValue = born;
			deadCounter.RawValue = dead;
			moves.Add(nextMove);
			currentFrame++;

			stopwatch.Stop();
			makeMoveCounter.IncrementBy(stopwatch.ElapsedMilliseconds);
		}
		private void RefreshArena()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			logger.Print("Refreshing. Frame:" + currentFrame.ToString());
			var current = moves[currentFrame];
			for (int y = 0; y < arenaSizeY; y++)
			{
				for (int x = 0; x < arenaSizeX; x++)
				{
					if (current[x, y])
					{
						arena[x, y].Fill = OCCUPIED_FIELD_COLOR;
					}
					else
					{
						arena[x, y].Fill = EMPTY_FIELD_COLOR;
					}
				}
			}
			Generation.Content = currentFrame.ToString();

			stopwatch.Stop();
			refreshArenaCounter.IncrementBy(stopwatch.ElapsedMilliseconds);
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
		private string GetFileName()
		{
			if (InputFileName.Text == "")
				return "DefaultFile";
			else
				return InputFileName.Text;
		}

		private void arena_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point p = Mouse.GetPosition(Arena);

			int x = Convert.ToInt32(p.X) / WIDTH;
			int y = Convert.ToInt32(p.Y) / HEIGHT;

			FieldClick(x, y);

			logger.Print("pressed: x=" + x.ToString() + " y=" + y.ToString());
		}
		private void CreateArena_Click(object sender, RoutedEventArgs e)
		{
			SetInputSizeX();
			SetInputSizeY();

			CreateFields();
			CreateArena.IsEnabled = false;
			NextFrameButton.IsEnabled = true;
			StartButton.IsEnabled = true;
			LoadButton.IsEnabled = false;
			SaveButton.IsEnabled = true;

			logger.Print("Created arena. Button IsEnabled=false");
		}
		private void NextFrame_Click(object sender, RoutedEventArgs e)
		{
			NextFrame();
		}
		private void Start_Click(object sender, RoutedEventArgs e)
		{
			StartButton.IsEnabled = false;
			StopButton.IsEnabled = true;

			int timerInterval = 0;
			if (int.TryParse(InputTimer.Text.ToString(), out timerInterval))
			{ }
			else
				timerInterval = 1000;

			timer.SetInterval(timerInterval);
			timer.Start();
		}
		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			StartButton.IsEnabled = true;
			StopButton.IsEnabled = false;

			timer.Stop();
		}
		private void PrevFrame_Click(object sender, RoutedEventArgs e)
		{
			if (currentFrame <= 0)
			{
				return;
			}
			currentFrame--;
			if (currentFrame <= 0)
			{
				PrevFrame.IsEnabled = false;
			}
			moves.RemoveAt(moves.Count - 1);

			Born.Content = "";
			Dead.Content = "";
			RefreshArena();
		}
		private void Save_Click(object sender, RoutedEventArgs e)
		{
			using (StreamWriter writer = new StreamWriter(GetFileName()))
			{
				writer.WriteLine(arenaSizeX.ToString() + " " + arenaSizeY.ToString());
				for (int y = 0; y < arenaSizeY; y++)
				{
					var row = "";
					for (int x = 0; x < arenaSizeX; x++)
					{
						row += moves[currentFrame][x, y] + " ";
					}
					writer.WriteLine(row);
				}
			}
		}
		private void Load_Click(object sender, RoutedEventArgs e)
		{
			string line = "";
			using (StreamReader sr = new StreamReader(GetFileName()))
			{
				line = sr.ReadLine();
				var size = line.Split(" ");

				arenaSizeX = Convert.ToInt32(size[0]);
				arenaSizeY = Convert.ToInt32(size[1]);
				CreateFields();

				for (int y = 0; (line = sr.ReadLine()) != null && y < arenaSizeY; y++)
				{
					var temp = line.Split(" ");
					for (int x = 0; x < arenaSizeX; x++)
					{
						if (temp[x] == "True")
						{
							moves[currentFrame][x, y] = true;
						}
						else if (temp[x] == "False")
						{
							moves[currentFrame][x, y] = false;
						}
					}
				}
			}
			RefreshArena();
		}
	}
}