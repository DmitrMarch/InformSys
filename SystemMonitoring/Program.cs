using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Management;
using System.Threading;
using MyLogger;

class Program
{
    static Meter s_meter = new("Sys.Monitoring", "1.0.0");

    static float s_cpuUsagePct;
    static double s_cpuClockSpeed;
    static string s_cpuName = "";
    static int s_cpuNumberOfCores;

    static float s_memTotalGB = GetTotalRamGB();
    static float s_memAvailableGB;
    static float s_memUsageGB;

    static double s_discTotalGB;
    static double s_discAvailableGB;
    static double s_discUsageGB;

    static string s_processName = "";
    static int s_processId;
    static float s_processCpu;
    static float s_processMemMB;

    static public void GetCpuInfo(ref double cpuClockSpeed, ref string cpuName, 
        ref int cpuNumberOfCores)
    {
        using (ManagementObject cpuMonitor = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
        {
            cpuName = Convert.ToString(cpuMonitor["Name"])!;
            cpuNumberOfCores = Convert.ToInt32(cpuMonitor["NumberOfCores"]);
            cpuClockSpeed = Convert.ToDouble(cpuMonitor["CurrentClockSpeed"]) / 1024;
            cpuClockSpeed = Math.Round(cpuClockSpeed, 2);
        }
    }

    static float GetTotalRamGB()
    {
        using (ManagementObject ramMonitor = new ManagementObject("Win32_OperatingSystem=@"))
        {
            float total_ram_gb = (float)Convert.ToInt32(ramMonitor["TotalVisibleMemorySize"]) / 
                (1024 * 1024);
            return (float)Math.Round(total_ram_gb, 1);
        }
    }

    static void GetDiscSizeGB(ref double totalSize, ref double freeSize)
    {
        double gigabyte = Math.Pow(1024, 3);
        long total_size_bytes = 0;
        long free_size_bytes = 0;

        DriveInfo[] all_discs = DriveInfo.GetDrives();

        foreach (DriveInfo d in all_discs)
        {
            if (d.IsReady == true)
            {
                total_size_bytes += d.TotalSize;
                free_size_bytes += d.TotalFreeSpace;
            }
        }

        totalSize = Math.Round((double)total_size_bytes / gigabyte, 1);
        freeSize = Math.Round((double)free_size_bytes / gigabyte, 1);
    }

    static void Main()
    {
        //метрика текущей нагрузки ЦП
        s_meter.CreateObservableGauge(
            name: "cpu-usgae",
            () => new Measurement<float>(
                s_cpuUsagePct,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("type", "Нагрузка"),
                    new ("name", s_cpuName),
                    new ("number-of-cores", s_cpuNumberOfCores),
                    new ("cpu-clock-speed", s_cpuClockSpeed)
                }),
            unit: "percentages",
            description: "Real-time CPU usage");

        //метрика доступной оперативной памяти
        s_meter.CreateObservableGauge(
            name: "memory-available",
            () => new Measurement<float>(
                s_memAvailableGB,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("type", "Доступно"),
                    new ("total", s_memTotalGB)
                }),
            unit: "GB",
            description: "Real-time memory available");

