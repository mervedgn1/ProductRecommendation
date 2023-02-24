using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationRepository.Model
{
    public class Transaction
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TransactionId { get; set; }

        public int ProductId { get; set; }

        public DateTime TransactionDate { get; set; }

    }
}
