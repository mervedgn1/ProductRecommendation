using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProductRecommendationRepository.Context;
using ProductRecommendation.Service;
using ProductRecommendationModel.Model;
using Microsoft.ML.Data;

namespace ProductRecommendationService.Service
{
    public class ProductService : IProductService
    {
        private readonly ProductContext productContext;

        private readonly IHttpContextAccessor httpContextAccessor;

        public ProductService(ProductContext productContext, IHttpContextAccessor httpContextAccessor)
        {
            this.productContext = productContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        public void BuildDatasetAndTrainModel()
        {
            //var orderIds = productContext.Transactions.GroupBy(x => x.TransactionId).Select(x => x.FirstOrDefault()).ToList();

            //orderIds.ForEach(orderId =>
            //{

            //})

            var transactions = productContext.Transactions.AsEnumerable();

            var productRelations = (from transaction1 in transactions
                                    from transaction2 in transactions
                                    where transaction1.TransactionId == transaction2.TransactionId
                                    select new ProductRelation
                                    {
                                        ProductId = transaction1.ProductId,
                                        RelatedProductId = transaction2.ProductId
                                    }).Where(x => x.ProductId != x.RelatedProductId)
                                   .GroupBy(x => new { x.ProductId, x.RelatedProductId })
                                   .Select(x => x.FirstOrDefault())
                                   .OrderBy(x => x.ProductId)
                                   .ThenBy(x => x.RelatedProductId);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("ProductID\tRelatedProuductID");
            foreach (var p in productRelations)
            {
                builder.AppendLine($"{p.ProductId}\t{p.RelatedProductId}");
            }
            System.IO.File.WriteAllText(GetPath("dataset.txt"), builder.ToString());

            var mlContext = new MLContext();

            //var data = mlContext.Data.LoadFromEnumerable<ProductRelation>(productRelations);

            var data = mlContext.Data.LoadFromTextFile(
                  path: GetPath($"dataset.txt"),
                  columns: new[]
                  {
                    new TextLoader.Column(
                    name:     "Label",
                    dataKind: DataKind.Single,
                    index:    0),

                    new TextLoader.Column(
                    name:     "ProductId",
                    dataKind: DataKind.Int32,
                    source:   new [] { new TextLoader.Range(0) }),

                    new TextLoader.Column(
                    name:     "RelatedProductId",
                    dataKind: DataKind.Int32,
                    source:   new [] { new TextLoader.Range(1) })
                    },
                    hasHeader: true,
                    separatorChar: '\t');

            var partitions = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "ProductIdEncoded",
                MatrixRowIndexColumnName = "RelatedProductIdEncoded",
                LabelColumnName = "Label",
                LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass,
                Alpha = 0.01,
                Lambda = 0.025,
                C = 0.00001
            };

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(
                inputColumnName: "ProductId",
                outputColumnName: "ProductIdEncoded"
                ).Append(mlContext.Transforms.Conversion.MapValueToKey(
                    inputColumnName: "RelatedProductId",
                    outputColumnName: "RelatedProductIdEncoded"
                    )).Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));


            var model = pipeline.Fit(partitions.TrainSet);

            mlContext.Model.Save(model,
                  inputSchema: data.Schema,
                  filePath: GetPath("model.zip"));

