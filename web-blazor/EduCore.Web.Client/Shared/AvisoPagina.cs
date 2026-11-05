namespace EduCore.Web.Client.Shared;

/// <summary>
/// Mensagem exibida no topo da página após uma ação (sucesso ou erro).
/// </summary>
public sealed record AvisoPagina(string Titulo, string Mensagem, bool Erro = false);
