using Microsoft.AspNetCore.Mvc;
using Perfumaria.API.Aplicacao.DTOs;
using Perfumaria.API.Aplicacao.Servicos;
using Perfumaria.API.Infraestrutura.Observabilidade;
using System.Diagnostics;

namespace Perfumaria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotasOlfativasController : ControllerBase
{
    private readonly INotaOlfativaServico _servico;
    private readonly ILogger<NotasOlfativasController> _logger;

    public NotasOlfativasController(INotaOlfativaServico servico, ILogger<NotasOlfativasController> logger)
    {
        _servico = servico;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var notas = await _servico.ObterTodosAsync();
        return Ok(notas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var nota = await _servico.ObterPorIdAsync(id);
        return nota is null ? NotFound() : Ok(nota);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] NotaOlfativaRequestDto dto)
    {
        using var activity = AplicacaoMetricas.ActivitySource.StartActivity("CriarNotaOlfativa");
        activity?.SetTag("nota.nome", dto.Nome);
        activity?.SetTag("nota.familia", dto.Familia.ToString());

        try
        {
            var criada = await _servico.CriarAsync(dto);

            activity?.SetTag("nota.id", criada.Id);
            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "nota_olfativa"),
                new KeyValuePair<string, object?>("status", "sucesso"));

            _logger.LogInformation("Nota olfativa {NotaId} criada com sucesso: {Nome}", criada.Id, criada.Nome);
            return CreatedAtAction(nameof(ObterPorId), new { id = criada.Id }, criada);
        }
        catch (ArgumentException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "nota_olfativa"),
                new KeyValuePair<string, object?>("status", "falha"));

            _logger.LogWarning(ex, "Falha de validação ao criar nota olfativa: {Mensagem}", ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] NotaOlfativaRequestDto dto)
    {
        using var activity = AplicacaoMetricas.ActivitySource.StartActivity("AtualizarNotaOlfativa");
        activity?.SetTag("nota.id", id);

        try
        {
            var atualizado = await _servico.AtualizarAsync(id, dto);

            AplicacaoMetricas.ContadorCadastros.Add(1,
                new KeyValuePair<string, object?>("entidade", "nota_olfativa"),
                new KeyValuePair<string, object?>("status", atualizado ? "sucesso" : "nao_encontrado"));

            if (atualizado)
                _logger.LogInformation("Nota olfativa {NotaId} atualizada com sucesso", id);
            else
                _logger.LogWarning("Tentativa de atualizar nota olfativa inexistente: {NotaId}", id);

            return atualizado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogWarning(ex, "Falha de validação ao atualizar nota olfativa {NotaId}: {Mensagem}", id, ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        using var activity = AplicacaoMetricas.ActivitySource.StartActivity("RemoverNotaOlfativa");
        activity?.SetTag("nota.id", id);

        var removido = await _servico.RemoverAsync(id);

        AplicacaoMetricas.ContadorCadastros.Add(1,
            new KeyValuePair<string, object?>("entidade", "nota_olfativa"),
            new KeyValuePair<string, object?>("status", removido ? "sucesso" : "nao_encontrado"));

        if (removido)
            _logger.LogInformation("Nota olfativa {NotaId} removida com sucesso", id);
        else
            _logger.LogWarning("Tentativa de remover nota olfativa inexistente: {NotaId}", id);

        return removido ? NoContent() : NotFound();
    }
}