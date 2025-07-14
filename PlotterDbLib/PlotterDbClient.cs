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
        public PlotterDbClient(string serverUrl = "http://localhost:1111/")
        {
            client = new HttpClient { BaseAddress = new Uri(serverUrl) };

            options = new() { IncludeFields = true };
        }


        /// <summary>
        /// Url адрес сервера базы данных
        /// </summary>
        //public string ServerUrl { init; get; } = "http://localhost:1111/";


        /// <summary>
        /// Метод для получения данных из базы данных.
        /// </summary>
        /// <param name="filter">Параметры фильтрации</param>
        /// <returns>Коллекция плоттеров, удовлетворяющий фильтрам</returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<List<Plotter>> GetFilteredPlottersAsync(Filter filter)
        {
            var response = await SendRequestAsync(HttpMethod.Get, filter);
            return await GetObjectFromResponseAsync<List<Plotter>>(response);
        }


        protected async Task<HttpResponseMessage> SendRequestAsync<T>(
            HttpMethod method, T param)
        {
            ByteArrayContent byteContent = new(
                JsonSerializer.SerializeToUtf8Bytes(param, options));
            HttpRequestMessage message = new(method, "")
            {
                Content = byteContent
            };

            var response = await client.SendAsync(message);
            return response.EnsureSuccessStatusCode();
        }


        protected async Task<T> GetObjectFromResponseAsync<T>(HttpResponseMessage response)
        {
            var result = await response.Content.ReadFromJsonAsync<T>(options) ??
                throw new HttpRequestException("Could not get object from content");

            return result;
        }


        private readonly HttpClient client;
        private readonly JsonSerializerOptions options;
    }

}
