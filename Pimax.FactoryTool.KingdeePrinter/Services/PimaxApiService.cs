using Newtonsoft.Json;
using Pimax.FactoryTool.KingdeePrinter.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pimax.FactoryTool.KingdeePrinter.Services
{
    public static class PimaxApiService
    {
        const string url = "https://ae-openapi.feishu.cn";

        static Token token;

        public static async Task<T> PostAsync<T>(string url, object data)
        {
            using (var client = new HttpClient())
            {
                if (token != null)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token.AccessToken);
                }
                var body = new StringContent(JsonConvert.SerializeObject(data));
                var response = await client.PostAsync(url, body);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                var apiResult = JsonConvert.DeserializeObject<ApiResult<T>>(content, settings);
                if (apiResult.Code != "0")
                {
                    throw new PimaxApiServiceException($"PimaxApiService.PostAsync.Error: ({apiResult.Code}) {apiResult.Message}");
                }
                return apiResult.Data;
            }
        }

        private static async Task GetTokenAsync()
        {
            token = await PostAsync<Token>(url + "/auth/v1/appToken", new
            {
                clientId = AppConfig.PimaxClientId,
                clientSecret = AppConfig.PimaxClientSecret
            });
        }

        public static async Task AddWhiteList(string sn)
        {
            if (token == null || !token.IsValid) await GetTokenAsync();

            await PostAsync<dynamic>(url + "/v1/data/namespaces/oasis_workspace__c/objects/oasis_sn_white_list/records",
                new
                {
                    record = new
                    {
                        sn,
                        equity_category = "2",
                        list_type = "0",
                        status = "0",
                        import_person = "KingdeePrinter.exe"
                    }
                });
        }
    }

    public class ApiResult<T>
    {
        public string Code { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class Token
    {
        public string AccessToken { get; set; }
        public long ExpireTime { get; set; }

        public bool IsValid =>
            ExpireTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > 60;
    }
}

public class PimaxApiServiceException : Exception
{
    public PimaxApiServiceException(string message) : base(message) { }
}
