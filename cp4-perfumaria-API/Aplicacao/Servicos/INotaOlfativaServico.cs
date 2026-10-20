using Perfumaria.API.Aplicacao.DTOs;

namespace Perfumaria.API.Aplicacao.Servicos;

public interface INotaOlfativaServico
{
    Task<IEnumerable<NotaOlfativaResponseDto>> ObterTodosAsync();
    Task<NotaOlfativaResponseDto?> ObterPorIdAsync(int id);
    Task<NotaOlfativaResponseDto> CriarAsync(NotaOlfativaRequestDto dto);
    Task<bool> AtualizarAsync(int id, NotaOlfativaRequestDto dto);
    Task<bool> RemoverAsync(int id);
}