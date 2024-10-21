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
                configuration["CosmosDb:Account"],
                configuration["CosmosDb:Key"]
            );

            var database = cosmosClient.GetDatabase(configuration["CosmosDb:DatabaseName"]);
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