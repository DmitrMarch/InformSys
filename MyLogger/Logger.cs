namespace MyLogger
{
    public static class Logger
    {
        public static void WriteLog(string error, string text, 
            string logfileName="", bool console=true)
        {
            Console.WriteLine(error + ": " + text);
        }
    }
}
