using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Perfumaria.API.Aplicacao.DTOs;
using Perfumaria.API.Aplicacao.Servicos;
using Perfumaria.API.Controllers;
using Perfumaria.API.Dominio.Enums;
using Xunit;

namespace Perfumaria.Tests.Unit.Controllers;

public class PerfumesControllerTests
{
    private readonly Mock<IPerfumeServico> _servicoMock;
    private readonly Mock<ILogger<PerfumesController>> _loggerMock;
    private readonly PerfumesController _controller;

    public PerfumesControllerTests()
    {
        _servicoMock = new Mock<IPerfumeServico>();
        _loggerMock = new Mock<ILogger<PerfumesController>>();
        _controller = new PerfumesController(_servicoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObterPorId_Deve_RetornarOk_QuandoPerfumeExiste()
    {
        // Arrange
        var dto = new PerfumeResponseDto { Id = 1, Nome = "Sauvage" };
        _servicoMock.Setup(s => s.ObterPorIdAsync(1)).ReturnsAsync(dto);

        // Act
        var resultado = await _controller.ObterPorId(1);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)resultado).Value.Should().Be(dto);
    }

    [Fact]
    public async Task ObterPorId_Deve_RetornarNotFound_QuandoPerfumeNaoExiste()
    {
        // Arrange
        _servicoMock.Setup(s => s.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((PerfumeResponseDto?)null);

        // Act
        var resultado = await _controller.ObterPorId(99);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Criar_Deve_RetornarCreated_QuandoDtoValido()
    {
        // Arrange
        var dtoRequest = new PerfumeRequestDto { Nome = "Sauvage", Marca = "Dior", Genero = GeneroPerfume.Masculino, VolumeMl = 100, Preco = 649.90m, AnoLancamento = 2015 };
        var dtoResponse = new PerfumeResponseDto { Id = 1, Nome = "Sauvage" };

        _servicoMock.Setup(s => s.CriarAsync(dtoRequest)).ReturnsAsync(dtoResponse);

        // Act
        var resultado = await _controller.Criar(dtoRequest);

        // Assert
        resultado.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)resultado).Value.Should().Be(dtoResponse);
    }

    [Fact]
    public async Task Criar_Deve_RetornarBadRequest_QuandoServicoLancaArgumentException()
    {
        // Arrange
        var dtoRequest = new PerfumeRequestDto { Nome = "", Marca = "Dior", Genero = GeneroPerfume.Masculino, VolumeMl = 100, Preco = 649.90m, AnoLancamento = 2015 };
        _servicoMock.Setup(s => s.CriarAsync(dtoRequest)).ThrowsAsync(new ArgumentException("Nome inválido"));

        // Act
        var resultado = await _controller.Criar(dtoRequest);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Remover_Deve_RetornarNoContent_QuandoRemovidoComSucesso()
    {
        // Arrange
        _servicoMock.Setup(s => s.RemoverAsync(1)).ReturnsAsync(true);

        // Act
        var resultado = await _controller.Remover(1);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Remover_Deve_RetornarNotFound_QuandoPerfumeNaoExiste()
    {
        // Arrange
        _servicoMock.Setup(s => s.RemoverAsync(It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var resultado = await _controller.Remover(99);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AdicionarNota_Deve_RetornarBadRequest_QuandoNotaJaAssociada()
    {
        // Arrange
        var dto = new AdicionarNotaRequestDto { NotaOlfativaId = 1, Posicao = PosicaoNota.Topo };
        _servicoMock.Setup(s => s.AdicionarNotaAsync(1, dto))
            .ThrowsAsync(new InvalidOperationException("Esta nota já foi adicionada a este perfume."));

        // Act
        var resultado = await _controller.AdicionarNota(1, dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }
}