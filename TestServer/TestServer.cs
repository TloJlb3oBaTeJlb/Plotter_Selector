using PlotterDbLib;

namespace TestServer
{
    internal class TestServer
    {
        static async Task Main(string[] args)
        {
            PlotterDbServer server = new();
            var task = server.RunAsync();

            while (!server.IsRunning) { }
            PlotterDbClient client = new();
            (await client.GetFilteredPlottersAsync(new())).ForEach(Console.WriteLine);

            await task;
        }
    }
}
