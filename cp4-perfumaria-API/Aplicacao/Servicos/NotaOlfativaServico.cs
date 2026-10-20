using Perfumaria.API.Aplicacao.DTOs;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Interfaces;

namespace Perfumaria.API.Aplicacao.Servicos;

public class NotaOlfativaServico : INotaOlfativaServico
{
    private readonly INotaOlfativaRepositorio _repositorio;

    public NotaOlfativaServico(INotaOlfativaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<NotaOlfativaResponseDto>> ObterTodosAsync()
    {
        var notas = await _repositorio.ObterTodosAsync();
        return notas.Select(MapearParaDto);
    }

    public async Task<NotaOlfativaResponseDto?> ObterPorIdAsync(int id)
    {
        var nota = await _repositorio.ObterPorIdAsync(id);
        return nota is null ? null : MapearParaDto(nota);
    }

    public async Task<NotaOlfativaResponseDto> CriarAsync(NotaOlfativaRequestDto dto)
    {
        var nota = new NotaOlfativa(dto.Nome, dto.Familia);
        await _repositorio.AdicionarAsync(nota);
        return MapearParaDto(nota);
    }

    public async Task<bool> AtualizarAsync(int id, NotaOlfativaRequestDto dto)
    {
        var nota = await _repositorio.ObterPorIdAsync(id);
        if (nota is null) return false;

        nota.AtualizarDados(dto.Nome, dto.Familia);
        await _repositorio.AtualizarAsync(nota);
        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var nota = await _repositorio.ObterPorIdAsync(id);
        if (nota is null) return false;

        await _repositorio.RemoverAsync(nota);
        return true;
    }

    private static NotaOlfativaResponseDto MapearParaDto(NotaOlfativa nota)
    {
        return new NotaOlfativaResponseDto { Id = nota.Id, Nome = nota.Nome, Familia = nota.Familia };
    }
}