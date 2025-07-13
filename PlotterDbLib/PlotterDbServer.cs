using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;



namespace PlotterDbLib
{
    public class PlotterDbServer
    {
        public PlotterDbServer(bool forceRecreation = false)
        {
            using var dbContext = new PlotterDbContext(DbPath);

            if (forceRecreation) dbContext.Database.EnsureDeleted();

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

                    //SetUpDataBase(dbContext);
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


        public string DbPath { init; get; } = "./plotters.db";
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
                        var request = context.Request;
                        Console.WriteLine(
                            $"Received request: {request.HttpMethod} {request.Url}");

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
            
            try
            {
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        await HandleGetAsync(context);
                        break;
                    case "POST":
                        await HandlePostAsync(context);
                        break;
                    case "PUT":
                        await HandlePutAsync(context);
                        break;
                    case "DELETE":
                        await HandleDeleteAsync(context);
                        break;
                    default:
                        context.Response.StatusCode = 405;
                        break;
                }
            }
            catch (JsonException exception)
            {
                Console.WriteLine(exception.Message);
                context.Response.StatusCode = 400;
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine(exception.Message);
                context.Response.StatusCode = 400;
            }
            context.Response.Close();

            Console.WriteLine("Request handled");
        }


        private async Task HandleGetAsync(HttpListenerContext context)
        {
            var filter = GetObjectFromContent<Filter>(context.Request);
            await AddJsonContentAsync(context.Response, GetFiltered(filter));
        }


        // I dont like that next 3 methods are pretty much duplicates, but I dont
        // know how to compbine them
        private async Task HandlePostAsync(HttpListenerContext context)
        {
            var plotter = GetObjectFromContent<Plotter>(context.Request);

            using PlotterDbContext db = new(DbPath);
            db.Add(plotter);
            await db.SaveChangesAsync();
        }


        private async Task HandlePutAsync(HttpListenerContext context)
        {
            var plotter = GetObjectFromContent<Plotter>(context.Request);

            using PlotterDbContext db = new(DbPath);
            db.Update(plotter);
            await db.SaveChangesAsync();
        }


        private async Task HandleDeleteAsync(HttpListenerContext context)
        {
            var plotter = GetObjectFromContent<Plotter>(context.Request);

            using PlotterDbContext db = new(DbPath);
            db.Remove(plotter);
            await db.SaveChangesAsync();
        }


        private T GetObjectFromContent<T>(HttpListenerRequest request)
        {
            byte[] buffer = new byte[request.ContentLength64];
            request.InputStream.Read(buffer, 0, (int)request.ContentLength64);
            return JsonSerializer.Deserialize<T>(buffer, options) ?? 
                throw new JsonException("Deserialized null value");
        }


        private async Task AddJsonContentAsync(HttpListenerResponse response, 
            object content)
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(content, options);

            response.ContentLength64 = buffer.Length;
            response.ContentType = "Application/json";

            using Stream output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();
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
                    Positioning = Positioning.Drum
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


            /*protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                //modelBuilder.Entity<Plotter>().Property(p => p.ResolutionX);
                //modelBuilder.Entity<Plotter>().Property(p => p.ResolutionY);
            }//*/


            private readonly string dbPath;
        }


        private readonly HttpListener listener;
        private readonly JsonSerializerOptions options;
    }
}
