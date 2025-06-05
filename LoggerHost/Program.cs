using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

enum LogLevel { TRACE, DEBUG, INFO, WARNING, ERROR }

class Program
{
    static LogLevel currentFilter = LogLevel.TRACE;
    static string logFile = null;
    static bool toConsole = true;
    static StreamWriter fileWriter = null;
    static bool running = true;

    static void Main()
    {
        Console.Title = "Log Console (Press 1-5 to change filter, Esc to exit)";
        var pipe = new NamedPipeServerStream("LoggerPipe", PipeDirection.In);

        pipe.WaitForConnection();
        var reader = new StreamReader(pipe);

        // Запускаем поток чтения клавиш
        Thread inputThread = new Thread(ReadKeyInput) { IsBackground = true };
        inputThread.Start();

        Console.WriteLine("Фильтр: " + currentFilter);

        while (running)
        {
            string line = reader.ReadLine();
            if (line == null) continue;

            if (line.StartsWith("__exit__"))
                break;

            if (line.StartsWith("__config__"))
            {
                var parts = line.Split('|');
                if (parts.Length == 3)
                {
                    logFile = parts[1];
                    toConsole = bool.Parse(parts[2]);

                    if (!string.IsNullOrWhiteSpace(logFile))
                        fileWriter = new StreamWriter(File.Open(logFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                        {
                            AutoFlush = true
                        };

                    Console.WriteLine($"[Конфигурация] Файл: {logFile}, Консоль: {toConsole}");
                }
                continue;
            }

            var level = ExtractLevel(line);
            if (level >= currentFilter)
            {
                if (toConsole) Console.WriteLine(line);
                if (fileWriter != null) fileWriter.WriteLine(line);
            }
        }

        running = false;
        fileWriter?.Dispose();
        Console.WriteLine("Завершение... Нажмите любую клавишу");
        Console.ReadKey();
    }

    static void ReadKeyInput()
    {
        while (running)
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.D1: currentFilter = LogLevel.TRACE; break;
                case ConsoleKey.D2: currentFilter = LogLevel.DEBUG; break;
                case ConsoleKey.D3: currentFilter = LogLevel.INFO; break;
                case ConsoleKey.D4: currentFilter = LogLevel.WARNING; break;
                case ConsoleKey.D5: currentFilter = LogLevel.ERROR; break;
                default: continue;
            }

            Console.Title = $"Log Console [Filter: {currentFilter}] (1-5 to filter)";
            Console.WriteLine($"[Фильтр обновлён] -> {currentFilter}");
        }
    }

    private static LogLevel ExtractLevel(string log)
    {
        foreach (LogLevel lvl in Enum.GetValues(typeof(LogLevel)))
        {
            if (log.Contains($"| {lvl} ->")) return lvl;
        }
        return LogLevel.TRACE;
    }
}
