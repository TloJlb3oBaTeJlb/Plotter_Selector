namespace PlotterDbLib
{
    /// <summary>
    /// Клиент, позволяющий модифицировать базу данных.
    /// </summary>
    public class PlotterDbAdminClient : PlotterDbClient
    {
        public PlotterDbAdminClient(string serverUrl = "http://localhost:1111/") 
            : base(serverUrl) { }


        public async Task<HttpResponseMessage> AddPlotterAsync(Plotter plotter) =>
            await SendRequestAsync(HttpMethod.Post, plotter);


        public async Task<HttpResponseMessage> UpdatePlotterAsync(Plotter plotter) =>
            await SendRequestAsync(HttpMethod.Put, plotter);


        public async Task<HttpResponseMessage> RemovePlotterAsync(Plotter plotter) =>
            await SendRequestAsync(HttpMethod.Delete, plotter);

    }
}
