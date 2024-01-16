using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PLANET_Proj_1
{
	public interface ITimer
	{
		public void Start();
		public void Stop();
		public void AddFunc(EventHandler function);
		public void SetInterval(int interval);
	}
}
