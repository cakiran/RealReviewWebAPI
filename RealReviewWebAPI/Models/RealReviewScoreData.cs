using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealReviewWebAPI.Models
{
    public class RealReviewScoreData
    {
        public int RealReviewScore { get; set; }
        public string[] Reviews { get; set; }
        public double Accuracy { get; set; }
        public double AreaUnderROCCurve { get; set; }
        public double F1Score { get; set; }
    }
}
