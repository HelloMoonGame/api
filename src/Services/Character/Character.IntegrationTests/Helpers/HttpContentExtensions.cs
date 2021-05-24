using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Character.IntegrationTests.Helpers
{
    public static class HttpContentExtensions
    {
        public static Task<T?> ReadObjectFromJsonAsync<T>(this HttpContent content,
            CancellationToken cancellationToken = default)
        {
            return content.ReadFromJsonAsync<T>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = {new JsonStringEnumConverter()}
            }, cancellationToken);
        }
    }
}
