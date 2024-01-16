using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PLANET_Proj_1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private IHost _host;
		public App()
		{
			_host = new HostBuilder()
						.ConfigureServices((context, services) =>
						{
							services.AddSingleton<ILogger, TraceLogger>();
							services.AddSingleton<ITimer, Timer>();
							services.AddSingleton<MainWindow>();
						})
						.Build();
		}
		private async void Application_Startup(object sender, StartupEventArgs e)
		{
			await _host.StartAsync();

			var mainWindow = _host.Services.GetService<MainWindow>();
			mainWindow.Show();
		}
		private async void Application_Exit(object sender, ExitEventArgs e)
		{
			using (_host)
			{
				await _host.StopAsync(TimeSpan.FromSeconds(5));
			}
		}
	}
}
