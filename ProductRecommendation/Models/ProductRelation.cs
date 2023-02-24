using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendation.Models
{
    public class ProductRelation
    {
        public int RelatedProductId { get; set; }

        public int ProductId { get; set; }
    }
}
