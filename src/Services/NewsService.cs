using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Soulfire.Bot.Dtos.Responses;

namespace Soulfire.Bot.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public NewsService(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _httpClient = clientFactory.CreateClient("newsClient");
            _apiKey = configuration["NewsApi:ApiKey"];
        }

        public async Task<ArticlesResponse> GetTopHeadlines()
        {
            var builder = new UriBuilder(_httpClient.BaseAddress + "top-headlines");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["country"] = "ru";
            query["apiKey"] = _apiKey;
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ArticlesResponse>(responseBody);
            return result;
        }
    }
}
