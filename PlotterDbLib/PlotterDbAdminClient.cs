namespace PlotterDbLib
{
    /// <summary>
    /// Клиент, позволяющий модифицировать базу данных.
    /// </summary>
    public class PlotterDbAdminClient : PlotterDbClient
    {
        public PlotterDbAdminClient() : base() { }


        public async Task<HttpResponseMessage> ChangePlotterAsync(Plotter plotter) =>
            await SendRequestAsync(HttpMethod.Put, plotter);


        public async Task<HttpResponseMessage> AddPlotterAsync(Plotter plotter) =>
            await SendRequestAsync(HttpMethod.Post, plotter);


        public async Task<HttpResponseMessage> RemovePlotterAsync(Plotter plotter) =>
            await SendRequestAsync(HttpMethod.Delete, plotter);

    }
}
