using Moq;
using FluentAssertions;
using Perfumaria.API.Aplicacao.DTOs;
using Perfumaria.API.Aplicacao.Servicos;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Enums;
using Perfumaria.API.Dominio.Interfaces;
using Xunit;

namespace Perfumaria.Tests.Unit.Aplicacao.Servicos;

public class NotaOlfativaServicoTests
{
    private readonly Mock<INotaOlfativaRepositorio> _repositorioMock;
    private readonly NotaOlfativaServico _servico;

    public NotaOlfativaServicoTests()
    {
        _repositorioMock = new Mock<INotaOlfativaRepositorio>();
        _servico = new NotaOlfativaServico(_repositorioMock.Object);
    }

    [Fact]
    public async Task Deve_CriarNota_QuandoDadosValidos()
    {
        // Arrange
        var dto = new NotaOlfativaRequestDto { Nome = "Sândalo", Familia = FamiliaOlfativa.Amadeirado };
        _repositorioMock.Setup(r => r.AdicionarAsync(It.IsAny<NotaOlfativa>())).Returns(Task.CompletedTask);

        // Act
        var resultado = await _servico.CriarAsync(dto);

        // Assert
        resultado.Nome.Should().Be(dto.Nome);
        _repositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<NotaOlfativa>()), Times.Once);
    }

    [Fact]
    public async Task Deve_RetornarFalse_QuandoAtualizarNotaInexistente()
    {
        // Arrange
        var dto = new NotaOlfativaRequestDto { Nome = "Sândalo", Familia = FamiliaOlfativa.Amadeirado };
        _repositorioMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((NotaOlfativa?)null);

        // Act
        var resultado = await _servico.AtualizarAsync(1, dto);

        // Assert
        resultado.Should().BeFalse();
        _repositorioMock.Verify(r => r.AtualizarAsync(It.IsAny<NotaOlfativa>()), Times.Never);
    }

    [Fact]
    public async Task Deve_AtualizarNota_QuandoNotaExiste()
    {
        // Arrange
        var notaExistente = new NotaOlfativa("Bergamota", FamiliaOlfativa.Citrico);
        var dto = new NotaOlfativaRequestDto { Nome = "Bergamota Siciliana", Familia = FamiliaOlfativa.Citrico };

        _repositorioMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(notaExistente);

        // Act
        var resultado = await _servico.AtualizarAsync(1, dto);

        // Assert
        resultado.Should().BeTrue();
        notaExistente.Nome.Should().Be("Bergamota Siciliana");
        _repositorioMock.Verify(r => r.AtualizarAsync(notaExistente), Times.Once);
    }
}