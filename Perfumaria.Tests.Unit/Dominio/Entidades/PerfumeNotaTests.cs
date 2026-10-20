using FluentAssertions;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Enums;
using Xunit;

namespace Perfumaria.Tests.Unit.Dominio.Entidades;

public class PerfumeNotaTests
{
    [Fact]
    public void Deve_CriarPerfumeNota_QuandoDadosValidos()
    {
        // Arrange
        var perfume = new Perfume("Nº5", "Chanel", GeneroPerfume.Feminino, 100, 899.90m, 1921);
        var nota = new NotaOlfativa("Bergamota", FamiliaOlfativa.Citrico);

        // Act
        var perfumeNota = new PerfumeNota(perfume, nota, PosicaoNota.Topo);

        // Assert
        perfumeNota.Perfume.Should().Be(perfume);
        perfumeNota.NotaOlfativa.Should().Be(nota);
        perfumeNota.Posicao.Should().Be(PosicaoNota.Topo);
    }

    [Fact]
    public void Deve_LancarExcecao_QuandoPerfumeNulo()
    {
        // Arrange
        var nota = new NotaOlfativa("Bergamota", FamiliaOlfativa.Citrico);
        Action act = () => new PerfumeNota(null!, nota, PosicaoNota.Topo);

        // Act & Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deve_LancarExcecao_QuandoNotaOlfativaNula()
    {
        // Arrange
        var perfume = new Perfume("Nº5", "Chanel", GeneroPerfume.Feminino, 100, 899.90m, 1921);
        Action act = () => new PerfumeNota(perfume, null!, PosicaoNota.Fundo);

        // Act & Assert
        act.Should().Throw<ArgumentNullException>();
    }
}