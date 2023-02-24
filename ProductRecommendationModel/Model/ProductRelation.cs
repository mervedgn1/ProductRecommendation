using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationModel.Model
{
    public class ProductRelation
    {
        public int RelatedProductId { get; set; }

        public int ProductId { get; set; }
    }
}
