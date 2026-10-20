using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Aplicacao.DTOs;

public class PerfumeNotaResponseDto
{
    public int NotaOlfativaId { get; set; }
    public string NomeNota { get; set; } = string.Empty;
    public PosicaoNota Posicao { get; set; }
}