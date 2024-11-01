using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

[ApiController]
[Route("api/[controller]")]
public class TinkaController : ControllerBase
{
    private readonly TinkaService _tinkaService;

    public TinkaController(TinkaService tinkaService)
    {
        _tinkaService = tinkaService;
    }

    [HttpPost("sorteo")]
    public async Task<IActionResult> CreateSorteo([FromBody] Sorteo sorteo)
    {
        try
        {
            var response = await _tinkaService.CreateSorteoAsync(sorteo);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear el sorteo: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("sorteos")]
    public async Task<IActionResult> GetSorteos()
    {
        var sorteos = await _tinkaService.GetAllSorteosAsync();
        return Ok(sorteos);
    }

    [HttpDelete("sorteo/{id}")]
    public async Task<IActionResult> DeleteSorteo(string id)
    {
        try
        {
            await _tinkaService.DeleteSorteoAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar el sorteo: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("frecuencia")]
    public async Task<IActionResult> CreateFrecuencia([FromBody] FrecuenciaBolilla frecuencia)
    {
        try
        {
            
            var response = await _tinkaService.CreateFrecuenciaAsync(frecuencia);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear la frecuencia: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("frecuencias")]
    public async Task<IActionResult> GetFrecuencias()
    {
        var frecuencias = await _tinkaService.GetAllFrecuenciasAsync();
        return Ok(frecuencias);
    }

    [HttpDelete("frecuencia/{id}")]
    public async Task<IActionResult> DeleteFrecuencia(int id)
    {
        try
        {
            await _tinkaService.DeleteFrecuenciaAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar la frecuencia: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("prediccion")]
    public IActionResult GetPrediccionFinal()
    {
        var prediccion = _tinkaService.PrediccionFinal();
        return Ok(prediccion);
    }

    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("El archivo está vacío.");
            }

            using var stream = file.OpenReadStream();

            // Configurar la licencia de EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);

            var sorteoWorksheet = package.Workbook.Worksheets["Sorteo"];
            var frecuenciaWorksheet = package.Workbook.Worksheets["FrecuenciaBolilla"];

            // Borrar los registros existentes
            await _tinkaService.DeleteAllSorteosAsync();
            await _tinkaService.DeleteAllFrecuenciasAsync();

            // Cargar los nuevos registros de la pestaña "Sorteo"
            for (int row = 2; row <= sorteoWorksheet.Dimension.End.Row; row++)
            {
                var sorteo = new Sorteo
                {
                    Id = sorteoWorksheet.Cells[row, 1].Text,
                    Fecha = DateTime.Parse(sorteoWorksheet.Cells[row, 2].Text),
                    Bolilla1 = int.Parse(sorteoWorksheet.Cells[row, 3].Text),
                    Bolilla2 = int.Parse(sorteoWorksheet.Cells[row, 4].Text),
                    Bolilla3 = int.Parse(sorteoWorksheet.Cells[row, 5].Text),
                    Bolilla4 = int.Parse(sorteoWorksheet.Cells[row, 6].Text),
                    Bolilla5 = int.Parse(sorteoWorksheet.Cells[row, 7].Text),
                    Bolilla6 = int.Parse(sorteoWorksheet.Cells[row, 8].Text),
                    documentType = "Sorteo",
            };
                await _tinkaService.CreateSorteoAsync(sorteo);
            }

            // Cargar los nuevos registros de la pestaña "FrecuenciaBolilla"
            for (int row = 2; row <= frecuenciaWorksheet.Dimension.End.Row; row++)
            {
                var frecuencia = new FrecuenciaBolilla
                {
                    Bolilla = int.Parse(frecuenciaWorksheet.Cells[row, 1].Text),
                    NumVeces = int.Parse(frecuenciaWorksheet.Cells[row, 2].Text),
                    documentType = "FrecuenciaBolilla",
                };
                await _tinkaService.CreateFrecuenciaAsync(frecuencia);
            }

            return Ok("Archivo importado exitosamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al importar el archivo: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }
}

