using System.IO;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using OfficeOpenXml;
using Xunit;

namespace TinkaApiTests
{
    public class TinkaControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TinkaControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateSorteo_ReturnsSuccess()
        {
            var sorteo = new
            {
                id = "1200",
                fecha = "2024-06-23",
                bolilla1 = 30,
                bolilla2 = 41,
                bolilla3 = 11,
                bolilla4 = 5,
                bolilla5 = 36,
                bolilla6 = 35
            };

            var response = await _client.PostAsJsonAsync("/api/tinka/sorteo", sorteo);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateFrecuencia_ReturnsSuccess()
        {
            var frecuencia = new
            {
                bolilla = 51,
                numVeces = 15
            };

            var response = await _client.PostAsJsonAsync("/api/tinka/frecuencia", frecuencia);
            response.EnsureSuccessStatusCode();
        }


        [Fact]
        public async Task GetPrediccion_ReturnsArray()
        {
            var response = await _client.GetAsync("/api/tinka/prediccion");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("[", content);
            Assert.Contains("]", content);
        }

        [Fact]
        public async Task ImportExcel_ReturnsSuccess()
        {
            // Crear un archivo Excel en memoria para la prueba
            using var package = new ExcelPackage();
            var sorteoWorksheet = package.Workbook.Worksheets.Add("Sorteo");
            sorteoWorksheet.Cells[1, 1].Value = "id";
            sorteoWorksheet.Cells[1, 2].Value = "fecha";
            sorteoWorksheet.Cells[1, 3].Value = "bolilla1";
            sorteoWorksheet.Cells[1, 4].Value = "bolilla2";
            sorteoWorksheet.Cells[1, 5].Value = "bolilla3";
            sorteoWorksheet.Cells[1, 6].Value = "bolilla4";
            sorteoWorksheet.Cells[1, 7].Value = "bolilla5";
            sorteoWorksheet.Cells[1, 8].Value = "bolilla6";

            sorteoWorksheet.Cells[2, 1].Value = Guid.NewGuid().ToString();
            sorteoWorksheet.Cells[2, 2].Value = "2024-06-16";
            sorteoWorksheet.Cells[2, 3].Value = 8;
            sorteoWorksheet.Cells[2, 4].Value = 46;
            sorteoWorksheet.Cells[2, 5].Value = 19;
            sorteoWorksheet.Cells[2, 6].Value = 2;
            sorteoWorksheet.Cells[2, 7].Value = 23;
            sorteoWorksheet.Cells[2, 8].Value = 21;

            var frecuenciaWorksheet = package.Workbook.Worksheets.Add("FrecuenciaBolilla");
            frecuenciaWorksheet.Cells[1, 1].Value = "bolilla";
            frecuenciaWorksheet.Cells[1, 2].Value = "numVeces";

            frecuenciaWorksheet.Cells[2, 1].Value = 10;
            frecuenciaWorksheet.Cells[2, 2].Value = 15;

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", "ResultadosTinka.xlsx");

            var response = await _client.PostAsync("/api/tinka/import-excel", content);
            response.EnsureSuccessStatusCode();
        }

    }
}

