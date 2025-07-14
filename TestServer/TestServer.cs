using PlotterDbLib;

namespace TestServer
{

    internal class TestServer
    {
        private static PlotterDbAdminClient client = null!;

        static void Main(string[] args)
        {
            PlotterDbServer server = new();
            Console.WriteLine("Server boots up...");
            server.StartAsync();
            while (!server.IsRunning) { }
            
            //await RunAdminConsole();

            Console.WriteLine("Press any key to stop server");
            Console.ReadKey();
            Console.Write('\b');
            server.Stop();
        }
    }
}
