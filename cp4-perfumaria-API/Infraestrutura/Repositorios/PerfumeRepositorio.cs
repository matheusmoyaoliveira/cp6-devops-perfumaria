using Microsoft.EntityFrameworkCore;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Interfaces;
using Perfumaria.API.Infraestrutura.Dados;

namespace Perfumaria.API.Infraestrutura.Repositorios;

public class PerfumeRepositorio : IPerfumeRepositorio
{
    private readonly AppDbContext _contexto;

    public PerfumeRepositorio(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Perfume>> ObterTodosAsync()
    {
        return await _contexto.Perfumes
            .Include(p => p.Notas)
                .ThenInclude(pn => pn.NotaOlfativa)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Perfume?> ObterPorIdAsync(int id)
    {
        return await _contexto.Perfumes
            .Include(p => p.Notas)
                .ThenInclude(pn => pn.NotaOlfativa)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AdicionarAsync(Perfume perfume)
    {
        await _contexto.Perfumes.AddAsync(perfume);
        await _contexto.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Perfume perfume)
    {
        _contexto.Perfumes.Update(perfume);
        await _contexto.SaveChangesAsync();
    }

    public async Task RemoverAsync(Perfume perfume)
    {
        _contexto.Perfumes.Remove(perfume);
        await _contexto.SaveChangesAsync();
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _contexto.Perfumes.AnyAsync(p => p.Id == id);
    }
}