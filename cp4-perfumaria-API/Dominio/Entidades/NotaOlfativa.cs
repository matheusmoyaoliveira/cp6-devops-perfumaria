using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Dominio.Entidades;

public class NotaOlfativa
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public FamiliaOlfativa Familia { get; private set; }

    private readonly List<PerfumeNota> _perfumes = new();
    public IReadOnlyCollection<PerfumeNota> Perfumes => _perfumes.AsReadOnly();

    protected NotaOlfativa() { }

    public NotaOlfativa(string nome, FamiliaOlfativa familia)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da nota olfativa não pode ser vazio.", nameof(nome));

        Nome = nome;
        Familia = familia;
    }
    public void AtualizarDados(string nome, FamiliaOlfativa familia)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da nota olfativa não pode ser vazio.", nameof(nome));

        Nome = nome;
        Familia = familia;
    }
}