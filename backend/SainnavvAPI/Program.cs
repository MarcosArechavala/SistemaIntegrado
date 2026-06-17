using SainnavvAPI.Data;
// NO hace falta un using especial para AddNewtonsoftJson, 
// pero asegurate de que el paquete se haya instalado correctamente en el paso 1.

var builder = WebApplication.CreateBuilder(args);

// CONFIGURAR LOS CONTROLADORES + JSON AQUÍ
// El error ocurría porque el paquete no era compatible. 
// Con la versión 8.0.x, esto funcionará:
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient<SainnavvAPI.Services.HospitalService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<HistoriaRepository>();

var app = builder.Build();

app.UseCors(policy =>
    policy.WithOrigins("http://localhost:4200")
          .AllowAnyMethod()
          .AllowAnyHeader());

// app.UseHttpsRedirection(); // Mantenla comentada si te da problemas locales

app.UseAuthorization();
app.MapControllers();
app.Run();