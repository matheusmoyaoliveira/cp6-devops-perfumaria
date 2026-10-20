using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Aplicacao.DTOs;

public class PerfumeResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public GeneroPerfume Genero { get; set; }
    public int VolumeMl { get; set; }
    public decimal Preco { get; set; }
    public int AnoLancamento { get; set; }
    public List<PerfumeNotaResponseDto> Notas { get; set; } = new();
}