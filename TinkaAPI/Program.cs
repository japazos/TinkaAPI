using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Asegúrate de que el entorno sea Production por defecto
        var environment = builder.Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Production";
        builder.Host.UseEnvironment(environment);

        // Configuración del cliente de Cosmos DB
        builder.Services.AddSingleton(s =>
        {
            var configuration = s.GetRequiredService<IConfiguration>();
            var cosmosClient = new CosmosClient(
                configuration.GetValue<string>("COSMOS_DB_ACCOUNT"),
                configuration.GetValue<string>("COSMOS_DB_KEY")
            );
            var database = cosmosClient.GetDatabase(configuration["CosmosDb:DatabaseName"]);
            var sorteoContainer = database.GetContainer("TinkaPrediccion");
            return new TinkaService(sorteoContainer);
        });

        // Configuración del cliente HTTP para la API
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiSettings:BaseUrl"));
        });

        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TinkaAPI V1");
        });


        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        //app.MapControllers();
        app.Run();
    }
}

