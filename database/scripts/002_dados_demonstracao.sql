/*
    EduCore - 002_dados_demonstracao.sql
    Dados de demonstracao (portfolio): uma turma completa com professores, avaliacoes, notas e chamadas.
    Requer o 001_modelo_academico.sql aplicado antes.

    O que cria (sem duplicar o que ja existe):
      - Disciplinas Matematica, Portugues (com Gramatica e Literatura), Ciencias e Historia:
        reaproveita as que ja existem com o mesmo nome; so cria as que faltarem.
      - Turma "9o Ano A" (2026), 5 professores e 8 alunos de demonstracao.
      - Atribuicoes: Matematica, Ciencias e Historia inteiras; Portugues dividida (Gramatica + Literatura).
      - Avaliacoes dos 4 bimestres (pesos somando 100), notas das avaliacoes que ja aconteceram e
        chamadas ate hoje. Alunos com perfis diferentes (destaque, na media, em risco por nota e
        por frequencia) para o boletim e os graficos mostrarem todas as faixas.

    Acesso (todos com a senha Demo@2026):
      professora: renata.alves@demo.educore.com.br   (Matematica)
      aluna:      sofia.ribeiro@demo.educore.com.br
      aluno:      enzo.carvalho@demo.educore.com.br  (media abaixo de 6)
      aluna:      valentina.gomes@demo.educore.com.br (frequencia baixa)

    Como rodar: abra no SSMS conectado ao banco do EduCore e execute (F5).
    Seguro para rodar de novo: se ja foi aplicado, nao faz nada. Tudo roda numa transacao.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.HistoricoScripts WHERE Script = '001_modelo_academico')
BEGIN
    RAISERROR('Aplique o 001_modelo_academico.sql antes deste script.', 16, 1);
    RETURN;
END;

IF EXISTS (SELECT 1 FROM dbo.HistoricoScripts WHERE Script = '002_dados_demonstracao')
BEGIN
    PRINT '002_dados_demonstracao ja foi aplicado neste banco. Nada a fazer.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @ano int = 2026;
    DECLARE @hoje date = CAST(GETDATE() AS date);
    -- BCrypt de "Demo@2026" (mesmo algoritmo do login da API)
    DECLARE @senha varchar(255) = '$2a$11$9z2U3i6QD2RL7H3aE3ZvceRLwgJWQ6uYjDL7Rrn5B7cU7b/G0u/eK';

    /* ---------- Disciplinas e subdisciplinas (reaproveita pelo nome) ---------- */
    DECLARE @disciplinas TABLE (Chave varchar(10) PRIMARY KEY, Nome nvarchar(100), Codigo varchar(20), CargaHoraria int, Id int NULL);
    INSERT INTO @disciplinas (Chave, Nome, Codigo, CargaHoraria) VALUES
        ('MAT', N'Matemática', 'MAT', 160),
        ('POR', N'Português', 'POR', 160),
        ('CIE', N'Ciências', 'CIE', 80),
        ('HIS', N'História', 'HIS', 80);

    UPDATE d SET Id = (SELECT TOP 1 x.Id FROM dbo.Disciplina x WHERE x.Nome = d.Nome AND x.Ativo = 1 ORDER BY x.Id)
    FROM @disciplinas d;

    INSERT INTO dbo.Disciplina (Nome, Codigo, Descricao, CargaHoraria, Ativo)
    SELECT d.Nome, d.Codigo, NULL, d.CargaHoraria, 1 FROM @disciplinas d WHERE d.Id IS NULL;

    UPDATE d SET Id = (SELECT TOP 1 x.Id FROM dbo.Disciplina x WHERE x.Nome = d.Nome AND x.Ativo = 1 ORDER BY x.Id)
    FROM @disciplinas d WHERE d.Id IS NULL;

    DECLARE @mat int = (SELECT Id FROM @disciplinas WHERE Chave = 'MAT');
    DECLARE @por int = (SELECT Id FROM @disciplinas WHERE Chave = 'POR');
    DECLARE @cie int = (SELECT Id FROM @disciplinas WHERE Chave = 'CIE');
    DECLARE @his int = (SELECT Id FROM @disciplinas WHERE Chave = 'HIS');

    DECLARE @subs TABLE (Chave varchar(10) PRIMARY KEY, Nome nvarchar(100), Id int NULL);
    INSERT INTO @subs (Chave, Nome) VALUES ('GRA', N'Gramática'), ('LIT', N'Literatura');

    UPDATE s SET Id = (SELECT TOP 1 x.Id FROM dbo.SubDisciplina x WHERE x.DisciplinaId = @por AND x.Nome = s.Nome AND x.Ativo = 1 ORDER BY x.Id)
    FROM @subs s;

    INSERT INTO dbo.SubDisciplina (Nome, DisciplinaId, Descricao, Ativo)
    SELECT s.Nome, @por, NULL, 1 FROM @subs s WHERE s.Id IS NULL;

    UPDATE s SET Id = (SELECT TOP 1 x.Id FROM dbo.SubDisciplina x WHERE x.DisciplinaId = @por AND x.Nome = s.Nome AND x.Ativo = 1 ORDER BY x.Id)
    FROM @subs s WHERE s.Id IS NULL;

    DECLARE @gra int = (SELECT Id FROM @subs WHERE Chave = 'GRA');
    DECLARE @lit int = (SELECT Id FROM @subs WHERE Chave = 'LIT');

    /* ---------- Turma ---------- */
    DECLARE @turma int = (SELECT TOP 1 Id FROM dbo.Turma WHERE Nome = N'9º Ano A' AND AnoLetivo = @ano);
    IF @turma IS NULL
    BEGIN
        INSERT INTO dbo.Turma (Nome, AnoLetivo, Ativo) VALUES (N'9º Ano A', @ano, 1);
        SET @turma = SCOPE_IDENTITY();
    END;

    /* ---------- Professores (usuario -> funcionario -> professor) ---------- */
    DECLARE @professores TABLE (Chave varchar(10) PRIMARY KEY, Nome nvarchar(100), Email varchar(100), Especialidade nvarchar(100), Admissao date, ProfessorId int NULL);
    INSERT INTO @professores (Chave, Nome, Email, Especialidade, Admissao) VALUES
        ('RENATA', N'Renata Alves',  'renata.alves@demo.educore.com.br',  N'Matemática', '2019-02-04'),
        ('PAULO',  N'Paulo Mendes',  'paulo.mendes@demo.educore.com.br',  N'Língua Portuguesa', '2020-02-03'),
        ('LIVIA',  N'Lívia Campos',  'livia.campos@demo.educore.com.br',  N'Literatura', '2021-08-02'),
        ('TIAGO',  N'Tiago Nunes',   'tiago.nunes@demo.educore.com.br',   N'Ciências', '2018-02-05'),
        ('MARINA', N'Marina Lopes',  'marina.lopes@demo.educore.com.br',  N'História', '2022-02-07');

    INSERT INTO dbo.Usuario (Nome, Email, Senha, Tipo, Ativo)
    SELECT p.Nome, p.Email, @senha, 2, 1
    FROM @professores p
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuario u WHERE u.Email = p.Email);

    INSERT INTO dbo.Funcionario (UsuarioId, DataAdmissao, Cargo, Salario, TipoContrato, Status, Ativo)
    SELECT u.Id, p.Admissao, 'Professor', 5200, 1, 1, 1
    FROM @professores p
    JOIN dbo.Usuario u ON u.Email = p.Email
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Funcionario f WHERE f.UsuarioId = u.Id);

    INSERT INTO dbo.Professor (FuncionarioId, Especialidade)
    SELECT f.Id, p.Especialidade
    FROM @professores p
    JOIN dbo.Usuario u ON u.Email = p.Email
    JOIN dbo.Funcionario f ON f.UsuarioId = u.Id
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Professor x WHERE x.FuncionarioId = f.Id);

    UPDATE p SET ProfessorId = x.Id
    FROM @professores p
    JOIN dbo.Usuario u ON u.Email = p.Email
    JOIN dbo.Funcionario f ON f.UsuarioId = u.Id
    JOIN dbo.Professor x ON x.FuncionarioId = f.Id;

    /* ---------- Alunos e matriculas ---------- */
    -- Base: media tipica do aluno. Faltas: % de faltas nas aulas.
    DECLARE @alunos TABLE (Nome nvarchar(100), Email varchar(100), Base decimal(4,2), Faltas int, AlunoId int NULL);
    INSERT INTO @alunos (Nome, Email, Base, Faltas) VALUES
        (N'Sofia Ribeiro',    'sofia.ribeiro@demo.educore.com.br',    9.0, 3),
        (N'Lucas Ferreira',   'lucas.ferreira@demo.educore.com.br',   8.0, 4),
        (N'Manuela Pereira',  'manuela.pereira@demo.educore.com.br',  8.5, 5),
        (N'Isabela Costa',    'isabela.costa@demo.educore.com.br',    7.5, 4),
        (N'Gabriel Santos',   'gabriel.santos@demo.educore.com.br',   7.5, 6),
        (N'Rafael Oliveira',  'rafael.oliveira@demo.educore.com.br',  6.5, 18),
        (N'Enzo Carvalho',    'enzo.carvalho@demo.educore.com.br',    5.0, 8),
        (N'Valentina Gomes',  'valentina.gomes@demo.educore.com.br',  7.5, 30);

    INSERT INTO dbo.Usuario (Nome, Email, Senha, Tipo, Ativo)
    SELECT a.Nome, a.Email, @senha, 3, 1
    FROM @alunos a
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuario u WHERE u.Email = a.Email);

    INSERT INTO dbo.Aluno (UsuarioId)
    SELECT u.Id
    FROM @alunos a
    JOIN dbo.Usuario u ON u.Email = a.Email
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Aluno x WHERE x.UsuarioId = u.Id);

    UPDATE a SET AlunoId = x.Id
    FROM @alunos a
    JOIN dbo.Usuario u ON u.Email = a.Email
    JOIN dbo.Aluno x ON x.UsuarioId = u.Id;

    INSERT INTO dbo.Matricula (AlunoId, TurmaId)
    SELECT a.AlunoId, @turma
    FROM @alunos a
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Matricula m WHERE m.AlunoId = a.AlunoId AND m.TurmaId = @turma);

    /* ---------- Atribuicoes ---------- */
    INSERT INTO dbo.Atribuicao (ProfessorId, TurmaId, DisciplinaId, SubDisciplinaId, Ativo)
    SELECT p.ProfessorId, @turma, v.DisciplinaId, v.SubDisciplinaId, 1
    FROM (VALUES
            ('RENATA', @mat, CAST(NULL AS int)),
            ('PAULO',  @por, @gra),
            ('LIVIA',  @por, @lit),
            ('TIAGO',  @cie, NULL),
            ('MARINA', @his, NULL)
         ) v (Chave, DisciplinaId, SubDisciplinaId)
    JOIN @professores p ON p.Chave = v.Chave
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Atribuicao x
        WHERE x.Ativo = 1 AND x.TurmaId = @turma AND x.DisciplinaId = v.DisciplinaId
          AND (x.SubDisciplinaId = v.SubDisciplinaId OR (x.SubDisciplinaId IS NULL AND v.SubDisciplinaId IS NULL)));

    /* ---------- Avaliacoes dos 4 bimestres (pesos somam 100 por disciplina e bimestre) ---------- */
    -- Datas dentro do calendario padrao: 1o 01/02-15/04, 2o 16/04-30/06, 3o 01/08-30/09, 4o 01/10-15/12.
    DECLARE @datas TABLE (Bimestre int PRIMARY KEY, Prova date, Trabalho date);
    INSERT INTO @datas VALUES
        (1, DATEFROMPARTS(@ano, 3, 20), DATEFROMPARTS(@ano, 4, 8)),
        (2, DATEFROMPARTS(@ano, 5, 25), DATEFROMPARTS(@ano, 6, 19)),
        (3, DATEFROMPARTS(@ano, 8, 27), DATEFROMPARTS(@ano, 9, 22)),
        (4, DATEFROMPARTS(@ano, 10, 27), DATEFROMPARTS(@ano, 12, 4));

    INSERT INTO dbo.Avaliacao (TurmaId, DisciplinaId, SubDisciplinaId, Nome, Peso, DataAplicacao, Bimestre, Ativo)
    SELECT @turma, x.DisciplinaId, x.SubDisciplinaId, x.Nome, x.Peso,
           CASE x.Tipo WHEN 'P' THEN d.Prova ELSE d.Trabalho END, d.Bimestre, 1
    FROM @datas d
    CROSS JOIN (VALUES
            (@mat, CAST(NULL AS int), N'Prova bimestral', 60.00, 'P'),
            (@mat, NULL, N'Lista de exercícios', 40.00, 'T'),
            (@cie, NULL, N'Prova bimestral', 60.00, 'P'),
            (@cie, NULL, N'Relatório de laboratório', 40.00, 'T'),
            (@his, NULL, N'Prova bimestral', 50.00, 'P'),
            (@his, NULL, N'Seminário', 50.00, 'T'),
            (@por, @gra, N'Prova de gramática', 50.00, 'P'),
            (@por, @lit, N'Análise literária', 50.00, 'T')
         ) x (DisciplinaId, SubDisciplinaId, Nome, Peso, Tipo);

    /* ---------- Notas (so de avaliacoes que ja aconteceram) ---------- */
    -- Variacao deterministica de -1,0 a +1,0 em torno da media do aluno; arredonda para 0,5.
    INSERT INTO dbo.Nota (AvaliacaoId, AlunoId, Valor)
    SELECT av.Id, a.AlunoId,
           CAST(CASE
                    WHEN v.Valor > 10 THEN 10
                    WHEN v.Valor < 0 THEN 0
                    ELSE v.Valor
                END AS decimal(4,2))
    FROM dbo.Avaliacao av
    CROSS JOIN @alunos a
    CROSS APPLY (SELECT ROUND((a.Base + ((av.Id * 7 + a.AlunoId * 13) % 5 - 2) * 0.5) * 2, 0) / 2 AS Valor) v
    WHERE av.TurmaId = @turma
      AND av.DataAplicacao <= @hoje;

    /* ---------- Chamadas (dias letivos ate hoje, sem julho) ---------- */
    -- Dia da semana sem depender de DATEFIRST: 0 = segunda (01/01/1900 foi segunda).
    -- Matematica e Portugues 2x por semana; Ciencias e Historia 1x.
    ;WITH Numeros AS (
        SELECT TOP (366) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS N
        FROM sys.all_objects
    ),
    Dias AS (
        SELECT DATEADD(day, N, DATEFROMPARTS(@ano, 2, 2)) AS Data
        FROM Numeros
    ),
    Aulas AS (
        SELECT d.Data, h.DisciplinaId
        FROM Dias d
        JOIN (VALUES (0, @mat), (2, @mat), (1, @por), (3, @por), (2, @cie), (4, @his)) h (DiaSemana, DisciplinaId)
          ON DATEDIFF(day, '19000101', d.Data) % 7 = h.DiaSemana
        WHERE d.Data <= @hoje
          AND d.Data <= DATEFROMPARTS(@ano, 12, 15)
          AND MONTH(d.Data) <> 7
    )
    INSERT INTO dbo.Presenca (TurmaId, DisciplinaId, AlunoId, Data, Presente)
    SELECT @turma, au.DisciplinaId, a.AlunoId, au.Data,
           CASE WHEN (a.AlunoId * 31 + DATEDIFF(day, DATEFROMPARTS(@ano, 1, 1), au.Data) * 17 + au.DisciplinaId * 7) % 100 < a.Faltas
                THEN 0 ELSE 1 END
    FROM Aulas au
    CROSS JOIN @alunos a;

    INSERT INTO dbo.HistoricoScripts (Script) VALUES ('002_dados_demonstracao');

    COMMIT TRANSACTION;
    PRINT '002_dados_demonstracao aplicado: turma 9o Ano A (2026) com professores, alunos, avaliacoes, notas e chamadas. Senha dos usuarios de demonstracao: Demo@2026';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Erro: nada foi alterado.';
    THROW;
END CATCH;
