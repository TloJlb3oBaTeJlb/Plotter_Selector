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

            var plotters = await client.GetFilteredPlottersAsync(new() { Model = "a"});
            plotters.ForEach(Console.WriteLine);

            //Console.WriteLine(await client.AddPlotterAsync(plotters.First()));

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
