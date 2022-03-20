using CoxWebApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoxWebApi
{
    class CoxWebApiService
    {
        // API Endpoints
        private readonly string CreateNewDataset = "https://api.coxauto-interview.com/api/datasetId";
        private readonly string SubmitAnswer = "https://api.coxauto-interview.com/api/{0}/answer";
        private readonly string DealerInfo = "https://api.coxauto-interview.com/api/{0}/dealers/{1}";
        private readonly string VehicleList = "https://api.coxauto-interview.com/api/{0}/vehicles";
        private readonly string VehicleInfo = "https://api.coxauto-interview.com/api/{0}/vehicles/{1}";
        
        // private variables
        private readonly HttpClient _client;
        private ConcurrentDictionary<int,List<VehicleAnswer>> _dealerCorrespondedVehicles = new ConcurrentDictionary<int, List<VehicleAnswer>>();
        private AnswerResponse _answerResponse;
        private DatasetIdResponse _datasetIdResponse;
        private DealersResponse _dealersResponse;
        private VehicleResponse _vehicleResponse;
        private VehicleIdsResponse _vehicleIdsResponse;




        public CoxWebApiService()
        {
            _client = new HttpClient(); // Create http client to handle Get or Post Request

        }

        public void RunProgram()
        {   
            // Retrieve the Dataset Id
            _datasetIdResponse = GetResponse<DatasetIdResponse>(CreateNewDataset).Result;
            
            // Retrieve the Vehicle list
            _vehicleIdsResponse = GetResponse<VehicleIdsResponse>(string.Format(VehicleList, _datasetIdResponse.datasetId)).Result;
            



            Parallel.ForEach(_vehicleIdsResponse.vehicleIds, (number) =>
            {
                var vehicleResponse = GetResponse<VehicleResponse>(string.Format(VehicleInfo, datasetIdResponse.datasetId, number)).Result;
                var vehicleAnswer = new VehicleAnswer();
                vehicleAnswer.make = vehicleResponse.make;
                vehicleAnswer.model = vehicleResponse.model;
                vehicleAnswer.year = vehicleResponse.year;
                vehicleAnswer.vehicleId = vehicleResponse.vehicleid;

                if (!idWithVehicleList.ContainsKey(vehicleResponse.dealerId))
                {
                    idWithVehicleList.TryAdd(vehicleResponse.dealerId, new List<VehicleAnswer>());

                }
                idWithVehicleList[vehicleResponse.dealerId].Add(vehicleAnswer);
     
            });


            var dealers = new List<DealerAnswer>();
            Parallel.ForEach(idWithVehicleList.Keys.ToArray(), (num) =>
            {
                var dealerResponse = GetResponse<DealersResponse>(string.Format(DealerInfo, _datasetIdResponse.datasetId, num)).Result;
                Print(dealerResponse);

                var dealerAnswer = new DealerAnswer();
                dealerAnswer.dealerId = dealerResponse.dealerId;
                dealerAnswer.name = dealerResponse.name;
                dealerAnswer.vehicles = idWithVehicleList[num].ToArray();
                Print(dealerAnswer);

                dealers.Add(dealerAnswer);

            });

            var answer = new Answer();
            answer.dealers = dealers.ToArray();
            Print(answer);


            // send answer 
            var answerResponse = PostAnswer<AnswerResponse,Answer>(string.Format(SubmitAnswer, _datasetIdResponse.datasetId), answer).Result;
            Print(answerResponse);
        }

        public void AddDealer(int vehicleid) {

            var vehicleResponse = GetResponse<VehicleResponse>(string.Format(VehicleInfo, _datasetIdResponse.datasetId, number)).Result;
            var vehicleAnswer = new VehicleAnswer();
            vehicleAnswer.make = vehicleResponse.make;
            vehicleAnswer.model = vehicleResponse.model;
            vehicleAnswer.year = vehicleResponse.year;
            vehicleAnswer.vehicleId = vehicleResponse.vehicleid;

            if (!idWithVehicleList.ContainsKey(vehicleResponse.dealerId))
            {
                idWithVehicleList.TryAdd(vehicleResponse.dealerId, new List<VehicleAnswer>());

            }
            idWithVehicleList[vehicleResponse.dealerId].Add(vehicleAnswer);

        }



        public void Print(object x)
        {
            Console.WriteLine(JsonConvert.SerializeObject(x));
        }

        public async Task<T> GetResponse<T>(string url)
        {

            var response = await _client.GetAsync(url);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonResponse);
        }

        /// <summary>
        /// Convert object to Json string , send POST request to endpoint "url"
        /// </summary>
        /// <typeparam name="T">Generic Response object</typeparam>
        /// <param name="url">string Endpoint</param>
        /// <param name="ans">Answer object</param>
        /// <returns>Generic Response object</returns>
        public async Task<T> PostAnswer<T,U>(string url, U ans)
        {
            var serialize = JsonConvert.SerializeObject(ans);
            var content = new StringContent(serialize, UnicodeEncoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonResponse);
        }
    }
}
