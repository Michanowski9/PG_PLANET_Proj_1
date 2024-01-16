using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLANET_Proj_1
{
	internal class TraceLogger : ILogger
	{
		public TraceLogger() 
		{
		
		}

		public void Print(string message) 
		{
			Trace.WriteLine(message);
		}
	}
}
