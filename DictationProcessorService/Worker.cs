using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using SendGrid;
using SendGrid.Helpers.Mail;

using DictationProcessorLib;

namespace DictationProcessorService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public string uploadDir;

        public string outputDir;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
    {
                // We ultimately resolve the actual services we use from the scope we create below.
                // This ensures that all services that were registered with services.AddScoped<T>()
                // will be disposed at the end of the service scope (the current iteration).
                using var scope = _serviceScopeFactory.CreateScope();

                var uploadProcessor = scope.ServiceProvider.GetRequiredService<UploadProcessor>();
                var sendGridClient = scope.ServiceProvider.GetRequiredService<ISendGridClient>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                
                List<string> processedFiles = uploadProcessor.Process();

                var message = new SendGridMessage
                {
                    Subject = "Greetings",
                    PlainTextContent = $"The following media files have been processed:\n\n - {String.Join("\n - ", processedFiles)}",
                    From = new EmailAddress(configuration["Email:From"]),
                };

                message.AddTo(configuration["Email:Recipient"]);

                await sendGridClient.SendEmailAsync(message, cancellationToken);

                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }
}
