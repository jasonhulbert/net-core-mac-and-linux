using System;
using System.IO;
using System.Reflection;
using DictationProcessorLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DictationProcessorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config => config.AddUserSecrets(Assembly.GetExecutingAssembly()))
                .ConfigureServices((hostContext, services) =>
                {
                    var uploadProcessor = new UploadProcessor(new UploadProcessorOptions {
                        UploadDirectory = hostContext.Configuration.GetSection("App:UploadDirectory").Value,
                        OutputDirectory = hostContext.Configuration.GetSection("App:OutputDirectory").Value
                    });

                    uploadProcessor.Process();
                });
    }
}
