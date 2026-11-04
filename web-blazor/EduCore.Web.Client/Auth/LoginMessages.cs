namespace EduCore.Web.Client.Auth;

/// <summary>
/// Textos exibidos ao usuário no fluxo de login, centralizados para facilitar revisão e localização.
/// </summary>
public static class LoginMessages
{
    public const string ValidationTitle = "Verifique os dados informados";
    public const string Validation = "Alguns campos precisam de correção. Confira as mensagens destacadas abaixo de cada campo.";

    // Mensagem única para e-mail inexistente ou senha errada: evita enumeração de usuários.
    public const string InvalidCredentialsTitle = "E-mail ou senha incorretos";
    public const string InvalidCredentials = "Confira se o e-mail está correto e se a tecla Caps Lock está desativada, depois tente novamente.";

    public const string AccessDeniedTitle = "Acesso não permitido";
    public const string AccessDenied = "Seu usuário não tem permissão para acessar o sistema. Procure o administrador.";

    public const string AccountLockedTitle = "Conta bloqueada temporariamente";
    public const string AccountLocked = "Por segurança, sua conta foi bloqueada após várias tentativas sem sucesso. Tente novamente mais tarde ou procure o administrador.";

    public const string TooManyAttemptsTitle = "Muitas tentativas de acesso";
    public const string TooManyAttempts = "Aguarde alguns instantes antes de tentar novamente.";
    public const string TooManyAttemptsWithDelay = "Aguarde {0} segundo(s) antes de tentar novamente.";

    public const string ServerErrorTitle = "Serviço indisponível";
    public const string ServerError = "O serviço de autenticação está com instabilidade. Tente novamente em alguns instantes.";

    public const string UnavailableTitle = "Sem conexão com o servidor";
    public const string Unavailable = "Não foi possível se comunicar com o servidor. Verifique sua conexão com a internet e tente novamente.";

    public const string TimeoutTitle = "O servidor demorou para responder";
    public const string Timeout = "A solicitação excedeu o tempo limite. Tente novamente em alguns instantes.";

    public const string InvalidResponseTitle = "Resposta inesperada do servidor";
    public const string InvalidResponse = "O servidor retornou uma resposta que não pôde ser processada. Se o problema persistir, procure o suporte.";

    public const string UnexpectedTitle = "Não foi possível entrar";
    public const string Unexpected = "Ocorreu um erro inesperado (código {0}). Se o problema persistir, procure o suporte.";
}
