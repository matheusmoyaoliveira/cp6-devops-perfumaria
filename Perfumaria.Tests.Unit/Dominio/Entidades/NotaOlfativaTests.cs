using FluentAssertions;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Enums;
using Xunit;

namespace Perfumaria.Tests.Unit.Dominio.Entidades;

public class NotaOlfativaTests
{
    [Fact]
    public void Deve_CriarNotaOlfativa_QuandoDadosValidos()
    {
        // Arrange
        const string nome = "Bergamota";
        const FamiliaOlfativa familia = FamiliaOlfativa.Citrico;

        // Act
        var nota = new NotaOlfativa(nome, familia);

        // Assert
        nota.Nome.Should().Be(nome);
        nota.Familia.Should().Be(familia);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_QuandoNomeInvalido(string nome)
    {
        // Arrange
        Action act = () => new NotaOlfativa(nome, FamiliaOlfativa.Floral);

        // Act & Assert
        act.Should().Throw<ArgumentException>();
    }
}