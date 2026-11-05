using EduCore.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Converte um <see cref="ResultadoOperacao{T}"/> na resposta HTTP padronizada (ProblemDetails).
/// </summary>
public static class ResultadoOperacaoExtensions
{
    public static IActionResult ParaResposta<T>(
        this ControllerBase controller,
        ResultadoOperacao<T> resultado,
        Func<T, IActionResult> sucesso)
    {
        switch (resultado.Falha)
        {
            case null:
                return sucesso(resultado.Valor!);

            case FalhaOperacao.NaoEncontrado:
                return controller.Problem(
                    detail: resultado.Mensagem,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Registro não encontrado");

            case FalhaOperacao.Conflito:
                // 409 com o erro no campo: o front destaca o campo certo.
                controller.ModelState.AddModelError(resultado.Campo!, resultado.Mensagem!);
                return controller.ValidationProblem(
                    detail: resultado.Mensagem,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Registro duplicado",
                    modelStateDictionary: controller.ModelState);

            default:
                // detail com a mensagem: telas sem o campo correspondente (ex.: dialogo de matricula) exibem o texto certo.
                controller.ModelState.AddModelError(resultado.Campo!, resultado.Mensagem!);
                return controller.ValidationProblem(
                    detail: resultado.Mensagem,
                    modelStateDictionary: controller.ModelState);
        }
    }
}
