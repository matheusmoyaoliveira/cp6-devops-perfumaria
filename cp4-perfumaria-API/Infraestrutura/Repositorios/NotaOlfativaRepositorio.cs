using Microsoft.EntityFrameworkCore;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Interfaces;
using Perfumaria.API.Infraestrutura.Dados;

namespace Perfumaria.API.Infraestrutura.Repositorios;

public class NotaOlfativaRepositorio : INotaOlfativaRepositorio
{
    private readonly AppDbContext _contexto;

    public NotaOlfativaRepositorio(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<NotaOlfativa>> ObterTodosAsync()
    {
        return await _contexto.NotasOlfativas.AsNoTracking().ToListAsync();
    }

    public async Task<NotaOlfativa?> ObterPorIdAsync(int id)
    {
        return await _contexto.NotasOlfativas.FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task AdicionarAsync(NotaOlfativa nota)
    {
        await _contexto.NotasOlfativas.AddAsync(nota);
        await _contexto.SaveChangesAsync();
    }

    public async Task AtualizarAsync(NotaOlfativa nota)
    {
        _contexto.NotasOlfativas.Update(nota);
        await _contexto.SaveChangesAsync();
    }

    public async Task RemoverAsync(NotaOlfativa nota)
    {
        _contexto.NotasOlfativas.Remove(nota);
        await _contexto.SaveChangesAsync();
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _contexto.NotasOlfativas.AnyAsync(n => n.Id == id);
    }
}