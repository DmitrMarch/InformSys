using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace MyLogger
{
    public enum LogLevel { TRACE, DEBUG, INFO, WARNING, ERROR }

    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        public static Logger Instance => _instance.Value;

        private Process _logProcess;
        private NamedPipeClientStream _pipe;
        private StreamWriter _writer;
        private LogLevel _currentLevel = LogLevel.TRACE;
        private bool _toConsole = false;
        private string _logFileName = "";

        private Logger() { }

        public void Configure(LogLevel level, string logFileName = "", bool toConsole = true)
        {
            _currentLevel = level;
            _toConsole = toConsole;
            _logFileName = logFileName;

            EnsureLoggerHostRunning();
            ConnectPipe();
        }

        public void Log(LogLevel level, string message)
        {
            if (level < _currentLevel) return;
            if (_writer == null) return;

            string formatted = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {level} -> {message}";
            _writer.WriteLine(formatted);
            _writer.Flush();
        }

        public void Stop()
        {
            try
            {
                _writer?.WriteLine("__exit__");
                _writer?.Flush();
            }
            catch { }

            _writer?.Dispose();
            _pipe?.Dispose();

            if (_logProcess != null && !_logProcess.HasExited)
            {
                _logProcess.WaitForExit(3000); // подождать, пока он сам завершится
                _logProcess.Dispose();
            }
        }

        private void EnsureLoggerHostRunning()
        {
            if (Process.GetProcessesByName("LoggerHost").Length == 0)
            {
                _logProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "LoggerHost\\LoggerHost.exe",
                    CreateNoWindow = false,
                    UseShellExecute = true
                });
                Thread.Sleep(500); // немного подождать, пока pipe откроется
            }
        }

        private void ConnectPipe()
        {
            _pipe = new NamedPipeClientStream(".", "LoggerPipe", PipeDirection.Out);
            _pipe.Connect(2000); // 2 секунды на соединение
            _writer = new StreamWriter(_pipe)
            {
                AutoFlush = true
            };

            // Отправить начальную конфигурацию
            _writer.WriteLine($"__config__|{_logFileName}|{_toConsole}");
        }
    }
}
