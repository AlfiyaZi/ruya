﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Menu;
using Ruya.Configuration;
using Ruya.Core;
using Ruya.Diagnostics;
using Ruya.Domain;
using Ruya.Net;
using Ruya.Scheduler;
using Ruya.Host.Properties;

namespace Ruya.Host
{
    internal class Program
    {
        private static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        private static readonly FileVersionInfo FileVersionInfo = ExecutingAssembly.GetFileVersionInfo();
        public static string ServiceName = FileVersionInfo.ProductName;

        private static void Main(string[] args)
        {
            using (var mutex = new Mutex(false, ExecutingAssembly
                                                        .GetName()
                                                        .Name))
            {
                const int mutexTimeout = 2;
                if (!mutex.WaitOne(TimeSpan.FromSeconds(mutexTimeout), false))
                {
                    // HARD-CODED constant
                    Debug.WriteLine("Another instance is running.");
                    return;
                }
                RunProgram(args);
            }
        }

        [Conditional("DEBUG")]
        private void DeleteDefaultTraceLogFile()
        {           
            TraceListener traceListener = Trace.Listeners["xmlTextWriter"];
            FieldInfo fieldInfo = traceListener?.GetType()
                                                .GetField("initializeData", BindingFlags.Instance | BindingFlags.NonPublic);
            var logFile = (string)fieldInfo?.GetValue(traceListener);
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }

        [Conditional("DEBUG")]
        private static void DeleteLogFile()
        {
            string logFile = Tracer.Instance.GetAttributeValue(null, "rollingXmlWriter", "initializeData");
            Tracer.Instance.CloseAll();
            if (logFile != null)
            {
                Ruya.IO.FileHelper.DeleteFile(logFile);
            }
            //x Debug.Listeners.Add(new ConsoleTraceListener());
            Tracer.Instance.TraceInformation(Ruya.Diagnostics.Properties.Resources.RollingXmlWriterTraceListener_CutOff);
        }


        private static void RunProgram(string[] args)
        {
            //!++ remove the directive before publish
            DeleteLogFile();

            // directory
            //x Directory.SetCurrentDirectory(Path.GetDirectoryName(ExecutingAssembly.Location));
            DomainHelper.DetectCurrentDirectory();

            // unhandled exceptions
            var unhandledExceptionHelper = new UnhandledExceptionHelper();
            if (!Debugger.IsAttached)
            {
                unhandledExceptionHelper.Register();
            }

            // encrypt the config
            DomainHelper.ProtectSection(args, ExecutingAssembly);

            // service || console
            if (Environment.UserInteractive)
            {
                // running as console app
                Start(null, EventArgs.Empty);
                Run(args);
                Stop(null, EventArgs.Empty);
            }
            else
            {
                // running as service
                var servicesToRun = new ServiceBase[]
                                    {
                                        new Service1(ServiceName)
                                    };
                ServiceBase.Run(servicesToRun);
            }

            Tracer.Instance.CloseAll();
        }

        private static string GetCopyrightMessage()
        {
            string configuration = ExecutingAssembly.GetConfigurationAttribute();
            string output = string.Format(CultureInfo.InvariantCulture, Resources.Message_Copyright, FileVersionInfo.ProductName, FileVersionInfo.ProductVersion, FileVersionInfo.LegalCopyright, FileVersionInfo.LegalTrademarks, FileVersionInfo.CompanyName, configuration);
            return output;
        }

        internal static void Start(object sender, EventArgs eventArgs)
        {
            Tracer.Instance.TraceInformation(GetCopyrightMessage());
        }

