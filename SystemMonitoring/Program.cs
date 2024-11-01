using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

class Program
{
    static Random s_rand = new Random();
    static Meter s_meter = new("Sys.Monitoring", "1.0.0");

    static int s_cpuLoad;
    static int s_memLoad;
    static int s_discLoad;

    static void Main(string[] args)
    {
        //метрика нагрузки ЦП
        s_meter.CreateObservableGauge(
            name: "cpu-load",
            () => new Measurement<int>(
                s_cpuLoad,
                new KeyValuePair<string, object?>("name", "cpu")),
            unit: "percentages",
            description: "Real-time CPU load");

        //метрика нагрузки оперативной памяти
        s_meter.CreateObservableGauge(
            name: "memory-load",
            () => new Measurement<int>(
                s_memLoad,
                new KeyValuePair<string, object?>("name", "mem")),
            unit: "percentages",
            description: "Real-time Memory load");

        //метрика нагрузки диска
        s_meter.CreateObservableGauge(
            name: "disc-load",
            () => new Measurement<int>(
                s_discLoad,
                new KeyValuePair<string, object?>("name", "disc")),
            unit: "percentages",
            description: "Real-time Disc load");

        //используем конечную точку метрик Прометея на порту 9184
        using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Sys.Monitoring")
                .AddPrometheusHttpListener(options => options.UriPrefixes = new string[] { "http://localhost:9184/" })
                .Build();

        //запускаем Прометей
        Directory.SetCurrentDirectory("D:\\Software\\prometheus-2.53.2.windows-amd64");
        Process process = new Process();
        process.StartInfo.FileName = "prometheus.exe";
        process.StartInfo.UseShellExecute = true;
        process.Start();

        Console.WriteLine("Нажмите любую клавишу, чтобы остановить...");

        //обновляем метрики
        while (!Console.KeyAvailable)
        {
            Thread.Sleep(1000);

            s_cpuLoad = s_rand.Next(0, 100);
            s_memLoad = s_rand.Next(0, 100);
            s_discLoad = s_rand.Next(0, 100);
        }

        //убиваем процесс Прометея
        process.Kill();
    }
}
