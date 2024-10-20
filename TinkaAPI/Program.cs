using Microsoft.Azure.Cosmos;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuración del cliente de Cosmos DB
        builder.Services.AddSingleton(s =>
        {
            var cosmosClient = new CosmosClient(
                builder.Configuration["CosmosDb:Account"],
                builder.Configuration["CosmosDb:Key"],
                new CosmosClientOptions
                {
                    HttpClientFactory = () =>
                    {
                        var httpClientHandler = new HttpClientHandler();
                        httpClientHandler.ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true;
                        return new HttpClient(httpClientHandler);
                    }
                });

            var database = cosmosClient.GetDatabase("AzCosmosDBPazos");
            var sorteoContainer = database.GetContainer("TinkaPrediccion");
            var frecuenciaContainer = database.GetContainer("FrecuenciaBolilla");

            return new TinkaService(sorteoContainer, frecuenciaContainer);
        });

        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();
        app.Run();
    }
}