using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoxWebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var webApi = new CoxWebApiService();
            webApi.RunProgram();
        }


    }
}
