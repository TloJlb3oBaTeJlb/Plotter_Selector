using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlotterDbLib
{
    public class PlotterDbAdminClient : PlotterDbClient
    {
        public PlotterDbAdminClient() : base() { }


        public void ChangePlotter() { }


        public async Task<HttpResponseMessage> AddPlotterAsync()//change
        {
            //StringContent str = new(, Encoding.UTF8);
            //JsonContent content = new JsonContent()
            ByteArrayContent byteContent = new(
                JsonSerializer.SerializeToUtf8Bytes(new Plotter() { Model = "works?" }, options));
            //var content = JsonSerializer.Serialize(new Plotter() { Model = "works?" });
            var res = await client.PostAsync("", byteContent);
            
            return res;
        }


        public void RemovePlotter() { }
    }
}
