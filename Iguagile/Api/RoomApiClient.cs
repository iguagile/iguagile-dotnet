using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Iguagile.Api
{
    public class RoomApiClient : IDisposable
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string baseUrl;

        public RoomApiClient(string baseUrl)
        {
            var uri = new Uri(baseUrl);
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    this.baseUrl = uri.AbsoluteUri;
                    break;
                default:
                    throw new ArgumentException($"invalid scheme: {uri.Scheme}");
            }
        }

        public async Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest request)
        {
            var requestStream = new MemoryStream();
            var requestSerializer = new DataContractJsonSerializer(typeof(CreateRoomRequest));
            requestSerializer.WriteObject(requestStream, request);
            var requestJson = Encoding.UTF8.GetString(requestStream.ToArray());
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(new Uri(baseUrl + "create"), requestContent);
            var responseStream = await response.Content.ReadAsStreamAsync();
            var responseSerializer = new DataContractJsonSerializer(typeof(CreateRoomResponse));
            return responseSerializer.ReadObject(responseStream) as CreateRoomResponse;
        }

        public async Task<SearchRoomResponse[]> SearchRoomAsync(SearchRoomRequest request)
        {
            var uri = new Uri($"{baseUrl}search?name={request.ApplicationName}&version={request.Version}");
            var response = await httpClient.GetAsync(uri);
            var responseStream = await response.Content.ReadAsStreamAsync();
            var responseSerializer = new DataContractJsonSerializer(typeof(SearchRoomResponse));
            return responseSerializer.ReadObject(responseStream) as SearchRoomResponse[];
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
