using Perfumaria.API.Aplicacao.DTOs;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Interfaces;

namespace Perfumaria.API.Aplicacao.Servicos;

public class PerfumeServico : IPerfumeServico
{
    private readonly IPerfumeRepositorio _perfumeRepositorio;
    private readonly INotaOlfativaRepositorio _notaRepositorio;

    public PerfumeServico(IPerfumeRepositorio perfumeRepositorio, INotaOlfativaRepositorio notaRepositorio)
    {
        _perfumeRepositorio = perfumeRepositorio;
        _notaRepositorio = notaRepositorio;
    }

    public async Task<IEnumerable<PerfumeResponseDto>> ObterTodosAsync()
    {
        var perfumes = await _perfumeRepositorio.ObterTodosAsync();
        return perfumes.Select(MapearParaDto);
    }

    public async Task<PerfumeResponseDto?> ObterPorIdAsync(int id)
    {
        var perfume = await _perfumeRepositorio.ObterPorIdAsync(id);
        return perfume is null ? null : MapearParaDto(perfume);
    }

    public async Task<PerfumeResponseDto> CriarAsync(PerfumeRequestDto dto)
    {
        var perfume = new Perfume(dto.Nome, dto.Marca, dto.Genero, dto.VolumeMl, dto.Preco, dto.AnoLancamento);
        await _perfumeRepositorio.AdicionarAsync(perfume);
        return MapearParaDto(perfume);
    }

    public async Task<bool> AtualizarAsync(int id, PerfumeRequestDto dto)
    {
        var perfume = await _perfumeRepositorio.ObterPorIdAsync(id);
        if (perfume is null) return false;

        perfume.AtualizarDados(dto.Nome, dto.Marca, dto.Genero, dto.VolumeMl, dto.Preco, dto.AnoLancamento);
        await _perfumeRepositorio.AtualizarAsync(perfume);
        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var perfume = await _perfumeRepositorio.ObterPorIdAsync(id);
        if (perfume is null) return false;

        await _perfumeRepositorio.RemoverAsync(perfume);
        return true;
    }

    public async Task<bool> AdicionarNotaAsync(int perfumeId, AdicionarNotaRequestDto dto)
    {
        var perfume = await _perfumeRepositorio.ObterPorIdAsync(perfumeId);
        if (perfume is null) return false;

        var nota = await _notaRepositorio.ObterPorIdAsync(dto.NotaOlfativaId);
        if (nota is null) return false;

        perfume.AdicionarNota(nota, dto.Posicao);
        await _perfumeRepositorio.AtualizarAsync(perfume);
        return true;
    }

    private static PerfumeResponseDto MapearParaDto(Perfume perfume)
    {
        return new PerfumeResponseDto
        {
            Id = perfume.Id,
            Nome = perfume.Nome,
            Marca = perfume.Marca,
            Genero = perfume.Genero,
            VolumeMl = perfume.VolumeMl,
            Preco = perfume.Preco,
            AnoLancamento = perfume.AnoLancamento,
            Notas = perfume.Notas.Select(pn => new PerfumeNotaResponseDto
            {
                NotaOlfativaId = pn.NotaOlfativaId,
                NomeNota = pn.NotaOlfativa.Nome,
                Posicao = pn.Posicao
            }).ToList()
        };
    }
}