using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationRepository.Model
{
    public class Product
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        public double Price { get; set; }

        public string ProductName{ get; set; }

        public string Brand { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