            var predictions = model.Transform(partitions.TestSet);
            var metrics = mlContext.Regression.Evaluate(predictions);
            Console.WriteLine("Evaluating model...");
            Console.WriteLine($"RMSE (Root Mean Square Error): {metrics.RootMeanSquaredError}");
            Console.WriteLine($"L1 (Mean Absolute Prediction Error): {metrics.MeanAbsoluteError}");
            Console.WriteLine($"L2 (Mean Square Error): {metrics.MeanSquaredError}");
            Console.WriteLine();
        }

        public ProductsViewModel GetAllCategoriesWithProducts()
        {
            var categories = productContext.Categories.Include(x => x.Products).ToList();

            return new ProductsViewModel { CategoriesWithProducts = categories };
        }

        public void AddShoppingCart(int productId)
        {
            var productsCookie = httpContextAccessor.HttpContext.Request.Cookies["products_cookie"];

            if(productsCookie == default)
            {
                productsCookie = productId.ToString();
                httpContextAccessor.HttpContext.Response.Cookies.Append("products_cookie", productsCookie);

                return;
            }

            var productIds = productsCookie.Split("|");

            if (!productIds.Contains(productId.ToString()))
            {
                productsCookie = string.Join('|', productsCookie, productId);
            }

            httpContextAccessor.HttpContext.Response.Cookies.Append("products_cookie", productsCookie);

            return;

        }

        public void DeleteShoppingCart(int productId)
        {
            var productsCookie = httpContextAccessor.HttpContext.Request.Cookies["products_cookie"];

            if(productsCookie == default)
            {
                return;
            }

            var productIds = productsCookie.Split("|").ToList();

            if (productIds.Contains(productId.ToString()))
            {
                productIds.RemoveAt(productIds.IndexOf(productId.ToString()));
                productsCookie = string.Join("|", productIds);
            }

            httpContextAccessor.HttpContext.Response.Cookies.Append("products_cookie", productsCookie);

            return;
        }

        public ShoppingCartViewModel ShoppingCart()
        {
            var model = new ShoppingCartViewModel
            {
                RecommendedProducts = new List<RecommendedProductInformation>(),
                ShoppingCartProducts = new List<ProductInformation>()
            };

            var productsCookie = httpContextAccessor.HttpContext.Request.Cookies["products_cookie"];

            if (string.IsNullOrWhiteSpace(productsCookie))
            {
                return model;
            }

            var productIds = new List<string>();
            productIds = productsCookie.Split('|').ToList();            

            if (productIds.Any())
            {
                var ids = productIds.Where(x => x != string.Empty).Select(x => int.Parse(x)).ToList();
                var shoppingCartProducts = productContext.Products.Where(x => ids.Contains(x.ProductId)).ToList();
                model.ShoppingCartProducts.AddRange(shoppingCartProducts.Select(x => new ProductInformation
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Brand = x.Brand,
                    Price = x.Price
                }));
            }

            if (System.IO.File.Exists(GetPath("dataset.txt")))
            {
                var mlContext = new MLContext();

                ITransformer recommendationModel;

                using (var stream = new FileStream(
                  path: GetPath("model.zip"),
                  mode: FileMode.Open,
                  access: FileAccess.Read,
                  share: FileShare.Read))
                {
                    recommendationModel = mlContext.Model.Load(stream, out DataViewSchema schema);
                }

                var predictionEngine = mlContext.Model.CreatePredictionEngine<ProductRelation, RecommendedProduct>(recommendationModel);

               foreach(var cartProduct in model.ShoppingCartProducts)
                {
                    
                    var topThree = productContext.Products
                      .Select(product =>
                        predictionEngine.Predict(
                          new ProductRelation
                          {
                              ProductId = cartProduct.ProductId,
                              RelatedProductId = product.ProductId
                          })
                        )
                      .ToList()
                      .OrderByDescending(x => x.Score)
                      .Take(3);

                    var cartProductTransactionIds = productContext.Transactions.Where(x => x.ProductId == cartProduct.ProductId).Select(x => x.TransactionId).ToList();
                    var topThreeIds = topThree.Select(x => x.RelatedProductId).ToList();
                    
                    //Doğrulama başlangıç

                    Console.WriteLine($"{cartProduct.ProductId} id'li urunun bulundugu transactionlar: {string.Join(",", cartProductTransactionIds)}");
                    foreach(var topThreeItem in topThree)
                    {
                        var y = productContext.Transactions.Where(x => cartProductTransactionIds.Contains(x.TransactionId) && x.ProductId == topThreeItem.RelatedProductId).Select(x => x.TransactionId).ToList();
                        Console.WriteLine($"{cartProduct.ProductId} id'li urunun {topThreeItem.RelatedProductId} id'li urun ile birlikte bulundugu transactionlar: {string.Join(",", y)}");
                        Console.WriteLine($"{cartProduct.ProductId} - {topThreeItem.RelatedProductId} skoru: {topThreeItem.Score}");
                    }
                    Console.WriteLine("--------------------------");
                    //Doğrulama bitiş

                    model.RecommendedProducts.AddRange(topThree.Select(product => new RecommendedProductInformation
                    {
                        RelatedProductId = product.RelatedProductId,
                        RecommendedProductName = productContext.Products.Find(Convert.ToInt32(product.RelatedProductId)).ProductName,
                        Brand = productContext.Products.Find(Convert.ToInt32(product.RelatedProductId)).Brand,
                        Price = productContext.Products.Find(Convert.ToInt32(product.RelatedProductId)).Price,
                        Score = product.Score
                    }).Where(x => x.RelatedProductId != cartProduct.ProductId).ToList());
                }

                model.RecommendedProducts = model.RecommendedProducts
                   .OrderByDescending(rec => rec.Score)
                   .Take(3)
                   .ToList();
            }

            return model;
        }

        public string GetPath(string file)
        {
            return Path.Combine("C:\\Users\\Merve Doğan\\source\\repos\\ProductRecommendation\\ProductRecommendation", "wwwroot", "MLDatasetAndModel", file);
        }
        
    }
}
