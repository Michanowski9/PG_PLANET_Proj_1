using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PLANET_Proj_1
{
	internal class Timer : ITimer
	{
		DispatcherTimer dispatcherTimer = new DispatcherTimer();
		ILogger logger;
		public Timer(ILogger logger)
		{
			this.logger = logger;
		}
		public void Start()
		{
			logger.Print("timer: start");
			dispatcherTimer.Start();
		}
		public void Stop()
		{
			logger.Print("timer: stop");
			dispatcherTimer.Stop();
		}
		public void AddFunc(EventHandler function)
		{
			logger.Print("timer: add func");
			dispatcherTimer.Tick += function;
		}
		public void SetInterval(int interval)
		{
			logger.Print("timer: set interval");
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(interval);
		}
	}
}
