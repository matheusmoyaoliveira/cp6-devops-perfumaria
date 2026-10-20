using Microsoft.Extensions.Diagnostics.HealthChecks;
using Perfumaria.API.Infraestrutura.Dados;

namespace Perfumaria.API.Infraestrutura.Health;

public class BancoDadosHealthCheck : IHealthCheck
{
    private readonly AppDbContext _contexto;

    public BancoDadosHealthCheck(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var conseguiuConectar = await _contexto.Database.CanConnectAsync(cancellationToken);

            return conseguiuConectar
                ? HealthCheckResult.Healthy("Conexão com o banco Oracle está saudável.")
                : HealthCheckResult.Unhealthy("Não foi possível estabelecer conexão com o banco Oracle.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Erro ao tentar validar a conexão com o banco Oracle.",
                exception: ex);
        }
    }
}