using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoxWebApi.Models
{
    class VehicleResponse
    {
        public int vehicleid { get; set; }
        public int year { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public int dealerId { get; set; }
    }
}
