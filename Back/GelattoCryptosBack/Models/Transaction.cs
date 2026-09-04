using System.ComponentModel.DataAnnotations.Schema;

namespace Criptos_TP_FINAL_PROGRAMACION_3.Models
{   public class Transaction
    {
        public int Id { get; set; }
        public string CryptoCode { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,8)")]
        public decimal CryptoAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Money { get; set; }
        public DateTime DateTime { get; set; }
    }
}
