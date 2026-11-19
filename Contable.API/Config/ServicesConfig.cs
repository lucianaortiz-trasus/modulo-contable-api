namespace Contable.API.Config;

/// <summary>
/// Armado del contenedor de dependencias.
/// </summary>
public static class ServicesConfig
{
    public static void AddConfig(this IServiceCollection services, IConfiguration configuration)
    {
        AppSettings.Bind(configuration);

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                    .WithHeaders("Authorization", "Content-Type", "token");
            });
        });

        services.AddSwagger();

        if (AppSettings.Uses.UseJwt) services.AddJwt();

        services.AddInternalServices();

        services.AddDbContext<EjecucionContext>(o => o.UseOracle(configuration.GetConnectionString("Ejecucion")));
        services.AddDbContext<PresupuestoContext>(o => o.UseOracle(configuration.GetConnectionString("Presupuesto")));
        services.AddDbContext<PortalContext>(o => o.UseOracle(configuration.GetConnectionString("Portal")));

        services.AddHealthChecks()
            .AddCheck<DatabaseEjecucionHealthCheck>(nameof(DatabaseEjecucionHealthCheck))
            .AddCheck<DatabasePresupuestoHealthCheck>(nameof(DatabasePresupuestoHealthCheck))
            .AddCheck<DatabasePortalHealthCheck>(nameof(DatabasePortalHealthCheck));

        services.AddControllers();

        services.Configure<KestrelServerOptions>(o => o.Limits.MaxRequestBodySize = 5242880);

        if (AppSettings.Uses.UseAuth)
        {
            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        }
    }

    private static void AddInternalServices(this IServiceCollection services)
    {
        // Cliente con nombre para hablar con el Portal. Con IHttpClientFactory y no con un
        // HttpClient suelto: reusa los sockets y respeta los cambios de DNS.
        services.AddHttpClient(SeguridadService.NombreDelCliente, cliente =>
        {
            cliente.Timeout = TimeSpan.FromSeconds(AppSettings.Portal.TimeoutSegundos);
        });

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ISeguridadService, SeguridadService>();
        services.AddScoped<IDiagnosticoService, DiagnosticoService>();
        services.AddScoped<IPlanDeCuentasService, PlanDeCuentasService>();
        services.AddScoped<IMatrizContableService, MatrizContableService>();
        services.AddScoped<IAsientoService, AsientoService>();
        services.AddScoped<IReportesService, ReportesService>();
        services.AddScoped<IEstadisticasService, EstadisticasService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IModelosAsientoService, ModelosAsientoService>();
    }

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API RESTful CONTABLE", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Ingrese el token con el formato: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                    },
                    Array.Empty<string>()
                },
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
        });
    }

    private static void AddJwt(this IServiceCollection services)
    {
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = AppSettings.Jwt.Issuer,
                ValidAudience = AppSettings.Jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppSettings.Jwt.SecretKey)),
                ClockSkew = TimeSpan.Zero,
            };
        });
    }
}
