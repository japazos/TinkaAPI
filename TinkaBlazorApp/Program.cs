
var builder = WebApplication.CreateBuilder(args);

// Habilitar errores detallados
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

// Añadir servicios al contenedor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(); // No es necesario agregarlo dos veces
builder.Services.AddHttpClient("TinkaApi", client =>
{
    client.BaseAddress = new Uri("https://predicciontinka.azurewebsites.net/api/"); // URL de tu API
});

// Configurar la aplicación
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Valor predeterminado de HSTS es 30 días. Puede cambiarse para escenarios de producción, ver https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

