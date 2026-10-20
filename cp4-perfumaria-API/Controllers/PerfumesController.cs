using Microsoft.AspNetCore.Mvc;
using Perfumaria.API.Aplicacao.DTOs;
using Perfumaria.API.Aplicacao.Servicos;
using Perfumaria.API.Infraestrutura.Observabilidade;
using System.Diagnostics;

namespace Perfumaria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerfumesController : ControllerBase
{
    private readonly IPerfumeServico _servico;
    private readonly ILogger<PerfumesController> _logger;

    public PerfumesController(IPerfumeServico servico, ILogger<PerfumesController> logger)
    {
        _servico = servico;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var perfumes = await _servico.ObterTodosAsync();
        return Ok(perfumes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var perfume = await _servico.ObterPorIdAsync(id);
        return perfume is null ? NotFound() : Ok(perfume);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PerfumeRequestDto dto)
    {
        using var activity = AplicacaoMetricas.ActivitySource.StartActivity("CriarPerfume");
        activity?.SetTag("perfume.nome", dto.Nome);
        activity?.SetTag("perfume.marca", dto.Marca);

        try
        {
            var perfumeCriado = await _servico.CriarAsync(dto);

            activity?.SetTag("perfume.id", perfumeCriado.Id);
            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "perfume"),
                new KeyValuePair<string, object?>("status", "sucesso"));

            _logger.LogInformation("Perfume {PerfumeId} criado com sucesso: {Nome}", perfumeCriado.Id, perfumeCriado.Nome);
            return CreatedAtAction(nameof(ObterPorId), new { id = perfumeCriado.Id }, perfumeCriado);
        }
        catch (ArgumentException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "perfume"),
                new KeyValuePair<string, object?>("status", "falha"));

            _logger.LogWarning(ex, "Falha de validação ao criar perfume: {Mensagem}", ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PerfumeRequestDto dto)
    {
        using var activity = AplicacaoMetricas.ActivitySource.StartActivity("AtualizarPerfume");
        activity?.SetTag("perfume.id", id);

        try
        {
            var atualizado = await _servico.AtualizarAsync(id, dto);

            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "perfume"),
                new KeyValuePair<string, object?>("status", atualizado ? "sucesso" : "nao_encontrado"));

            if (atualizado)
                _logger.LogInformation("Perfume {PerfumeId} atualizado com sucesso", id);
            else
                _logger.LogWarning("Tentativa de atualizar perfume inexistente: {PerfumeId}", id);

            return atualizado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogWarning(ex, "Falha de validação ao atualizar perfume {PerfumeId}: {Mensagem}", id, ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        using var activity = AplicacaoMetricas.ActivitySource.StartActivity("RemoverPerfume");
        activity?.SetTag("perfume.id", id);

        var removido = await _servico.RemoverAsync(id);

        AplicacaoMetricas.ContadorCadastros.Add(1,
            new KeyValuePair<string, object?>("entidade", "perfume"),
            new KeyValuePair<string, object?>("status", removido ? "sucesso" : "nao_encontrado"));

        if (removido)
            _logger.LogInformation("Perfume {PerfumeId} removido com sucesso", id);
        else
            _logger.LogWarning("Tentativa de remover perfume inexistente: {PerfumeId}", id);

        return removido ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/notas")]
    public async Task<IActionResult> AdicionarNota(int id, [FromBody] AdicionarNotaRequestDto dto)
    {
        using var activity = AplicacaoMetricas.ActivitySource.StartActivity("AdicionarNotaAoPerfume");
        activity?.SetTag("perfume.id", id);
        activity?.SetTag("nota.id", dto.NotaOlfativaId);
        activity?.SetTag("nota.posicao", dto.Posicao.ToString());

        try
        {
            var sucesso = await _servico.AdicionarNotaAsync(id, dto);

            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "perfume_nota"),
                new KeyValuePair<string, object?>("status", sucesso ? "sucesso" : "nao_encontrado"));

            if (sucesso)
                _logger.LogInformation("Nota {NotaId} adicionada ao perfume {PerfumeId}", dto.NotaOlfativaId, id);
            else
                _logger.LogWarning("Falha ao adicionar nota {NotaId} ao perfume {PerfumeId}: recurso não encontrado", dto.NotaOlfativaId, id);

            return sucesso ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "perfume_nota"),
                new KeyValuePair<string, object?>("status", "falha"));

            _logger.LogWarning(ex, "Falha ao adicionar nota ao perfume {PerfumeId}: {Mensagem}", id, ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
    }
}