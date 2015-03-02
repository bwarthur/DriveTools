using System;
using System.Reflection;
using log4net;
using log4net.Config;

namespace DriveTools.Logging
{
    public static class Logging
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(Logging));

        public static void Start()
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SafireUtilities.Resources.EnhancedLogging.xml");
            XmlConfigurator.Configure(resourceStream);
            if (resourceStream != null) resourceStream.Close();
            //BasicConfigurator.Configure();
#if DEVELOPMENT
                var repository = (Hierarchy) Log.Logger.Repository;
                var appenders = Log.Logger.Repository.GetAppenders();
                foreach (var fileAppender in appenders.Where(appender => appender.GetType() == typeof (RollingFileAppender)).Cast<RollingFileAppender>())
                {
                    fileAppender.File = @"C:\FMO\Logs\Trace-" + Environment.UserName + ".log";
                    fileAppender.ActivateOptions();
                }
                repository.Configured = true;
#endif
            Log.Info("Logging is started!");
        }
    }
}
