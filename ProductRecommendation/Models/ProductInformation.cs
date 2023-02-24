using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendation.Models
{
    public class ProductInformation
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }

        public string Brand { get; set; }
    }
}
