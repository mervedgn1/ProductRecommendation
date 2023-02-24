
using ProductRecommendationModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendation.Service
{
    public interface IProductService
    {
        void BuildDatasetAndTrainModel();

        ProductsViewModel GetAllCategoriesWithProducts();

        void AddShoppingCart(int productId);

        ShoppingCartViewModel ShoppingCart();

        void DeleteShoppingCart(int productId);
    }
}
