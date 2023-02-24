using Microsoft.EntityFrameworkCore;
using ProductRecommendationRepository.Model;

namespace ProductRecommendationRepository.Context
{
    public class ProductContext : DbContext
    {


        public ProductContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Product>().ToTable("Product");
            builder.Entity<Transaction>().ToTable("Transaction");
            builder.Entity<Category>().ToTable("Category");
        }
    }
}
