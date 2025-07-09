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
            var dbContext = new PlotterDbContext(DbPath);

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
        public bool IsRunning { get; private set; } = false;


        public async Task RunAsync()
        {
            listener.Start();
            Console.WriteLine("Server started with url - " + Url);
            IsRunning = true;

            while (true)
            {
                var context = await listener.GetContextAsync();
                Console.WriteLine("Request recieved");

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
                    await SendResponse(response, serialisedList);
                }
                catch (ArgumentNullException exception)
                {
                    Console.WriteLine(exception.Message);
                    await SendResponse(response, "BadRequst", 400);
                }
                catch (JsonException exception)
                {
                    Console.WriteLine(exception.Message);
                    await SendResponse(response, "BadRequst", 400);
                }

                Console.WriteLine("Request handled");
            }

            // TODO make server stoppable
            /*listener.Stop();
            listener.Close();

            Console.WriteLine("Server stopped");*/

        }


        private async Task SendResponse(HttpListenerResponse response, 
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


        private readonly HttpListener listener;
        private readonly JsonSerializerOptions options;
    }
}
