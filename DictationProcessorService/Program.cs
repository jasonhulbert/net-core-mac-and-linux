using System;
using System.Reflection;
using DictationProcessorLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid;

namespace DictationProcessorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config => config.AddUserSecrets(Assembly.GetExecutingAssembly()))
                .ConfigureServices((hostContext, services) =>
                {
                    var sendGridApiKey = hostContext.Configuration.GetSection("SendGrid:ApiKey").Value;
                    var uploadDirectory = hostContext.Configuration.GetSection("App:UploadDirectory").Value;
                    var outputDirectory = hostContext.Configuration.GetSection("App:OutputDirectory").Value;

                    if (string.IsNullOrWhiteSpace(sendGridApiKey))
                        throw new InvalidOperationException("Can not start the service without a valid SendGrid API key!");

                    services.AddScoped<ISendGridClient>(s => new SendGridClient(new SendGridClientOptions
                    {
                        ApiKey = sendGridApiKey
                    }));

                    services.AddScoped<UploadProcessor>((s => new UploadProcessor(new UploadProcessorOptions
                    {
                        UploadDirectory = uploadDirectory,
                        OutputDirectory = outputDirectory
                    })));

                    services.AddHostedService<Worker>();
                });
    }
}
