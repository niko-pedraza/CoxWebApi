using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoxWebApi.Models
{
    class DealerAnswer
    {
        public int dealerId { get; set; }
        public string name { get; set; }
        public VehicleAnswer[] vehicles { get; set; }

    }
}
