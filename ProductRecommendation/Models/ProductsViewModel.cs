
using ProductRecommendationRepository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendation.Models
{
    public class ProductsViewModel
    {
        public List<Category> CategoriesWithProducts { get; set; }
    }
}
