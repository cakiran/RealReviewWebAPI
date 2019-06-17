using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealReviewWebAPI.Models
{
    public class Business
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string state { get; set; }
        public string zipCode { get; set; }
        public string city { get; set; }
        public string[] reviews { get; set; }
    }
}
