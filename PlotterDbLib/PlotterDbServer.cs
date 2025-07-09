using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;


namespace PlotterDbLib
{
    public class PlotterDbServer
    {
        public PlotterDbServer()
        {
            using var dbContext = new PlotterDbContext(DbPath);

            //dbContext.Database.EnsureDeleted();
            if (dbContext.Database.EnsureCreated()) SetUpDataBase(dbContext);
            else
            {
                var tested = new Plotter() { Model = "Test" };
                dbContext.Plotters.Add(tested);
                try
                {
                    dbContext.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Error while accessing database. " +
                        "Database will be rebuilt.");

                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.EnsureCreated();

                    SetUpDataBase(dbContext);
                }
                finally
                {
                    dbContext.Plotters.Remove(tested);
                    dbContext.SaveChanges();
                }
            }

                listener = new();
            listener.Prefixes.Add(Url);
            
            options = new JsonSerializerOptions { IncludeFields = true };
        }


        //public bool ContinueToRun { set; get; } = true;
        public string DbPath { init; get; } = "plotters.db";
        public string Url { init; get; } = "http://localhost:1111/";
        public bool IsRunning { get => listener.IsListening; }


        public async Task StartAsync()
        {
            listener.Start();
            Console.WriteLine("Server started with url - " + Url);

            while (true)
            {
                try
                {
                    while (true)
                    {
                        var context = await listener.GetContextAsync();
                        Console.WriteLine("Request received");

                        HandleRequestAsync(context);
                    }
                }
                catch (HttpListenerException)
                {
                    listener.Close();
                    Console.WriteLine("Server stopped");
                }
            }

        }


        public void Stop() => listener.Stop();


        private async void HandleRequestAsync(HttpListenerContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    await HandleGetAsync(context);
                    break;
                case "POST":
                    await HandlePostAsync(context);
                    break;
                default:
                    await SendResponseAsync(context.Response, "MethodNotAllowed", 405);
                    break;
            }

            Console.WriteLine("Request handled");
        }


        private async Task HandleGetAsync(HttpListenerContext context)
        {
            var response = context.Response;
            try
            {
                // stream
                /*var req = context.Request;
                byte[] buffer = new byte[req.ContentLength64];
                context.Request.InputStream.Read(buffer, 0, 
                (int)req.ContentLength64);
                buffer.ToString();//*/

                Filter filter = GetFilter(context.Request.Url);
                // forming filtered result
                string serialisedList =
                    JsonSerializer.Serialize(GetFiltered(filter), options);
                await SendResponseAsync(response, serialisedList);
            }
            catch (ArgumentNullException exception)
            {
                Console.WriteLine(exception.Message);
                await SendResponseAsync(response, "BadRequest", 400);
            }
            catch (JsonException exception)
            {
                Console.WriteLine(exception.Message);
                await SendResponseAsync(response, "BadRequest", 400);
            }
        }


        private async Task HandlePostAsync(HttpListenerContext context)
        {
            await SendResponseAsync(context.Response, "NotImplemented", 405);
        }


        private async Task SendResponseAsync(HttpListenerResponse response, 
            string content, int statusCode = 200)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.ContentLength64 = buffer.Length;
            response.ContentType = "Application/json";
            response.StatusCode = statusCode;

            using Stream output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();
        }


        private Filter GetFilter(Uri? url)
        {
            ArgumentNullException.ThrowIfNull(url, nameof(url));

            // query
            /*Console.WriteLine(url.Query);
            var query = HttpUtility.ParseQueryString(url.Query);
            Dictionary<string, string> dict = new();
            foreach (string key in query.Keys) dict[key] = query[key];//*/

            string serialisedFilter = new(url.LocalPath.Skip(1).ToArray());
            if (serialisedFilter == string.Empty) return new Filter();

            var obj = JsonSerializer.Deserialize<Filter>(serialisedFilter, options);
            
            if (obj == null) return new Filter();
            return (Filter)obj;

        }


        private List<Plotter> GetFiltered(Filter filter)
        {
            using PlotterDbContext db = new(DbPath);

            List<Plotter> result = [];

            foreach (var plotter in db.Plotters) 
                if (filter.IsSuitable(plotter)) result.Add(plotter);
            return result;
        }


        private static void SetUpDataBase(PlotterDbContext dbContext)
        {
            // Test data
            dbContext.AddRange([
                new Plotter
                {
                    Model = "canon",
                    Price = 1,
                    Positioning = Positioning.Flatbed
                },
                new Plotter
                {
                    Model = "hp",
                    Price = 5,
                    Positioning = Positioning.RollToToll
                },
                new Plotter
                {
                    Model = "Yotta Something",
                    Price = 100,
                    DrawingMethod = DrawingMethod.Inkjet
                }
            ]);

            dbContext.SaveChanges();
        }


        private class PlotterDbContext : DbContext
        {
            public PlotterDbContext(string db_path) : base() => dbPath = db_path;


            public DbSet<Plotter> Plotters { get; set; }


            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }


            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Plotter>().Property(p => p.ResolutionX);
                modelBuilder.Entity<Plotter>().Property(p => p.ResolutionY);
            }


            private readonly string dbPath;
        }


        private readonly HttpListener listener;
        private readonly JsonSerializerOptions options;
    }
}
