using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendation.Models
{
    public class RecommendedProduct
    {
        public int RecommendedProductId { get; set; }

        public float RecommendationScore { get; set; }
    }
}
