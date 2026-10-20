using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Perfumaria.API.Aplicacao.Middlewares;
using Perfumaria.API.Aplicacao.Servicos;
using Perfumaria.API.Dominio.Interfaces;
using Perfumaria.API.Infraestrutura.Dados;
using Perfumaria.API.Infraestrutura.Health;
using Perfumaria.API.Infraestrutura.Observabilidade;
using Perfumaria.API.Infraestrutura.Repositorios;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuracao) =>
{
    configuracao
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            "logs/app-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddHealthChecks()
    .AddCheck<BancoDadosHealthCheck>("banco_dados_oracle");

builder.Services.AddScoped<IPerfumeRepositorio, PerfumeRepositorio>();
builder.Services.AddScoped<INotaOlfativaRepositorio, NotaOlfativaRepositorio>();
builder.Services.AddScoped<IPerfumeServico, PerfumeServico>();
builder.Services.AddScoped<INotaOlfativaServico, NotaOlfativaServico>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(recurso => recurso.AddService(AplicacaoMetricas.NomeServico))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(AplicacaoMetricas.NomeServico)
        .AddConsoleExporter())
    .WithMetrics(metricas => metricas
        .AddAspNetCoreInstrumentation()
        .AddMeter(AplicacaoMetricas.NomeServico)
        .AddConsoleExporter());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();