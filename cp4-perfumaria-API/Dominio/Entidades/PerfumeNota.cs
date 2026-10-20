using Perfumaria.API.Dominio.Enums;

namespace Perfumaria.API.Dominio.Entidades;

public class PerfumeNota
{
    public int PerfumeId { get; private set; }
    public Perfume Perfume { get; private set; } = null!;
    public int NotaOlfativaId { get; private set; }
    public NotaOlfativa NotaOlfativa { get; private set; } = null!;
    public PosicaoNota Posicao { get; private set; }

    protected PerfumeNota() { }

    public PerfumeNota(Perfume perfume, NotaOlfativa notaOlfativa, PosicaoNota posicao)
    {
        Perfume = perfume ?? throw new ArgumentNullException(nameof(perfume));
        NotaOlfativa = notaOlfativa ?? throw new ArgumentNullException(nameof(notaOlfativa));
        Posicao = posicao;
    }
}