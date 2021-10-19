using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace service_wrapper
{
    public class Worker : BackgroundService
    {
        private IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = _configuration.GetValue<string>("Run:FileName");
                        process.StartInfo.Arguments = _configuration.GetValue<string>("Run:Arguments");
                        process.StartInfo.WorkingDirectory = _configuration.GetValue<string>("Run:WorkingDirectory");
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process.Start();

                        await process.WaitForExitAsync(stoppingToken); // Waits here for the process to exit.

                        process.CloseMainWindow();

                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.ToString());
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }
    }
}
