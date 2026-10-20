using System.ComponentModel.DataAnnotations;
using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Aplicacao.DTOs;

public class AdicionarNotaRequestDto
{
    [Required]
    public int NotaOlfativaId { get; set; }

    [Required]
    public PosicaoNota Posicao { get; set; }
}