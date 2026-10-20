using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Perfumaria.API.Infraestrutura.Observabilidade;

public static class AplicacaoMetricas
{
    public const string NomeServico = "Perfumaria.API";

    private static readonly Meter Meter = new(NomeServico);
    public static readonly ActivitySource ActivitySource = new(NomeServico);

    public static readonly Counter<long> ContadorCadastros = Meter.CreateCounter<long>(
        name: "perfumaria.cadastros.total",
        unit: "{cadastro}",
        description: "Contagem total de cadastros realizados (perfumes e notas olfativas).");
}