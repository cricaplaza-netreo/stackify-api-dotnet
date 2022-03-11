using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using StackifyLib;
using static System.Net.WebRequestMethods;

namespace CoreConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory());
               //.AddJsonFile("Stackify.json", optional: true, reloadOnChange: true);

            var config = builder.Build();
            config.ConfigureStackifyLogging();
            // OR
            //StackifyLib.Config.SetConfiguration(config);

            //enable debug logging
            StackifyLib.Utils.StackifyAPILogger.OnLogMessage += StackifyAPILogger_OnLogMessage;
            StackifyLib.Utils.StackifyAPILogger.LogEnabled = true;

            //Config.ReadStackifyJSONConfig();

            //NLogTest();
            Console.WriteLine($"Stackify Config AppName: {StackifyLib.Config.AppName}");
            Console.WriteLine($"Stackify Config EnvironmentName: {StackifyLib.Config.Environment}");
            Console.WriteLine($"Stackify Config ApiKey: {StackifyLib.Config.ApiKey}");

            StackifyLib.ProfileTracer.SetApplicationName("Test Name");
            StackifyLib.ProfileTracer.SetEnvironmentName("Test Environment");

            var tracer = StackifyLib.ProfileTracer.CreateAsOperation("Operation Name", Trace.CorrelationManager.ActivityId.ToString());
            tracer.Exec(() =>   //FYI, async code is also supported via tracer.ExecAsync()
            {
                //Do some stuff here
                Task.Delay(1000).GetAwaiter().GetResult();
            });
            StackifyLib.Logger.Shutdown(); //best practice for console apps
            Console.ReadLine();
        }


        private static void NLogTest()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config");

            NLog.LogManager.Configuration = new XmlLoggingConfiguration(path, true);
            NLog.Logger nlog = LogManager.GetCurrentClassLogger();

            for (int i = 0; i < 100; i++)
            {
                //      StackifyLib.Logger.Queue("Debug", "Test message");
                //System.Threading.Thread.Sleep(1);
                nlog.Debug("Hello");
                //nlog.Debug(new { color = "red", int1 = 1 });
            }
        }

        private static void Log4NetTest()
        {
            XmlDocument log4netConfig = new XmlDocument();

            using (StreamReader reader = new StreamReader(new FileStream("log4net.config", FileMode.Open, FileAccess.Read)))
            {
                log4netConfig.Load(reader);
            }

            ILoggerRepository rep = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);

            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

            for (int i = 0; i < 100; i++)
            {
                //      StackifyLib.Logger.Queue("Debug", "Test message");
                //System.Threading.Thread.Sleep(1);
                log.Debug(new { color = "red", int1 = 1 });
            }

        }

        private static void StackifyAPILogger_OnLogMessage(string data)
        {
            Debug.WriteLine(data);
        }
    }
}
