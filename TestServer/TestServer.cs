using PlotterDbLib;

namespace TestServer
{
    internal class TestServer
    {
        static async Task Main(string[] args)
        {
            PlotterDbServer server = new();
            var task = server.StartAsync();

            // Test admin client
            while (!server.IsRunning) { }
            PlotterDbAdminClient client = new();

            var plotters = await client.GetFilteredPlottersAsync(new());
            plotters.ForEach(Console.WriteLine);

            //change test
            /*Plotter changed = new()
            {
                PlotterId = plotters.First().PlotterId,
                Model = "--Changed--"
            };
            Console.WriteLine(await client.ChangePlotterAsync(changed));//*/

            // del test
            //Console.WriteLine(await client.RemovePlotterAsync(plotters.First()));

            //plotters = await client.GetFilteredPlottersAsync(new());
            //plotters.ForEach(Console.WriteLine);

            server.Stop();
        }
    }
}