        internal static void Stop(object sender, EventArgs eventArgs)
        {
            _job.StopJob();
            Server.Stop();
            Tracer.Instance.TraceInformation(new string('~', 79));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
        internal static void Run(string[] args)
        {
            Action defaultAction = DefaultAction;
            if (!Environment.UserInteractive)
            {
                defaultAction();
            }
            else
            {
                var menu = new MenuCollection(GetCopyrightMessage(),
#if DEBUG
                                              true
#else
                                          false
#endif
                    )
                           {
                               defaultAction,
#if !DEBUG
                               InstallService,
                               UnInstallService,
#else
                               DebuggerHelper.CallDebugger,
                               //XmlTraceSourceSample
                               //ConfigReader,
                               //SchemaReader,
                               //Draft.TraceTester
#endif
                };
                menu.Run();
            }

        }

        [Description("Install Service")]
        internal static void InstallService()
        {
            ManagedInstallerClass.InstallHelper(new[] { ExecutingAssembly.Location });
        }

        [Description("Uninstall Service")]
        internal static void UnInstallService()
        {
            const string uninstallParameter = "/u";
            ManagedInstallerClass.InstallHelper(new [] { uninstallParameter, ExecutingAssembly.Location });
        }

        [Description("Default Action")]
        internal static void DefaultAction()
        {
#warning Refactor
            ConfigurationProvider configurationProvider = new ConfigurationProvider().SetAssembly(ExecutingAssembly);
            // HARD-CODED constant
            string serverAddressFromConfiguration = configurationProvider.GetSettingValue("ServerAddress");

            List<string> serverAddresses = serverAddressFromConfiguration.SplitCSV()
                                                                         .Select(address => address.Contains(Constants.WildCardStar)
                                                                                                ? address
                                                                                                : new FiddlerSupportedUri(address).Address.ToString())
                                                                         .ToList();
            Server.Start(serverAddresses, TaskToWork, true);
        }

        public static KeyValuePair<TimeSpan, int> GetJobSettings()
        {
            ConfigurationProvider configurationProvider = new ConfigurationProvider().SetAssembly(ExecutingAssembly);
            // HARD-CODED constant
            string interval = configurationProvider.GetSettingValue("JobInterval");
            TimeSpan serviceInterval = TimeSpanHelper.GetInterval(interval);
            // HARD-CODED constant
            string maxSkipAllowed = configurationProvider.GetSettingValue("JobMaxSkipAllowed");
            var output = new KeyValuePair<TimeSpan, int>(serviceInterval, Convert.ToInt32(maxSkipAllowed));
            return output;
        }

        private static Job _job;

        private static void TaskToWork()
        {
            KeyValuePair<TimeSpan, int> jobSettings = GetJobSettings();
            _job = new Job
                   {
                       // HARD-CODED constant
                       Name = MethodBase.GetCurrentMethod()
                                        .Name,
                       Interval = jobSettings.Key,
                       MaxSkipAllowed = jobSettings.Value
                   };
            _job.Skip += SkipJob;
            _job.SetJob(TaskToRepeat)
                .StartJob();
        }

        private static void SkipJob(object sender, SkipEventArgs e)
        {
            // HARD-CODED constant
            Tracer.Instance.TraceInformation($"Skip count #{e.Counter}");
        }

        private static void TaskToRepeat()
        {
            // HARD-CODED constant
            string activityStartTime = MethodBase.GetCurrentMethod()
                                                 .Name + "_" + DateTime.UtcNow.ToString("s");
            Tracer.Instance.TraceInformation(activityStartTime);

            Guid prevActivityId = Trace.CorrelationManager.ActivityId;
            Guid newActivityId = Guid.NewGuid();
            Trace.CorrelationManager.ActivityId = newActivityId;
            Tracer.Instance.TraceEvent(TraceEventType.Start, 0, activityStartTime);

            Tests.RunConsoleTests();

            Tracer.Instance.TraceEvent(TraceEventType.Stop, 0, activityStartTime);
            Trace.CorrelationManager.ActivityId = prevActivityId;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Description("TraceSource Test")]
        private static void XmlTraceSourceSample()
        {
            XmlWriterTraceListener xmlTraceListener = null;
            XmlWriterTraceListener xmlTraceListenerWithCallStack = null;
            TraceSource mySource = null;

            try
            {
                mySource = new TraceSource("TestSource")
                           {
                               Switch = new SourceSwitch("switch1", "All")
                           };
                const TraceOptions baseTraceOutputOptions = TraceOptions.DateTime | TraceOptions.LogicalOperationStack | TraceOptions.ProcessId | TraceOptions.ThreadId | TraceOptions.Timestamp;
                xmlTraceListener = new XmlWriterTraceListener(@"C:\inetpub\wwwroot\des\dupage\Logs\Output.svclog")
                                   {
                                       TraceOutputOptions = baseTraceOutputOptions,
                                       Filter = new EventTypeFilter(SourceLevels.All)
                                   };
                xmlTraceListenerWithCallStack = new XmlWriterTraceListener(@"C:\inetpub\wwwroot\des\dupage\Logs\OutputWithCallStack.svclog")
                                                {
                                                    TraceOutputOptions = baseTraceOutputOptions | TraceOptions.Callstack,
                                                    Filter = new EventTypeFilter(SourceLevels.Error)
                                                };
                mySource.Listeners.Add(xmlTraceListener);
                mySource.Listeners.Add(xmlTraceListenerWithCallStack);

                const string testString = "<Test><InnerElement Val=\"1\" /><InnerElement Val=\"Data\"/><AnotherElement>11</AnotherElement></Test>";
                var unEscapedXmlDiagnosticData = new UnescapedXmlDiagnosticData(testString);

                mySource.TraceEvent(TraceEventType.Start, 0, "Main Entry");

                mySource.TraceInformation("Hello World!");
                mySource.TraceEvent(TraceEventType.Error, 5, "Hello World!");
                mySource.TraceData(TraceEventType.Critical, 11, 1, 2, 3);
                mySource.TraceData(TraceEventType.Information, 11, 11, 214, 2.3, "Hello", 't', unEscapedXmlDiagnosticData);
                Guid prevActivityId = Trace.CorrelationManager.ActivityId;
                Guid newActivityId = Guid.NewGuid();
                Trace.CorrelationManager.ActivityId = newActivityId;
                mySource.TraceEvent(TraceEventType.Start, 0, "alpha");
                Trace.CorrelationManager.StartLogicalOperation("WorkerThread");
                mySource.Listeners[0].Filter = new SourceFilter("No match");
                mySource.TraceData(TraceEventType.Error, 5, "SourceFilter should reject this message for the trace listener.");
                mySource.TraceEvent(TraceEventType.Stop, 0, "zulu");
                Trace.CorrelationManager.ActivityId = prevActivityId;
                mySource.TraceTransfer(0, "transfer", newActivityId);
                Trace.CorrelationManager.StartLogicalOperation("AlphaThread");
                mySource.Listeners[0].Filter = new SourceFilter("TestSource");
                mySource.TraceData(TraceEventType.Error, 6, "SourceFilter should let this message through on the trace listener.");
                Trace.CorrelationManager.StopLogicalOperation();
                mySource.Listeners[0].Filter = null;
                Trace.CorrelationManager.StopLogicalOperation();
                mySource.TraceEvent(TraceEventType.Stop, 0, "Main Exit");
                mySource.Flush();
                mySource.Listeners.Clear();
                mySource.Close();
                xmlTraceListener = null;
                xmlTraceListenerWithCallStack = null;
            }
            finally
            {
                mySource = null;
                xmlTraceListener?.Dispose();
                xmlTraceListenerWithCallStack?.Dispose();

            }

        }

    }
}