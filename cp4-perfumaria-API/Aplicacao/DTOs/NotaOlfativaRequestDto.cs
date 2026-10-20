using System.ComponentModel.DataAnnotations;
using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Aplicacao.DTOs;

public class NotaOlfativaRequestDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public FamiliaOlfativa Familia { get; set; }
}