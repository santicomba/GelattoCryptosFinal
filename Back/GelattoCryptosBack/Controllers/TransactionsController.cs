using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Criptos_TP_FINAL_PROGRAMACION_3.Models;
using Criptos_TP_FINAL_PROGRAMACION_3.Data;

[ApiController]
[Route("transactions")]
[Authorize]   // <- nadie sin token entra a ningun endpoint de esta clase
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;

    public TransactionsController(AppDbContext db, IHttpClientFactory httpFactory)
    {
        _db = db;
        _http = httpFactory.CreateClient();
    }

    // saca el Id del usuario logueado desde el token, nunca confiamos en lo que manda el cliente
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool EsAdmin() => User.IsInRole("admin");

    // GET /transactions - trae SOLO las del usuario logueado (admin ve todas)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = _db.Transactions.AsQueryable();
        if (!EsAdmin())
            query = query.Where(t => t.UserId == GetUserId());

        var lista = await query.OrderByDescending(t => t.Id).ToListAsync();
        return Ok(lista);
    }

    // GET /transactions/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transaccion = await _db.Transactions.FindAsync(id);
        if (transaccion == null) return NotFound();
        if (!EsAdmin() && transaccion.UserId != GetUserId()) return Forbid();
        return Ok(transaccion);
    }

    // POST /transactions
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transaction t)
    {
        t.UserId = GetUserId();   // se asigna server-side, no lo manda el frontend

        if (t.CryptoAmount <= 0)
            return BadRequest("La cantidad debe ser mayor a 0");

        var precio = await ObtenerPrecio(t.CryptoCode);

        if (precio == null)
            return StatusCode(500, "No se pudo obtener el precio de la criptomoneda");

        var total = t.CryptoAmount * precio.Value;

        // COMPRA: validar saldo y descontarlo
        if (t.Action == "purchase")
        {
            var balance = await _db.Balances
                .FirstOrDefaultAsync(b => b.UserId == t.UserId);

            if (balance == null || balance.Saldo < total)
                return BadRequest("No tenés saldo suficiente para esta compra.");

            balance.Saldo -= total;
        }

        // VENTA: validar cripto disponible y acreditar el dinero
        if (t.Action == "sale")
        {
            var saldoCrypto = await CalcularSaldo(t.CryptoCode, t.UserId);

            if (t.CryptoAmount > saldoCrypto)
                return BadRequest(
                    $"No tenés suficiente {t.CryptoCode}. Saldo actual: {saldoCrypto}"
                );

            var balance = await _db.Balances
                .FirstOrDefaultAsync(b => b.UserId == t.UserId);

            if (balance == null)
            {
                balance = new Balance
                {
                    UserId = t.UserId,
                    Saldo = total
                };

                _db.Balances.Add(balance);
            }
            else
            {
                balance.Saldo += total;
            }
        }


        t.Money = total;

        _db.Transactions.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, t);
    }

    // PATCH /transactions/{id} - solo admin
    [HttpPatch("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Edit(int id, [FromBody] Transaction datos)
    {
        var transaccion = await _db.Transactions.FindAsync(id);
        if (transaccion == null) return NotFound();

        if (datos.CryptoCode != null) transaccion.CryptoCode = datos.CryptoCode;
        if (datos.Action != null) transaccion.Action = datos.Action;
        if (datos.CryptoAmount > 0) transaccion.CryptoAmount = datos.CryptoAmount;
        if (datos.Money > 0) transaccion.Money = datos.Money;
        if (datos.DateTime != default) transaccion.DateTime = datos.DateTime;

        await _db.SaveChangesAsync();
        return Ok(transaccion);
    }

    // DELETE /transactions/{id} - solo admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaccion = await _db.Transactions.FindAsync(id);
        if (transaccion == null) return NotFound();

        _db.Transactions.Remove(transaccion);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /transactions/portfolio - SOLO del usuario logueado (admin ve todo junto, igual que antes)
    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio()
    {
        var query = _db.Transactions.AsQueryable();
        if (!EsAdmin())
            query = query.Where(t => t.UserId == GetUserId());

        var transacciones = await query.ToListAsync();

        var saldos = transacciones
            .GroupBy(t => t.CryptoCode)
            .Select(g => new
            {
                CryptoCode = g.Key,
                Cantidad = g.Sum(t => t.Action == "purchase" ? t.CryptoAmount : -t.CryptoAmount)
            })
            .Where(b => b.Cantidad > 0)
            .ToList();

        var portafolio = new List<object>();
        decimal total = 0;

        foreach (var item in saldos)
        {
            var precio = await ObtenerPrecio(item.CryptoCode);
            if (precio != null)
            {
                var valorARS = item.Cantidad * precio.Value;
                total += valorARS;
                portafolio.Add(new { item.CryptoCode, item.Cantidad, ValorARS = valorARS });
            }
        }

        return Ok(new { Tenencias = portafolio, TotalEnARS = total });
    }

    private async Task<decimal> CalcularSaldo(string cryptoCode, string userId)
    {
        var transacciones = await _db.Transactions
            .Where(t => t.CryptoCode == cryptoCode && t.UserId == userId)
            .ToListAsync();

        decimal comprado = transacciones.Where(t => t.Action == "purchase").Sum(t => t.CryptoAmount);
        decimal vendido = transacciones.Where(t => t.Action == "sale").Sum(t => t.CryptoAmount);
        return comprado - vendido;
    }

    private async Task<decimal?> ObtenerPrecio(string cryptoCode)
    {
        try
        {
            var url = $"https://criptoya.com/api/binance/{cryptoCode}/ars/1";
            var respuesta = await _http.GetFromJsonAsync<CriptoyaResponse>(url);
            return respuesta?.totalBid;
        }
        catch
        {
            return null;
        }
    }
}

public class CriptoyaResponse
{
    public decimal totalBid { get; set; }
    public decimal totalAsk { get; set; }
}