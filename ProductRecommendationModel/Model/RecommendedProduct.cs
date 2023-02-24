using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationModel.Model
{
    public class RecommendedProduct
    {
        public int RelatedProductId { get; set; }
                
        public float Score { get; set; }
    }
}
