using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Criptos_TP_FINAL_PROGRAMACION_3.Models;
using Criptos_TP_FINAL_PROGRAMACION_3.Data;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;

    public TransactionsController(AppDbContext db, IHttpClientFactory httpFactory)
    {
        _db = db;
        _http = httpFactory.CreateClient();
    }

    // GET /transactions - trae todas las transacciones ordenadas por fecha
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _db.Transactions.OrderByDescending(t => t.Id).ToListAsync();
        return Ok(lista);
    }

    // GET /transactions/{id} - trae una transaccion por id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transaccion = await _db.Transactions.FindAsync(id);
        if (transaccion == null) return NotFound();
        return Ok(transaccion);
    }

    // POST /transactions - crea una compra o venta
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transaction t)
    {
        if (t.CryptoAmount <= 0)
            return BadRequest("La cantidad debe ser mayor a 0");

        if (t.Action == "sale")
        {
            var saldo = await CalcularSaldo(t.CryptoCode);
            if (t.CryptoAmount > saldo)
                return BadRequest($"No tenés suficiente {t.CryptoCode}. Saldo actual: {saldo}");
        }

        var precio = await ObtenerPrecio(t.CryptoCode);
        if (precio == null)
            return StatusCode(500, "No se pudo obtener el precio de la criptomoneda");

        t.Money = t.CryptoAmount * precio.Value;

        _db.Transactions.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, t);
    }

    // PATCH /transactions/{id} - edita una transaccion
    [HttpPatch("{id}")]
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

    // DELETE /transactions/{id} - borra una transaccion
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaccion = await _db.Transactions.FindAsync(id);
        if (transaccion == null) return NotFound();

        _db.Transactions.Remove(transaccion);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /transactions/portfolio - estado actual de la cartera
    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio()
    {
        var transacciones = await _db.Transactions.ToListAsync();

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
                portafolio.Add(new
                {
                    item.CryptoCode,
                    item.Cantidad,
                    ValorARS = valorARS
                });
            }
        }

        return Ok(new { Tenencias = portafolio, TotalEnARS = total });
    }

    // Calcula cuanta cripto tiene el usuario
    private async Task<decimal> CalcularSaldo(string cryptoCode)
    {
        var transacciones = await _db.Transactions
            .Where(t => t.CryptoCode == cryptoCode)
            .ToListAsync();

        decimal comprado = transacciones.Where(t => t.Action == "purchase").Sum(t => t.CryptoAmount);
        decimal vendido = transacciones.Where(t => t.Action == "sale").Sum(t => t.CryptoAmount);
        return comprado - vendido;
    }

    // Consulta el precio actual de una cripto en criptoya
    private async Task<decimal?> ObtenerPrecio(string cryptoCode)
    {
        try
        {
            var url = $"https://criptoya.com/api/satoshitango/{cryptoCode}/ars/1";
            var respuesta = await _http.GetFromJsonAsync<CriptoyaResponse>(url);
            return respuesta?.totalBid;
        }
        catch
        {
            return null;
        }
    }
}

// Clase para parsear la respuesta de criptoya
public class CriptoyaResponse
{
    public decimal totalBid { get; set; }
    public decimal totalAsk { get; set; }
}