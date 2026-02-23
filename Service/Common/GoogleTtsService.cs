using Microsoft.AspNetCore.WebUtilities;

namespace XeniaTokenBackend.Service.Common
{
    public static class GoogleTtsService
    {
        public static async Task<byte[]> GetMp3Async(
            IHttpClientFactory httpClientFactory,
            string text
        )
        {
            const string baseUrl = "https://translate.google.com/translate_tts";

            var query = new Dictionary<string, string>
            {
                ["ie"] = "UTF-8",
                ["client"] = "tw-ob",
                ["tl"] = "en-IN",
                ["q"] = text
            };

            var url = QueryHelpers.AddQueryString(baseUrl, query);

            var client = httpClientFactory.CreateClient();

            // 🔑 IMPORTANT: Google blocks requests without User-Agent
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
            );

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
