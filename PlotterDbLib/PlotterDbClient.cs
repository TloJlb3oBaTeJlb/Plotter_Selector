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
            
            ByteArrayContent byteContent = new(
                JsonSerializer.SerializeToUtf8Bytes(filter, options));

            HttpRequestMessage message = new(HttpMethod.Get, "") {
                Content = byteContent
            };

            var response = await client.SendAsync(message);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new HttpRequestException(
                    "Request failed, status code: " + response.StatusCode);

            var res = await response.Content.ReadFromJsonAsync<List<Plotter>>(options);

            if (res != null) return res;
            return [];
        }


        protected readonly HttpClient client;
        protected readonly JsonSerializerOptions options;
    }

}
