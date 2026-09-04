using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("prices")]
public class PricesController : ControllerBase
{
    private readonly HttpClient _http;

    public PricesController(IHttpClientFactory httpFactory)
    {
        _http = httpFactory.CreateClient();
    }

    [HttpGet("{cryptoCode}")]
    public async Task<IActionResult> ObtenerPrecio(string cryptoCode)
    {
        try
        {
            var url = $"https://criptoya.com/api/satoshitango/{cryptoCode}/ars/1";
            var respuesta = await _http.GetFromJsonAsync<CriptoyaResponse>(url);
            if (respuesta == null) return StatusCode(500, "No se pudo obtener el precio");

            return Ok(new
            {
                CryptoCode = cryptoCode,
                Price = respuesta.totalBid,
                TotalAsk = respuesta.totalAsk
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}