using System.Security.Claims;
using EduCore.Web.Client.Auth;
using Microsoft.AspNetCore.Components.Routing;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;

namespace EduCore.Web.Client.Layout.Navegacao;

/// <summary>
/// Define o menu lateral de cada perfil. Ao liberar uma tela nova, basta marcar o item como disponível
/// (ou incluí-lo) aqui; o NavMenu apenas renderiza o resultado.
/// </summary>
/// <remarks>
/// O menu é só navegação: a autorização real fica em cada página (AuthorizeView) e na API.
/// </remarks>
public static class MenuPorPerfil
{
    private static readonly MenuItem Inicio = new(
        "Início", "/",
        new Icons.Regular.Size20.Home(), new Icons.Filled.Size20.Home(),
        Match: NavLinkMatch.All);

    private static readonly IReadOnlyList<MenuSecao> MenuAdmin =
    [
        new(null, [Inicio]),
        new("Pessoas",
        [
            new("Novo usuário", "usuarios/novo",
                new Icons.Regular.Size20.PersonAdd(), new Icons.Filled.Size20.PersonAdd()),
        ]),
        new("Acadêmico",
        [
            new("Disciplinas", "disciplinas",
                new Icons.Regular.Size20.BookOpen(), new Icons.Filled.Size20.BookOpen()),
            new("Turmas", "turmas",
                new Icons.Regular.Size20.PeopleTeam(), new Icons.Filled.Size20.PeopleTeam()),
        ]),
    ];

    private static readonly IReadOnlyList<MenuSecao> MenuProfessor =
    [
        new(null, [Inicio]),
        new("Sala de aula",
        [
            // Avaliacoes, notas e chamada sao sempre de uma turma + disciplina: ficam dentro de "Minhas turmas".
            new("Minhas turmas", "professor/turmas",
                new Icons.Regular.Size20.PeopleTeam(), new Icons.Filled.Size20.PeopleTeam()),
        ]),
    ];

    private static readonly IReadOnlyList<MenuSecao> MenuAluno =
    [
        new(null, [Inicio]),
        new("Vida escolar",
        [
            new("Boletim", "aluno/boletim",
                new Icons.Regular.Size20.DocumentBulletList(), new Icons.Filled.Size20.DocumentBulletList(), Disponivel: false),
            new("Frequência", "aluno/frequencia",
                new Icons.Regular.Size20.CalendarCheckmark(), new Icons.Filled.Size20.CalendarCheckmark(), Disponivel: false),
        ]),
    ];

    private static readonly IReadOnlyList<MenuSecao> MenuPadrao = [new(null, [Inicio])];

    /// <summary>
    /// Retorna as seções de menu do usuário autenticado, conforme o perfil (role) do token.
    /// </summary>
    public static IReadOnlyList<MenuSecao> Para(ClaimsPrincipal usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (usuario.IsInRole(Roles.Admin)) return MenuAdmin;
        if (usuario.IsInRole(Roles.Professor)) return MenuProfessor;
        if (usuario.IsInRole(Roles.Aluno)) return MenuAluno;

        return MenuPadrao;
    }
}
