using Microsoft.AspNetCore.Mvc;
using ProductRecommendation.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendation.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService productService;

        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Products()
        {
            return View(productService.GetAllCategoriesWithProducts());
        }

        public IActionResult AddShoppingCart(int id)
        {
            productService.AddShoppingCart(id);
            return View("Products", productService.GetAllCategoriesWithProducts());
        }

        public IActionResult DeleteShoppingCart(int id)
        {
            productService.DeleteShoppingCart(id);
            //return View("ShoppingCart", productService.ShoppingCart());
            return RedirectToAction("ShoppingCart");
        }

        public IActionResult ShoppingCart()
        {            
            return View(productService.ShoppingCart());
        }
    }
}
