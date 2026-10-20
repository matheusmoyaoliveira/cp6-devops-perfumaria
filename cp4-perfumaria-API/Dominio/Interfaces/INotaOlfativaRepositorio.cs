using Perfumaria.API.Dominio.Entidades;

namespace Perfumaria.API.Dominio.Interfaces;

public interface INotaOlfativaRepositorio
{
    Task<IEnumerable<NotaOlfativa>> ObterTodosAsync();
    Task<NotaOlfativa?> ObterPorIdAsync(int id);
    Task AdicionarAsync(NotaOlfativa nota);
    Task AtualizarAsync(NotaOlfativa nota);
    Task RemoverAsync(NotaOlfativa nota);
    Task<bool> ExisteAsync(int id);
}