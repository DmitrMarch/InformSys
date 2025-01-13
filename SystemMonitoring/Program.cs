using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Management;

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

    static float s_discTotalGB = 500;
    static float s_discAvailableGB;
    static float s_discUsageGB;

    static public void GetCpuInfo(ref double cpuClockSpeed, ref string cpuName, 
        ref int cpuNumberOfCores)
    {
        using (ManagementObject cpuMonitor = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
        {
            cpuName = Convert.ToString(cpuMonitor["Name"]);
            cpuNumberOfCores = Convert.ToInt32(cpuMonitor["NumberOfCores"]);
            cpuClockSpeed = Convert.ToDouble(cpuMonitor["CurrentClockSpeed"]) / 1024;
            cpuClockSpeed = Math.Round(cpuClockSpeed, 2);
        }
    }

    static float GetTotalRamGB()
    {
        using (ManagementObject ramMonitor = new ManagementObject("Win32_OperatingSystem=@"))
        {
            return Convert.ToInt32(ramMonitor["TotalVisibleMemorySize"]) / (1024 * 1024);
        }
    }

    static void Main(string[] args)
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
            () => new Measurement<float>(
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
            () => new Measurement<float>(
                s_discUsageGB,
                new List<KeyValuePair<string, object?>>()
                {
                    new ("type", "Занято"),
                    new ("total", s_discTotalGB)
                }),
            unit: "GB",
            description: "Real-time disc usage");

        //используем конечную точку метрик Прометея на порту 9184
        using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Sys.Monitoring")
                .AddPrometheusHttpListener(options => options.UriPrefixes = new string[] { "http://localhost:9184/" })
                .Build();

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

        Console.WriteLine("Нажмите любую клавишу, чтобы завершить все дочерние процессы...");

        GetCpuInfo(ref s_cpuClockSpeed, ref s_cpuName, ref s_cpuNumberOfCores);

        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;
        cpuCounter = new PerformanceCounter("Процессор", "% загруженности процессора", "_Total");
        ramCounter = new PerformanceCounter("Память", "Доступно МБ");

        //обновляем метрики каждую секунду
        while (!Console.KeyAvailable)
        {
            Thread.Sleep(1000);

            s_cpuUsagePct = cpuCounter.NextValue();

            s_memAvailableGB = ramCounter.NextValue() / 1024;
            s_memUsageGB = s_memTotalGB - s_memAvailableGB;

            s_discAvailableGB = 100;
            s_discUsageGB = s_discTotalGB - s_discAvailableGB;
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

        Console.WriteLine("Все дочерние процессы завершены");
    }
}
