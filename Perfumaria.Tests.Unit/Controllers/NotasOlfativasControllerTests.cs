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

public class NotasOlfativasControllerTests
{
    private readonly Mock<INotaOlfativaServico> _servicoMock;
    private readonly Mock<ILogger<NotasOlfativasController>> _loggerMock;
    private readonly NotasOlfativasController _controller;

    public NotasOlfativasControllerTests()
    {
        _servicoMock = new Mock<INotaOlfativaServico>();
        _loggerMock = new Mock<ILogger<NotasOlfativasController>>();
        _controller = new NotasOlfativasController(_servicoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObterTodos_Deve_RetornarOk_ComListaDeNotas()
    {
        // Arrange
        var lista = new List<NotaOlfativaResponseDto>
        {
            new() { Id = 1, Nome = "Bergamota", Familia = FamiliaOlfativa.Citrico }
        };
        _servicoMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(lista);

        // Act
        var resultado = await _controller.ObterTodos();

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)resultado).Value.Should().BeEquivalentTo(lista);
    }

    [Fact]
    public async Task Criar_Deve_RetornarCreated_QuandoDtoValido()
    {
        // Arrange
        var dtoRequest = new NotaOlfativaRequestDto { Nome = "Sândalo", Familia = FamiliaOlfativa.Amadeirado };
        var dtoResponse = new NotaOlfativaResponseDto { Id = 1, Nome = "Sândalo", Familia = FamiliaOlfativa.Amadeirado };

        _servicoMock.Setup(s => s.CriarAsync(dtoRequest)).ReturnsAsync(dtoResponse);

        // Act
        var resultado = await _controller.Criar(dtoRequest);

        // Assert
        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Atualizar_Deve_RetornarNotFound_QuandoNotaNaoExiste()
    {
        // Arrange
        var dto = new NotaOlfativaRequestDto { Nome = "Sândalo", Familia = FamiliaOlfativa.Amadeirado };
        _servicoMock.Setup(s => s.AtualizarAsync(99, dto)).ReturnsAsync(false);

        // Act
        var resultado = await _controller.Atualizar(99, dto);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
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
}