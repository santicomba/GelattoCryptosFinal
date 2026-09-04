using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Criptos_TP_FINAL_PROGRAMACION_3.Data;
using Criptos_TP_FINAL_PROGRAMACION_3.Models;

[ApiController]
[Route("balances")]
[Authorize]
public class BalancesController : ControllerBase
{
    private readonly AppDbContext _db;

    public BalancesController(AppDbContext db)
    {
        _db = db;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET /balances
    [HttpGet]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetUserId();

        var balance = await _db.Balances
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (balance == null)
        {
            balance = new Balance
            {
                UserId = userId,
                Saldo = 0
            };

            _db.Balances.Add(balance);
            await _db.SaveChangesAsync();
        }

        return Ok(new
        {
            saldo = balance.Saldo
        });
    }

    // POST /balances/cargar
    [HttpPost("cargar")]
    public async Task<IActionResult> CargarSaldo([FromBody] CargarSaldoDto dto)
    {
        if (dto.Monto <= 0)
            return BadRequest("El monto debe ser mayor a 0.");

        var userId = GetUserId();

        var balance = await _db.Balances
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (balance == null)
        {
            balance = new Balance
            {
                UserId = userId,
                Saldo = dto.Monto
            };

            _db.Balances.Add(balance);
        }
        else
        {
            balance.Saldo += dto.Monto;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            saldo = balance.Saldo
        });
    }

    public class CargarSaldoDto
    {
        public decimal Monto { get; set; }
    }
}