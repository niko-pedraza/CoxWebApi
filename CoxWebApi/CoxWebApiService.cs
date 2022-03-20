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
        private List<DealerAnswer> _dealers = new List<DealerAnswer>();

        //Response Variables
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
            
            // Pair Dealers to vehicles, find dealer info, format data all together
            Parallel.ForEach(_vehicleIdsResponse.vehicleIds, (number) =>
            {
                PairDealerVehicleList(number);
            });
            Parallel.ForEach(_dealerCorrespondedVehicles.Keys.ToArray(), (num) =>
            {
                DealersList(num);
            });
            var answer = new Answer();
            answer.dealers = _dealers.ToArray();
        
            // Post Answer
           _answerResponse = PostAnswer<AnswerResponse,Answer>(string.Format(SubmitAnswer, _datasetIdResponse.datasetId), answer).Result;
            
            Print(_answerResponse);
        }

        /// <summary>
        /// Populates the global variable "_dealerCorrespondedVehicles" 
        /// </summary>
        /// <param name="vehicleid">Int32</param>
        public void PairDealerVehicleList(int vehicleid) {
            
            _vehicleResponse = GetResponse<VehicleResponse>(string.Format(VehicleInfo, _datasetIdResponse.datasetId, vehicleid)).Result;
            
            var vehicleAnswer = new VehicleAnswer();
            vehicleAnswer.make = _vehicleResponse.make;
            vehicleAnswer.model = _vehicleResponse.model;
            vehicleAnswer.year = _vehicleResponse.year;
            vehicleAnswer.vehicleId = _vehicleResponse.vehicleid;

            if (!_dealerCorrespondedVehicles.ContainsKey(_vehicleResponse.dealerId))
            {
                _dealerCorrespondedVehicles.TryAdd(_vehicleResponse.dealerId, new List<VehicleAnswer>());

            }
            _dealerCorrespondedVehicles[_vehicleResponse.dealerId].Add(vehicleAnswer);

        }

        /// <summary>
        /// Populates the global variable "_dealers"
        /// </summary>
        /// <param name="dealerId">Int32</param>
        public void DealersList(int dealerId) 
        {
            _dealersResponse = GetResponse<DealersResponse>(string.Format(DealerInfo, _datasetIdResponse.datasetId, dealerId)).Result;
           
            var dealerAnswer = new DealerAnswer();
            dealerAnswer.dealerId = _dealersResponse.dealerId;
            dealerAnswer.name = _dealersResponse.name;
            dealerAnswer.vehicles = _dealerCorrespondedVehicles[dealerId].ToArray();

            _dealers.Add(dealerAnswer);

        }

        /// <summary>
        /// send GET request to endpoint "url" in JSON format
        /// </summary>
        /// <typeparam name="T">Generic Type Object</typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<T> GetResponse<T>(string url)
        {

            var response = await _client.GetAsync(url);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonResponse);
        }

        /// <summary>
        /// Format Content from object , send POST request to endpoint "url" in JSON format
        /// </summary>
        /// <typeparam name="T">Generic Type Object</typeparam>
        /// <param name="url">string Endpoint</param>
        /// <param name="ans">Generic object</param>
        /// <returns>Generic Response object</returns>
        public async Task<T> PostAnswer<T,U>(string url, U ans)
        {
            var serialize = JsonConvert.SerializeObject(ans);
            var content = new StringContent(serialize, UnicodeEncoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonResponse);
        }


        public void Print(object x)
        {
            Console.WriteLine(JsonConvert.SerializeObject(x));
        }
    }
}
