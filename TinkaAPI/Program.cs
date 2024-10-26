using Microsoft.Azure.Cosmos;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuración del cliente de Cosmos DB
        builder.Services.AddSingleton(s =>
        {
            var configuration = s.GetRequiredService<IConfiguration>();
            var cosmosClient = new CosmosClient(
                configuration.GetValue<string>("COSMOS_DB_ACCOUNT", "https://azcosmosdbpazos.documents.azure.com:443/"),
                configuration.GetValue<string>("COSMOS_DB_KEY", "XrIhhO0mwv5EbiA07hXpkjM8a69chqn4Fr1O4ZDjRYghRkrnS7givBvFX1kwqNfp2LGPM85cz5LCACDbgYQy5A=="),
                //builder.Configuration["CosmosDb:Account"],
                //builder.Configuration["CosmosDb:Key"],
                new CosmosClientOptions
                {
                    HttpClientFactory = () =>
                    {
                        var httpClientHandler = new HttpClientHandler();
                        httpClientHandler.ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true;
                        return new HttpClient(httpClientHandler);
                    }
                });

            var database = cosmosClient.GetDatabase(builder.Configuration["CosmosDb:DatabaseName"]);
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