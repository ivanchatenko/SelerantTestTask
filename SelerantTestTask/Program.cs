using System;
using System.Threading;
namespace SelerantTestTask
{
    class Program
    {
        static CancellationToken tokenGlobal;
        static bool isConsoleCleared = false;
        static object locker = new object();
        static string key = string.Empty;        
        static System.Timers.Timer timer = new System.Timers.Timer(); // 5 sec timer
        static void Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            tokenGlobal = cancellationTokenSource.Token;

            SetTimer();
            Console.WriteLine("Enter natural number: ");
            do
            {
                timer.Start(); 
                key = Console.ReadLine();

                if (int.TryParse(key, out int number))
                {
                    timer.Stop();
                    cancellationTokenSource = new CancellationTokenSource();
                    tokenGlobal = cancellationTokenSource.Token;
                    Thread thread = new Thread(new ParameterizedThreadStart(Iterate));
                    thread.Start(number);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            cancellationTokenSource.Cancel();
            Console.Read();
        }
        /// <summary>
        /// Set timer settings
        /// </summary>
        public static void SetTimer()
        {
            timer.Interval = 5000;
            timer.Elapsed += TimerEventProcessor;
            timer.AutoReset = false;
            timer.Enabled = true;
        }
        /// <summary>
        /// Iterates every second a value until user-passed goal is reached 
        /// </summary>
        /// <param name="obj"></param>
        public static void Iterate(object obj)
        {
            int goal = (int)obj;
            int cursorLine = Console.CursorTop - 1;
            for (int iterator = 0; iterator <= goal; iterator++)
            {
                if (tokenGlobal.IsCancellationRequested)
                {
                    ClearConsole();
                    ShowResult(goal, iterator > 0 ? iterator - 1 : 0);
                    break;
                }
                Console.SetCursorPosition(0, cursorLine);
                Console.WriteLine($"Task {goal}: {iterator}");
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// Shows result of iterations (only for manual stop)
        /// </summary>
        /// <param name="taskGoal"></param>
        /// <param name="taskPosition"></param>
        public static void ShowResult(int taskGoal, int taskPosition)
        {
            Console.WriteLine($"Result of task #{taskGoal} - {taskPosition}");
        }
        /// <summary>
        /// Clears console, but only by one thread
        /// </summary>
        public static void ClearConsole()
        {
            lock (locker)
            {
                if (!isConsoleCleared)
                {
                    isConsoleCleared = true;
                    Console.Clear();
                }
            }
        }        
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        public static void TimerEventProcessor(object obj, EventArgs args)
        {
            timer.Stop();
            Console.WriteLine("Hey, enter the number!");
        }
    }
}
