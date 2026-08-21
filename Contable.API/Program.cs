var builder = WebApplication.CreateBuilder(args);

// CreateBuilder ya carga appsettings.json y appsettings.{ASPNETCORE_ENVIRONMENT}.json, y encima
// de esos las variables de entorno. No hay que volver a agregar el archivo del entorno: eso lo
// dejaria por encima de las variables y ningun secreto se podria sobrescribir por entorno.
builder.Services.AddConfig(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (AppSettings.Uses.UseSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(opt => opt.DefaultModelsExpandDepth(-1));
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseCors();

// UseAuthentication va antes de UseAuthorization: es el que valida el token y llena
// HttpContext.User. Sin el, los claims solo se resuelven de rebote cuando hay una policy, con lo
// que en un entorno con UseAuth en false la API queda abierta y sin usuario para la auditoria.
if (AppSettings.Uses.UseJwt) app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
