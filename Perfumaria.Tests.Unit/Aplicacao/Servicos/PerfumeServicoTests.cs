using Moq;
using FluentAssertions;
using Perfumaria.API.Aplicacao.DTOs;
using Perfumaria.API.Aplicacao.Servicos;
using Perfumaria.API.Dominio.Entidades;
using Perfumaria.API.Dominio.Enums;
using Perfumaria.API.Dominio.Interfaces;
using Xunit;

namespace Perfumaria.Tests.Unit.Aplicacao.Servicos;

public class PerfumeServicoTests
{
    private readonly Mock<IPerfumeRepositorio> _perfumeRepositorioMock;
    private readonly Mock<INotaOlfativaRepositorio> _notaRepositorioMock;
    private readonly PerfumeServico _servico;

    public PerfumeServicoTests()
    {
        _perfumeRepositorioMock = new Mock<IPerfumeRepositorio>();
        _notaRepositorioMock = new Mock<INotaOlfativaRepositorio>();
        _servico = new PerfumeServico(_perfumeRepositorioMock.Object, _notaRepositorioMock.Object);
    }

    [Fact]
    public async Task Deve_CriarPerfume_QuandoDadosValidos()
    {
        // Arrange
        var dto = new PerfumeRequestDto
        {
            Nome = "Sauvage",
            Marca = "Dior",
            Genero = GeneroPerfume.Masculino,
            VolumeMl = 100,
            Preco = 649.90m,
            AnoLancamento = 2015
        };

        _perfumeRepositorioMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Perfume>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _servico.CriarAsync(dto);

        // Assert
        resultado.Nome.Should().Be(dto.Nome);
        _perfumeRepositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Perfume>()), Times.Once);
    }

    [Fact]
    public async Task Deve_RetornarNull_QuandoPerfumeNaoExiste()
    {
        // Arrange
        _perfumeRepositorioMock
            .Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Perfume?)null);

        // Act
        var resultado = await _servico.ObterPorIdAsync(99);

        // Assert
        resultado.Should().BeNull();
        _perfumeRepositorioMock.Verify(r => r.ObterPorIdAsync(99), Times.Once);
    }

    [Fact]
    public async Task Deve_RetornarFalse_QuandoRemoverPerfumeInexistente()
    {
        // Arrange
        _perfumeRepositorioMock
            .Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Perfume?)null);

        // Act
        var resultado = await _servico.RemoverAsync(1);

        // Assert
        resultado.Should().BeFalse();
        _perfumeRepositorioMock.Verify(r => r.RemoverAsync(It.IsAny<Perfume>()), Times.Never);
    }

    [Fact]
    public async Task Deve_RemoverPerfume_QuandoPerfumeExiste()
    {
        // Arrange
        var perfumeExistente = new Perfume("Nº5", "Chanel", GeneroPerfume.Feminino, 100, 899.90m, 1921);
        _perfumeRepositorioMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(perfumeExistente);

        // Act
        var resultado = await _servico.RemoverAsync(1);

        // Assert
        resultado.Should().BeTrue();
        _perfumeRepositorioMock.Verify(r => r.RemoverAsync(perfumeExistente), Times.Once);
    }

    [Fact]
    public async Task Deve_AdicionarNota_QuandoPerfumeENotaExistem()
    {
        // Arrange
        var perfume = new Perfume("Nº5", "Chanel", GeneroPerfume.Feminino, 100, 899.90m, 1921);
        var nota = new NotaOlfativa("Bergamota", FamiliaOlfativa.Citrico);
        var dto = new AdicionarNotaRequestDto { NotaOlfativaId = 1, Posicao = PosicaoNota.Topo };

        _perfumeRepositorioMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(perfume);
        _notaRepositorioMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(nota);

        // Act
        var resultado = await _servico.AdicionarNotaAsync(1, dto);

        // Assert
        resultado.Should().BeTrue();
        perfume.Notas.Should().ContainSingle(pn => pn.Posicao == PosicaoNota.Topo);
        _perfumeRepositorioMock.Verify(r => r.AtualizarAsync(perfume), Times.Once);
    }

    [Fact]
    public async Task Deve_RetornarFalse_QuandoAdicionarNota_ComPerfumeInexistente()
    {
        // Arrange
        var dto = new AdicionarNotaRequestDto { NotaOlfativaId = 1, Posicao = PosicaoNota.Topo };
        _perfumeRepositorioMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((Perfume?)null);

        // Act
        var resultado = await _servico.AdicionarNotaAsync(1, dto);

        // Assert
        resultado.Should().BeFalse();
        _notaRepositorioMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
    }
}