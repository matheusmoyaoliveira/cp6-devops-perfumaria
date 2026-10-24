using FluentAssertions;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Enums;
using Xunit;

namespace Perfumaria.Tests.Unit.Dominio.Entidades;

public class PerfumeTests
{
    [Fact]
    public void Deve_CriarPerfume_QuandoDadosValidos()
    {
        // Arrange
        const string nome = "Bleu de Chanel";
        const string marca = "Chanel";
        const GeneroPerfume genero = GeneroPerfume.Masculino;
        const int volumeMl = 100;
        const decimal preco = 599.90m;
        const int anoLancamento = 2010;

        // Act
        var perfume = new Perfume(nome, marca, genero, volumeMl, preco, anoLancamento);

        // Assert
        perfume.Nome.Should().Be(nome);
        perfume.Marca.Should().Be(marca);
        perfume.Genero.Should().Be(genero);
        perfume.VolumeMl.Should().Be(volumeMl);
        perfume.Preco.Should().Be(preco);
        perfume.Notas.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Chanel", 100, 599.90)]
    [InlineData("Bleu de Chanel", "", 100, 599.90)]
    [InlineData("Bleu de Chanel", "Chanel", 0, 599.90)]
    [InlineData("Bleu de Chanel", "Chanel", -10, 599.90)]
    [InlineData("Bleu de Chanel", "Chanel", 100, 0)]
    [InlineData("Bleu de Chanel", "Chanel", 100, -50)]
    public void Deve_LancarExcecao_QuandoDadosInvalidos(string nome, string marca, int volumeMl, decimal preco)
    {
        // Arrange
        Action act = () => new Perfume(nome, marca, GeneroPerfume.Unissex, volumeMl, preco, 2020);

        // Act & Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deve_AdicionarNota_QuandoNotaValida()
    {
        // Arrange
        var perfume = new Perfume("Nº5", "Chanel", GeneroPerfume.Feminino, 100, 899.90m, 1921);
        var nota = new NotaOlfativa("Bergamota", FamiliaOlfativa.Citrico);

        // Act
        perfume.AdicionarNota(nota, PosicaoNota.Topo);

        // Assert
        perfume.Notas.Should().ContainSingle(pn => pn.Posicao == PosicaoNota.Topo);
    }
}