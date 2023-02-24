using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendation.Models
{
    public class ShoppingCartViewModel
    {
        public List<ProductInformation> ShoppingCartProducts { get; set; }

        public List<RecommendedProductInformation> RecommendedProducts { get; set; }
    }
}
