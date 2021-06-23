using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.IO;

namespace SCUDWebIntegration.Logger
{
    //singleton
    public class FileLoggerProvider : ILoggerProvider
    {
        #region consts
        const string FILE_CONFIG_SECTION = "File";
        const string FILE_LOG_PATH_KEY = "RelativeLogPath";
        const LogLevel DEFAULT_MIN = LogLevel.Information;
        static string DEFAULT_LOG_PATH = Path.Join(Environment.CurrentDirectory, "logs");
        #endregion

        private LogLevel actualDefMin;
        private string actualPath;
        private ImmutableDictionary<string, LogLevel> categoryLevels;

        public FileLoggerProvider(IConfiguration config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            #region FilePathSetting

            var logPath = config[FILE_LOG_PATH_KEY];
            if (string.IsNullOrEmpty(logPath))
            {
                logPath = DEFAULT_LOG_PATH;
            }

            var currentName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            for (int i = 0; i < int.MaxValue; i++)
            {
                var iTag = i == 0 ? "" : i.ToString();
                actualPath = Path.Join(logPath, $"{currentName}{iTag}.log");
                if (!File.Exists(actualPath)) break;
            }

            Directory.CreateDirectory(logPath);
            File.Create(actualPath).Dispose();

            #endregion

            #region MinLogLvlParse

            var logigingSection = config.GetSection("Logging");
            var tempDict = new Dictionary<string, LogLevel>();

            if (logigingSection.Exists())
            {

                var actualSection = logigingSection.GetSection(FILE_CONFIG_SECTION);
                var logLvlSection = logigingSection.GetSection("LogLevel");

                void ParseToDict(IConfigurationSection section)
                {
                    if (section.Exists())
                    {
                        foreach (var val in section.GetChildren())
                        {
                            if (Enum.TryParse(typeof(LogLevel), val.Value, out var valLvl))
                            {
                                tempDict.TryAdd(val.Key, (LogLevel)valLvl);
                            }
                        }
                    }
                }

                if (actualSection.Exists())
                {
                    actualSection = actualSection.GetSection("LogLevel");
                    ParseToDict(actualSection);
                }

                ParseToDict(logLvlSection);
            }

            if (!tempDict.TryGetValue("Default", out actualDefMin))
            {
                actualDefMin = DEFAULT_MIN;
            }
            else
            {
                tempDict.Remove("Default");
            }

            categoryLevels = tempDict.ToImmutableDictionary();

            #endregion
        }

        private void WriteText(string text)
        {
            //maybe by FileStream
            File.AppendAllText(actualPath, $"{text}{Environment.NewLine}");
        }

        public void Dispose()
        {
            //nothing
        }

        public ILogger CreateLogger(string categoryName)
        {

            if (categoryLevels.TryGetValue(categoryName, out var lvl))
            {
                return new FileLogger(lvl, categoryName, WriteText);
            }

            return new FileLogger(actualDefMin, categoryName, WriteText);
        }
    }
}
