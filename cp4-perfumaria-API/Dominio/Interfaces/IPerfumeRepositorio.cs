using Perfumaria.API.Dominio.Entidades;

namespace Perfumaria.API.Dominio.Interfaces;

public interface IPerfumeRepositorio
{
    Task<IEnumerable<Perfume>> ObterTodosAsync();
    Task<Perfume?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Perfume perfume);
    Task AtualizarAsync(Perfume perfume);
    Task RemoverAsync(Perfume perfume);
    Task<bool> ExisteAsync(int id);
}