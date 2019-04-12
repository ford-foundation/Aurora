using System;
using System.IO;
using System.Configuration;

namespace BAGIT_FILE_TRANSFER
{
    public static class Logger
    {        
        private static string _logFileName = ConfigurationManager.AppSettings["LogFileName"];
        private static string _logFileNameDateFormat = ConfigurationManager.AppSettings["LogFileNameDateFormat"];
        private static Object _LOCK = new Object();

        private static void Log(LogType logType, DateTime dateTime, String title, String body)
        {
            lock (_LOCK)
            {
                   var logDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Logs";
                var ArchiveDirectory = logDirectory + "\\Archive";
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                var logFileName = _logFileName + "_" + DateTime.Now.ToString(_logFileNameDateFormat);
                var logFile = Path.Combine(logDirectory, logFileName + ".txt");

                foreach (string fileName in Directory.GetFiles(logDirectory, _logFileName + "*.txt"))
                {
                    if (fileName != logFile)
                    {
                        if (!Directory.Exists(ArchiveDirectory))
                            Directory.CreateDirectory(ArchiveDirectory);
                        var archiveFileName = Path.Combine(ArchiveDirectory, Path.GetFileName(fileName));
                        if (File.Exists(archiveFileName))
                            File.Delete(archiveFileName);
                        File.Move(fileName, archiveFileName);
                    }
                }
                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine(String.Format("{0} {1} {2}", logType == LogType.Success ? "++++++++" : "--------", title, dateTime.ToString("MM/dd/yyyy HH:mm:ss")));
                    writer.WriteLine(body);
                    if (title != "Information")
                        writer.WriteLine(Environment.NewLine);
                }
            }
        }

        public static void LogSuccess(String Message)
        {
            Log(LogType.Success, DateTime.Now, LogType.Success.ToString(), Message);
        }
        public static void LogInformation(String Message)
        {
            Log(LogType.Success, DateTime.Now, LogType.Information.ToString(), Message);
        }

        public static void LogException(String stackTrace)
        {
            Log(LogType.Error, DateTime.Now, LogType.Error.ToString(), stackTrace);
        }
        public static string GetConfigValueByKey(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }

    public enum LogType
    {
        Success,
        Error,
        Information
    }
    public static class Utility
    {
        public static string GetConfigValueByKey(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
   }


