using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationModel.Model
{
    public class RecommendedProductInformation : RecommendedProduct
    {
        public string RecommendedProductName { get; set; }

        public double Price { get; set; }

        public string Brand { get; set; }
    }
}
