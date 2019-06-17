using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealReviewWebAPI.Helpers;
using RealReviewWebAPI.Models;

namespace RealReviewWebAPI.Controllers
{
    [Route("api/[controller]/businesses")]
    [ApiController]
    public class YelpMIController : ControllerBase
    {
        [HttpGet]
        [Route("{searchstring}/{location}")]
        public async Task<IEnumerable<Business>> GetBusinesses(string location, string searchstring)
        {
            if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(searchstring))
                return new List<Business>();
            List<Business> lstBusiness = new List<Business>();
            var client = new Yelp.Api.Client("");
            Yelp.Api.Models.SearchRequest searchRequest = new Yelp.Api.Models.SearchRequest();
            searchRequest.Location = location;
            searchRequest.Term = searchstring;
            var results = await client.SearchBusinessesAllAsync(searchRequest);
            Yelp.Api.Models.SearchResponse searchResponse = results;
            var businesses = searchResponse.Businesses;
            foreach (var b in businesses)
            {
                var business = new Business();
                business.name = b.Name.Trim();
                business.address = b.Location.Address1.Trim();
                business.city = b.Location.City.Trim();
                business.zipCode = b.Location.ZipCode.Trim();
                business.state = b.Location.State.Trim();
                business.phone = b.Phone.Trim();
                var reviewresult = await client.GetReviewsAsync(b.Id);
                var revs = reviewresult.Reviews;
                business.reviews = revs.Select(x => Regex.Replace(x.Text.Trim(), @"\r\n?|\n", "")).ToArray();
                lstBusiness.Add(business);
            }
            return lstBusiness;
        }

        [HttpPost]
        [Route("realreviewscore")]
        public RealReviewScoreData GetRealReviewScore([FromBody] Business business)
        {
            RealReviewScoreData realReviewScoreData = new RealReviewScoreData();
            BinaryClassifierPipeline _binaryClassifierPipeline = new BinaryClassifierPipeline();
            float allProbability = 0;
            realReviewScoreData.Accuracy = _binaryClassifierPipeline.Accuracy;
            realReviewScoreData.AreaUnderROCCurve = _binaryClassifierPipeline.AreaUnderROCCurve;
            realReviewScoreData.F1Score = _binaryClassifierPipeline.F1Score;
            realReviewScoreData.Reviews = business.reviews;
            foreach (var reviewTxt in business.reviews)
            {
                allProbability += _binaryClassifierPipeline.GetProbabilityByUsingModelWithSingleItem(reviewTxt) * 100;
            }
            realReviewScoreData.RealReviewScore = Convert.ToInt32(allProbability / 3);
            return realReviewScoreData;
        }

    }
}