        //метрика занятой оперативной памяти
        s_meter.CreateObservableGauge(
            name: "memory-usage",
            () => new Measurement<float>(
                s_memUsageGB,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("type", "Занято"),
                    new ("total", s_memTotalGB)
                }),
            unit: "GB",
            description: "Real-time memory usage");


        //метрика свободного места на диске
        s_meter.CreateObservableGauge(
            name: "disc-available",
            () => new Measurement<double>(
                s_discAvailableGB,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("type", "Доступно"),
                    new ("total", s_discTotalGB)
                }),
            unit: "GB",
            description: "Real-time disc available");

        //метрика занятого места на диске
        s_meter.CreateObservableGauge(
            name: "disc-usage",
            () => new Measurement<double>(
                s_discUsageGB,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("type", "Занято"),
                    new ("total", s_discTotalGB)
                }),
            unit: "GB",
            description: "Real-time disc usage");

        //метрика нагрузки процесса на ЦП
        s_meter.CreateObservableGauge(
            name: "process-cpu-usage",
            () => new Measurement<float>(
                s_processCpu,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("process_id", s_processId),
                    new ("process_name", s_processName)
                }),
            unit: "percentages",
            description: "Real-time running process");

        //метрика памяти, занятой процессом
        s_meter.CreateObservableGauge(
            name: "process-memory-usage",
            () => new Measurement<float>(
                s_processMemMB,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("process_id", s_processId),
                    new ("process_name", s_processName)
                }),
            unit: "MB",
            description: "Real-time running process");

        //используем конечную точку метрик Прометея на порту 9184
        using MeterProvider meter_provider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Sys.Monitoring")
                .AddPrometheusHttpListener(options => options.UriPrefixes = new string[] { "http://localhost:9184/" })
                .Build();

        //настройка логирования
        Logger.Instance.Configure(LogLevel.DEBUG, "app.log", toConsole: true);
        Logger.Instance.Log(LogLevel.INFO, "Приложение запущено");

        bool flag = true;

        if (flag)
        {
            //запускаем Прометея
            Directory.SetCurrentDirectory("D:\\Software\\Prometheus");
            Process prometheus = new Process();
            prometheus.StartInfo.FileName = "prometheus.exe";
            prometheus.StartInfo.UseShellExecute = true;
            prometheus.Start();

            //запускаем Графану и открываем её клиент в браузере
            Directory.SetCurrentDirectory("D:\\Software\\Grafana\\bin");
            Process grafana = new Process();
            grafana.StartInfo.FileName = "grafana-server.exe";
            grafana.StartInfo.UseShellExecute = true;
            grafana.Start();
            Process browser = new Process();
            browser.StartInfo.FileName = "http://localhost:3000";
            browser.StartInfo.UseShellExecute = true;
            browser.Start();

            //получаем постоянные системные данные
            GetCpuInfo(ref s_cpuClockSpeed, ref s_cpuName, ref s_cpuNumberOfCores);

            Console.WriteLine("Нажмите любую клавишу, чтобы завершить все дочерние процессы...");

            using (PerformanceCounter cpu_counter = new("Processor", "% Processor Time", "_Total"),
                ram_counter = new("Memory", "Available MBytes"))
            {

                //обновляем метрики каждую секунду
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(1000);

                    s_cpuUsagePct = cpu_counter.NextValue();

                    s_memAvailableGB = ram_counter.NextValue() / 1024;
                    s_memUsageGB = s_memTotalGB - s_memAvailableGB;

                    GetDiscSizeGB(ref s_discTotalGB, ref s_discAvailableGB);
                    s_discUsageGB = s_discTotalGB - s_discAvailableGB;

                    Logger.Instance.Log(LogLevel.INFO, $"Отправлена метрика Memory Total GB: {s_memTotalGB}");
                    Logger.Instance.Log(LogLevel.INFO, $"Отправлена метрика Memory Available GB: {s_memAvailableGB}");
                    Logger.Instance.Log(LogLevel.INFO, $"Отправлена метрика Memory Usage GB: {s_memUsageGB}");
                    Logger.Instance.Log(LogLevel.INFO, $"Отправлена метрика Disc Total GB: {s_discTotalGB}");
                    Logger.Instance.Log(LogLevel.INFO, $"Отправлена метрика Disc Usage GB: {s_discUsageGB}");

                    Process[] local_all = Process.GetProcesses();

                    foreach (Process proc in local_all)
                    {
                        if (proc.ProcessName == "")
                        {
                            continue;
                        }

                        s_processId = proc.Id;
                        s_processName = proc.ProcessName;

                        try
                        {
                            using (PerformanceCounter process_cpu = new("Process", "% Processor Time", s_processName),
                                process_ram = new("Process", "Working Set - Private", s_processName))
                            {

                                process_cpu.NextValue();
                                process_ram.NextValue();

                                s_processCpu = process_cpu.NextValue() / Environment.ProcessorCount;
                                s_processMemMB = process_ram.NextValue() / (1024 * 1024);
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            Logger.Instance.Log(LogLevel.WARNING, ex.Message);
                        }
                    }

                    Logger.Instance.Log(LogLevel.INFO, "Отправлены метрики нагрузки программ (процессов) на процессор и память");
                    Logger.Instance.Log(LogLevel.DEBUG, "Проверка уровня отладки");
                }
            }

            //убиваем процессы Прометея и Графаны
            prometheus.Kill();
            Process tk = new Process();
            tk.StartInfo.FileName = "taskkill.exe";
            tk.StartInfo.Arguments = "/F /T /IM grafana-server.exe";
            tk.StartInfo.UseShellExecute = true;
            tk.Start();
            grafana.WaitForExit();
            tk.Kill();

            Logger.Instance.Log(LogLevel.INFO, "Все дочерние процессы завершены");

            //убиваем логирование
            Logger.Instance.Stop();
        }
    }
}
