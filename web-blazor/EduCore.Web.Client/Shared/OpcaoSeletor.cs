namespace EduCore.Web.Client.Shared;

/// <summary>Uma opção do <c>SeletorSegmentado</c>.</summary>
public sealed record OpcaoSeletor<TValor>(TValor Valor, string Rotulo);
