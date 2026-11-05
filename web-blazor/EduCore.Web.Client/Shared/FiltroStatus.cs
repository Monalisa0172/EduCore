namespace EduCore.Web.Client.Shared;

/// <summary>
/// Filtro das listas de cadastros com exclusão lógica (disciplinas, turmas...).
/// </summary>
public enum FiltroStatus
{
    Ativos,
    Inativos,
    Todos
}

public static class FiltroStatusExtensions
{
    public static bool Aceita(this FiltroStatus filtro, bool ativo) => filtro switch
    {
        FiltroStatus.Ativos => ativo,
        FiltroStatus.Inativos => !ativo,
        _ => true
    };
}
