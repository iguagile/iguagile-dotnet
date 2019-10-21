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
                    if (!this.baseUrl.EndsWith("/"))
                    {
                        this.baseUrl += "/";
                    }
                    break;
                default:
                    throw new ArgumentException($"invalid scheme: {uri.Scheme}");
            }
        }

        public async Task<Room> CreateRoomAsync(CreateRoomRequest request)
        {
            var requestStream = new MemoryStream();
            var requestSerializer = new DataContractJsonSerializer(typeof(CreateRoomRequest));
            requestSerializer.WriteObject(requestStream, request);
            var requestJson = Encoding.UTF8.GetString(requestStream.ToArray());
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var uri = new Uri(baseUrl + "create");
            using (var response = await httpClient.PostAsync(uri, requestContent))
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseSerializer = new DataContractJsonSerializer(typeof(RoomApiResponse));
                var apiResponse = responseSerializer.ReadObject(responseStream) as RoomApiResponse;
                if (apiResponse == null || !apiResponse.Success || apiResponse.Rooms.Length == 0)
                {
                    throw new RoomApiException();
                }
                return apiResponse.Rooms[0];
            }
        }

        public async Task<Room[]> SearchRoomAsync(SearchRoomRequest request)
        {
            var uri = new Uri($"{baseUrl}search?name={request.ApplicationName}&version={request.Version}");
            using (var response = await httpClient.GetAsync(uri))
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseSerializer = new DataContractJsonSerializer(typeof(RoomApiResponse));
                var apiResponse = responseSerializer.ReadObject(responseStream) as RoomApiResponse;
                if (apiResponse == null || !apiResponse.Success || apiResponse.Rooms.Length == 0)
                {
                    throw new RoomApiException();
                }
                return apiResponse.Rooms;
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
