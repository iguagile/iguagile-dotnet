using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Iguagile.Api
{
    public class RoomApiClient : IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseUrl;

        public RoomApiClient(string baseUrl)
        {
            var uri = new Uri(baseUrl);
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    _baseUrl = uri.AbsoluteUri;
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
            var uri = new Uri(_baseUrl + "/rooms");
            using (var response = await _httpClient.PostAsync(uri, requestContent))
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseSerializer = new DataContractJsonSerializer(typeof(CreateRoomResponse));
                var apiResponse = responseSerializer.ReadObject(responseStream) as CreateRoomResponse;
                if (apiResponse == null || !apiResponse.Success || apiResponse.Room == null)
                {
                    throw new RoomApiException(apiResponse?.Error);
                }

                apiResponse.Room.ApplicationName = request.ApplicationName;
                apiResponse.Room.Version = request.Version;
                apiResponse.Room.Password = request.Password;

                return apiResponse.Room;
            }
        }

        public async Task<Room[]> SearchRoomAsync(SearchRoomRequest request)
        {
            var uriString = _baseUrl + "/rooms?";
            var parameters = new Dictionary<string, string>()
            {
                {"name", request.ApplicationName},
                {"version", request.Version}
            };

            using (var content = new FormUrlEncodedContent(parameters))
            {
                uriString += await content.ReadAsStringAsync();
            }

            var uri = new Uri(uriString);
            using (var response = await _httpClient.GetAsync(uri))
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseSerializer = new DataContractJsonSerializer(typeof(SearchRoomResponse));
                var apiResponse = responseSerializer.ReadObject(responseStream) as SearchRoomResponse;
                if (apiResponse == null || !apiResponse.Success)
                {
                    throw new RoomApiException(apiResponse?.Error);
                }

                if (apiResponse.Rooms == null)
                {
                    return new Room[0];
                }

                foreach (var room in apiResponse.Rooms)
                {
                    room.ApplicationName = request.ApplicationName;
                    room.Version = request.Version;
                }

                return apiResponse.Rooms;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}