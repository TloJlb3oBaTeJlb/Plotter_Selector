using System.Net.Http.Json;
using System.Text.Json;


namespace PlotterDbLib
{

    /// <summary>
    /// Интерфейс работы с базой данных. Получение данных осуществялется через 
    /// <c>PlotterDataBase.GetFilteredPlotters(Filter)</c>.
    /// Конструктор принимает на вход путь к базе данных.
    /// </summary>
    public class PlotterDbClient
    {
        public PlotterDbClient()
        {
            client = new HttpClient { BaseAddress = new Uri(ServerUrl) };

            options = new() { IncludeFields = true };
        }


        public string ServerUrl { init; get; } = "http://localhost:1111/";


        /// <summary>
        /// Метод для получения данных из базы данных.
        /// </summary>
        /// <param name="filter">Параметры фильтрации</param>
        /// <returns>Коллекция плоттеров, удовлетворяющий фильтрам</returns>
        public async Task<List<Plotter>> GetFilteredPlottersAsync(Filter filter)
        {
            List<Plotter> result = [];

            // query
            /*var ur = new FormUrlEncodedContent(filter.ToDictionary());
            var tmp = "path?" + await ur.ReadAsStringAsync();//*/

            // change because this is horrible
            string tmp = JsonSerializer.Serialize(filter, options);
            var res = await client.GetFromJsonAsync(tmp, typeof(List<Plotter>), options);

            if (res != null) result = (List<Plotter>)res;
            return result;
        }


        private readonly HttpClient client;
        private readonly JsonSerializerOptions options;
    }

}
