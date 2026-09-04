namespace Criptos_TP_FINAL_PROGRAMACION_3.Data
{
    using Microsoft.EntityFrameworkCore;
    using Criptos_TP_FINAL_PROGRAMACION_3.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
