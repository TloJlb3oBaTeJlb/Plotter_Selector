using PlotterDbLib;

namespace TestServer
{

    internal class TestServer
    {

        private static PlotterDbAdminClient client = null!;

        static async Task Main(string[] args)
        {
            PlotterDbServer server = new(false, "../../../../plotters.db");
            Console.WriteLine("Server boots up...");
            server.StartAsync();
            while (!server.IsRunning) { }

            await RunAdminConsole();

            server.Stop();
        }

        // I know this is bad, but for now it is good enough.
        static async Task RunAdminConsole()
        {
            client = new();

            while (true)
            {
                Console.WriteLine("Options:");
                ConsoleIO.ShowOptions(["Show", "Add", "Change", "Remove", "Exit and stop server"]);
                Console.Write("Your choice: ");
                var key = Console.ReadKey().KeyChar;
                Console.Write('\n');
                switch (key)
                {
                    case '1':
                        await ShowPlotters();
                        break;
                    case '2':
                        await AddPlotter();
                        break;
                    case '3':
                        await ShowPlotters();
                        await ChangePlotter();
                        break;
                    case '4':
                        await ShowPlotters();
                        await DeletePlotter();
                        break;
                    case '5':
                        return;
                    default:
                        Console.WriteLine("Incorrect input. Try again");
                        break;
                }
            }
        }


        static Plotter GetPlotter()
        {
            Plotter plotter = new()
            {
                Model = ConsoleIO.GetString("1/15 Model: "),
                Manufacturer = ConsoleIO.GetString("2/15 Manufacturer: "),
                PaperFormat = ConsoleIO.GetEnum<PaperFormat>("3/15 Paper format: "),
                Material = ConsoleIO.GetEnum<Material>("4/15 Material: "),
                Dimensions = ConsoleIO.GetString("5/15 Dimensions: "),
                Addendum = ConsoleIO.GetString("6/15 Addendum: "),
                PathToImage = ConsoleIO.GetString("7/15 Path to image: "),
                PlotterType = ConsoleIO.GetEnum<PlotterType>("8/15 Plotter type: ", true),
                DrawingMethod = ConsoleIO.GetEnum<DrawingMethod>("9/15 Drawing method: ", true),
                Positioning = ConsoleIO.GetEnum<Positioning>("10/15 Positioning: ", true),
                PrintingType = ConsoleIO.GetEnum<PrintingType>("11/15 Printing type: ", true),
                Price = ConsoleIO.GetInt("12/15 Price: ", 1),
                Width = ConsoleIO.GetDouble("13/15 Width: ", 0.0),
                Weight = ConsoleIO.GetDouble("14/15 Weght: ", 0.0),
                HasHardDrive = ConsoleIO.GetBool("15/15 HasHardDrive: "),

            };

            return plotter;
        }


        static async Task ShowPlotters()
        {
            var plotters = await client.GetFilteredPlottersAsync(new());
            Console.WriteLine("Plotters:");
            plotters.ForEach(Console.WriteLine);
        }


        static async Task AddPlotter()
        {
            var plotter = GetPlotter();
            var res = await client.AddPlotterAsync(plotter);
            Console.WriteLine("Response: " + res.ReasonPhrase);
        }


        static async Task ChangePlotter()
        {
            int id = ConsoleIO.GetInt("Id: ");
            // this is disgusting, but to make it good is too much work.
            var changedPlotter = GetPlotter();
            changedPlotter.PlotterId = id;
            var res = await client.UpdatePlotterAsync(changedPlotter);
            Console.WriteLine("Response: " + res.ReasonPhrase);
        }


        static async Task DeletePlotter()
        {
            int id = ConsoleIO.GetInt("Id: ");
            Plotter deletePlotter = new() { PlotterId = id, Model = "" };
            var res = await client.RemovePlotterAsync(deletePlotter);
            Console.WriteLine("Response: " + res.ReasonPhrase);
        }
    }
}
