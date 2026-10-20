using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Aplicacao.DTOs;

public class NotaOlfativaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public FamiliaOlfativa Familia { get; set; }
}