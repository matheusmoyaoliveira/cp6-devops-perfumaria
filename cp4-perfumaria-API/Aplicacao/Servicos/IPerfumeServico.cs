using Perfumaria.API.Aplicacao.DTOs;

namespace Perfumaria.API.Aplicacao.Servicos;

public interface IPerfumeServico
{
    Task<IEnumerable<PerfumeResponseDto>> ObterTodosAsync();
    Task<PerfumeResponseDto?> ObterPorIdAsync(int id);
    Task<PerfumeResponseDto> CriarAsync(PerfumeRequestDto dto);
    Task<bool> AtualizarAsync(int id, PerfumeRequestDto dto);
    Task<bool> RemoverAsync(int id);
    Task<bool> AdicionarNotaAsync(int perfumeId, AdicionarNotaRequestDto dto);
}