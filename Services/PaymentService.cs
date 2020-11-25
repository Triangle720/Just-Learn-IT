using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JustLearnIT.Services
{
    public static class PaymentService
    {
        private static string[] Secrets { get; set; }
        private static string Token { get; set; }
        private static Uri BaseUrl { get; set; }

        public static void CreateService(string secrest)
        {
            Secrets = secrest.Split(';');
            Token = Secrets[2];
            BaseUrl = new Uri("https://secure.snd.payu.com/");
        }

        public static async Task GetAccessToken()
        {
            using var httpClient = new HttpClient { BaseAddress = BaseUrl };
            using var content = new StringContent("grant_type=client_credentials&client_id=" + Secrets[0] +"&client_secret=" + Secrets[1], System.Text.Encoding.Default, "application/x-www-form-urlencoded");
            using var response = await httpClient.PostAsync("pl/standard/user/oauth/authorize", content);

            string responseData = await response.Content.ReadAsStringAsync();

            dynamic data = JObject.Parse(responseData);
            Token = data.access_token;
        }

        public static async Task<string> CreateOrder(string amount)
        {
            using var httpClient = new HttpClient { BaseAddress = BaseUrl };
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Token);

            using var content = new StringContent("{ \"continueUrl\": \"https://justlearnit20201113041152.azurewebsites.net/Payment/Result/\"," +
                                                     "\"customerIp\": \"127.0.0.1\"," +
                                                     "\"merchantPosId\": \""+ Secrets[0] + "\"," +
                                                     "\"description\": \"Just Lear IT subscription\"," +
                                                     "\"currencyCode\": \"PLN\"," +
                                                     "\"totalAmount\": \"" + amount + "00\"," +
                                                     "\"products\": [    { \"name\": \"JL_IT Subscription\"," +
                                                                         " \"unitPrice\": \"" + amount +"00\"," +
                                                                         " \"quantity\": \"1\" } ] }", System.Text.Encoding.Default, "application/json");

            using var response = await httpClient.PostAsync("api/v2_1/orders/", content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var uri = response.RequestMessage.RequestUri.AbsoluteUri;
                return string.Join(';', uri, uri.Substring(uri.IndexOf('=') + 1, uri.IndexOf('&') - 1 - uri.IndexOf('=')));
            }

            return string.Empty;
        }

        public static async Task<bool> IsOrderAccepted(string orderId)
        {
            using var httpClient = new HttpClient { BaseAddress = BaseUrl };
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Token);
            using var response = await httpClient.GetAsync("api/v2_1/orders/" + orderId);

            if (response.IsSuccessStatusCode) return true;

            return false;
        }
    }
}
