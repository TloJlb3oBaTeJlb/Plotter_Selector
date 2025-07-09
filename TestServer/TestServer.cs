using PlotterDbLib;

namespace TestServer
{
    internal class TestServer
    {
        static async Task Main(string[] args)
        {
            PlotterDbServer server = new();
            var task = server.StartAsync();

            while (!server.IsRunning) { }
            PlotterDbAdminClient client = new();
            // TODO: Unify requests on client
            //Console.WriteLine(await client.AddPlotterAsync());
            (await client.GetFilteredPlottersAsync(new() { Model = ""})).ForEach(Console.WriteLine);
            server.Stop();
        }
    }
}
