using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StartVNCExtender
{
    public class Program
    {
        static void ClearConsoleFirstLine(int y = 0)
        {
            Console.SetCursorPosition(0, y);
            Console.Write(" ");

            for (int i = 0; i < 48; i++)
            {
                Console.SetCursorPosition(i, y);
                Console.Write(' ');
            }
            Console.SetCursorPosition(0, y);
        }
        [STAThread]
        public static async Task Main(string[] args)
        {

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            

            string hostName = "Devtop1-W530";

            string[] WaitingAnim = new string[]
            {
                "Waiting for connection",
                "Waiting for connection.",
                "Waiting for connection..",
                "Waiting for connection...",
                "Waiting for connection....",
                "Waiting for connection.....",
                "Waiting for connection......",
                "Waiting for connection.......",
                "Waiting for connection........",
            };

            int waitIdx = 0;

            string host = "Devtop1-W530";
            int port = 5900;
            int pollInterval = 1000;

            Console.WriteLine($"Starting to poll for VNC server on {host}:{port}");

            // Define the callback as a lambda expression
            Action<bool> statusHandler = (isConnected) =>
            {
                if (isConnected)
                {
                    
                    Console.WriteLine("Server is active and ready!");
                }
                else
                {
                    string waitText = WaitingAnim[waitIdx];
                    if (waitIdx + 1 >= WaitingAnim.Length - 1)
                    {
                        waitIdx = 0;
                    }
                    else
                    {
                        waitIdx++;
                    }

                    ClearConsoleFirstLine(1);
                    Console.Write(waitText);

                    //Console.WriteLine("⏳ Server not found. Retrying...");
                }
            };

            // Call the polling method, passing the callback
            await VncChecker.PollForVncServerAsync(host, port, pollInterval, statusHandler);

            TightVNCAutomator.RunAutomation();

            Console.WriteLine("Bye bye...");
            Console.ReadLine();
        }
    }
}
