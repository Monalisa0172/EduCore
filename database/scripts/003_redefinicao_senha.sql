/*
    EduCore - 003_redefinicao_senha.sql
    "Esqueci minha senha": links de redefinicao de uso unico.

    O que faz:
      1. Cria a tabela RedefinicaoSenha. O banco guarda so o HASH (SHA-256) do token enviado por
         e-mail: quem ler a tabela nao consegue usar os links. Cada link vale 30 minutos e uma vez.
      2. Limpa senhas antigas gravadas em TEXTO PURO (sem o hash BCrypt, que comeca com "$2").
         Essas contas ja nao conseguiam entrar (o login recusa senha sem hash); agora a senha
         exposta some do banco e a pessoa define uma nova pelo "Esqueci minha senha".
         Nao ha backup dessas senhas de proposito: guarda-las seria manter o problema.

    Como rodar: abra no SSMS conectado ao banco do EduCore e execute (F5), depois do 001 e do 002.
    Seguro para rodar de novo: se ja foi aplicado, nao faz nada. Tudo roda numa transacao.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.HistoricoScripts', N'U') IS NULL
BEGIN
    RAISERROR('Aplique o 001_modelo_academico.sql antes deste script.', 16, 1);
    RETURN;
END;

IF EXISTS (SELECT 1 FROM dbo.HistoricoScripts WHERE Script = '003_redefinicao_senha')
BEGIN
    PRINT '003_redefinicao_senha ja foi aplicado neste banco. Nada a fazer.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    EXEC (N'CREATE TABLE dbo.RedefinicaoSenha
            (
                Id        int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RedefinicaoSenha PRIMARY KEY,
                UsuarioId int NOT NULL,
                TokenHash char(64) NOT NULL,
                CriadoEm  datetime2(0) NOT NULL,
                ExpiraEm  datetime2(0) NOT NULL,
                UsadoEm   datetime2(0) NULL,
                CONSTRAINT FK_RedefinicaoSenha_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id)
            );');
    EXEC (N'CREATE UNIQUE INDEX UX_RedefinicaoSenha_TokenHash ON dbo.RedefinicaoSenha (TokenHash);');
    EXEC (N'CREATE INDEX IX_RedefinicaoSenha_Usuario ON dbo.RedefinicaoSenha (UsuarioId, CriadoEm);');

    DECLARE @limpas int;
    UPDATE dbo.Usuario
       SET Senha = NULL
     WHERE Senha IS NOT NULL
       AND Senha NOT LIKE '$2[abxy]$%';
    SET @limpas = @@ROWCOUNT;

    INSERT INTO dbo.HistoricoScripts (Script) VALUES ('003_redefinicao_senha');

    COMMIT TRANSACTION;
    PRINT CONCAT('003_redefinicao_senha aplicado. Senhas em texto puro removidas: ', @limpas,
                 '. Essas pessoas definem uma nova senha pelo "Esqueci minha senha".');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Erro: nada foi alterado.';
    THROW;
END CATCH;
