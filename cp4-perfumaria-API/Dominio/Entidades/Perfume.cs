using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Dominio.Entidades;

public class Perfume
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public GeneroPerfume Genero { get; private set; }
    public int VolumeMl { get; private set; }
    public decimal Preco { get; private set; }
    public int AnoLancamento { get; private set; }

    private readonly List<PerfumeNota> _notas = new();
    public IReadOnlyCollection<PerfumeNota> Notas => _notas.AsReadOnly();

    protected Perfume() { } // necessário para o EF Core materializar via reflexão

    public Perfume(string nome, string marca, GeneroPerfume genero, int volumeMl, decimal preco, int anoLancamento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do perfume não pode ser vazio.", nameof(nome));
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("A marca não pode ser vazia.", nameof(marca));
        if (volumeMl <= 0)
            throw new ArgumentException("O volume deve ser maior que zero.", nameof(volumeMl));
        if (preco <= 0)
            throw new ArgumentException("O preço deve ser maior que zero.", nameof(preco));

        Nome = nome;
        Marca = marca;
        Genero = genero;
        VolumeMl = volumeMl;
        Preco = preco;
        AnoLancamento = anoLancamento;
    }

    public void AdicionarNota(NotaOlfativa nota, PosicaoNota posicao)
    {
        if (nota is null)
            throw new ArgumentNullException(nameof(nota));
        if (_notas.Any(pn => pn.NotaOlfativaId == nota.Id))
            throw new InvalidOperationException("Esta nota já foi adicionada a este perfume.");

        _notas.Add(new PerfumeNota(this, nota, posicao));
    }
    public void AtualizarDados(string nome, string marca, GeneroPerfume genero, int volumeMl, decimal preco, int anoLancamento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do perfume não pode ser vazio.", nameof(nome));
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("A marca não pode ser vazia.", nameof(marca));
        if (volumeMl <= 0)
            throw new ArgumentException("O volume deve ser maior que zero.", nameof(volumeMl));
        if (preco <= 0)
            throw new ArgumentException("O preço deve ser maior que zero.", nameof(preco));

        Nome = nome;
        Marca = marca;
        Genero = genero;
        VolumeMl = volumeMl;
        Preco = preco;
        AnoLancamento = anoLancamento;
    }
}