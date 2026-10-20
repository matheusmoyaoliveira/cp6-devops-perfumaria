using System.ComponentModel.DataAnnotations;
using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Aplicacao.DTOs;

public class PerfumeRequestDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A marca é obrigatória.")]
    [MaxLength(100)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    public GeneroPerfume Genero { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "O volume deve ser maior que zero.")]
    public int VolumeMl { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    public int AnoLancamento { get; set; }
}