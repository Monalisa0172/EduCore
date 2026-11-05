/*
    EduCore - 001_modelo_academico.sql
    Fase 2: modelo academico (atribuicao, avaliacao por turma, nota por avaliacao, chamada por turma).

    O que faz:
      1. Copia os dados de teste de Nota, Presenca, Avaliacao, ProfessorTurma e ProfessorSubDisciplina
         para o schema [bkp] (nada e perdido) e limpa essas tabelas. Esses registros nao tem turma
         nem avaliacao, entao nao ha como migra-los com seguranca.
      2. Cria a tabela Atribuicao (professor x turma x disciplina, subdisciplina opcional), que
         substitui ProfessorTurma e ProfessorSubDisciplina.
      3. Avaliacao ganha TurmaId; Nota passa a apontar para a avaliacao (AvaliacaoId) em vez da
         disciplina; Presenca ganha TurmaId.
      4. Regras garantidas pelo banco: matricula sem duplicidade, nota de 0 a 10, peso de 0 a 100,
         bimestre de 1 a 4, subdisciplina sempre da mesma disciplina, uma atribuicao ativa por
         turma+disciplina+subdisciplina, indices nas chaves estrangeiras.

    Como rodar: abra no SSMS conectado ao banco do EduCore (ex.: EscolaDB) e execute (F5).
    Seguro para rodar de novo: se ja foi aplicado, nao faz nada. Tudo roda numa transacao:
    se algo falhar, nada e alterado.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.HistoricoScripts', N'U') IS NULL
    CREATE TABLE dbo.HistoricoScripts
    (
        Script     varchar(100) NOT NULL CONSTRAINT PK_HistoricoScripts PRIMARY KEY,
        AplicadoEm datetime2(0) NOT NULL CONSTRAINT DF_HistoricoScripts_AplicadoEm DEFAULT SYSDATETIME()
    );

IF EXISTS (SELECT 1 FROM dbo.HistoricoScripts WHERE Script = '001_modelo_academico')
BEGIN
    PRINT '001_modelo_academico ja foi aplicado neste banco. Nada a fazer.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @sql nvarchar(max);

    /* ---------- 1. Backup dos dados de teste ---------- */
    IF SCHEMA_ID(N'bkp') IS NULL EXEC (N'CREATE SCHEMA bkp AUTHORIZATION dbo;');

    EXEC (N'SELECT * INTO bkp.Nota_001 FROM dbo.Nota;');
    EXEC (N'SELECT * INTO bkp.Presenca_001 FROM dbo.Presenca;');
    EXEC (N'SELECT * INTO bkp.Avaliacao_001 FROM dbo.Avaliacao;');
    EXEC (N'SELECT * INTO bkp.ProfessorTurma_001 FROM dbo.ProfessorTurma;');
    EXEC (N'SELECT * INTO bkp.ProfessorSubDisciplina_001 FROM dbo.ProfessorSubDisciplina;');

    DELETE FROM dbo.Nota;
    DELETE FROM dbo.Presenca;
    DELETE FROM dbo.Avaliacao;

    DROP TABLE dbo.ProfessorTurma;
    DROP TABLE dbo.ProfessorSubDisciplina;

    /* FKs antigas com nome gerado automaticamente (muda de maquina para maquina):
       removidas pelo catalogo e recriadas com nome fixo. */
    SET @sql = N'';
    SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.'
                 + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
    FROM sys.foreign_keys fk
    WHERE fk.parent_object_id IN (OBJECT_ID(N'dbo.Nota'), OBJECT_ID(N'dbo.Presenca'), OBJECT_ID(N'dbo.Matricula'));
    EXEC sys.sp_executesql @sql;

    /* ---------- 2. SubDisciplina: chave (Id, DisciplinaId) para FKs compostas ---------- */
    EXEC (N'ALTER TABLE dbo.SubDisciplina ADD CONSTRAINT UQ_SubDisciplina_Id_Disciplina UNIQUE (Id, DisciplinaId);');

    /* ---------- 3. Matricula: sem nulos e sem duplicidade ---------- */
    EXEC (N'ALTER TABLE dbo.Matricula ALTER COLUMN AlunoId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Matricula ALTER COLUMN TurmaId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Matricula ADD
            CONSTRAINT FK_Matricula_Aluno FOREIGN KEY (AlunoId) REFERENCES dbo.Aluno (Id),
            CONSTRAINT FK_Matricula_Turma FOREIGN KEY (TurmaId) REFERENCES dbo.Turma (Id);');
    EXEC (N'CREATE UNIQUE INDEX UX_Matricula_Turma_Aluno ON dbo.Matricula (TurmaId, AlunoId);');
    EXEC (N'CREATE INDEX IX_Matricula_Aluno ON dbo.Matricula (AlunoId);');

    /* ---------- 4. Perfis: um aluno por usuario, um professor por funcionario ---------- */
    EXEC (N'CREATE UNIQUE INDEX UX_Aluno_Usuario ON dbo.Aluno (UsuarioId) WHERE UsuarioId IS NOT NULL;');
    EXEC (N'CREATE UNIQUE INDEX UX_Professor_Funcionario ON dbo.Professor (FuncionarioId);');

    /* ---------- 5. Atribuicao ---------- */
    EXEC (N'CREATE TABLE dbo.Atribuicao
            (
                Id              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Atribuicao PRIMARY KEY,
                ProfessorId     int NOT NULL,
                TurmaId         int NOT NULL,
                DisciplinaId    int NOT NULL,
                SubDisciplinaId int NULL,
                Ativo           bit NOT NULL CONSTRAINT DF_Atribuicao_Ativo DEFAULT (1),
                CONSTRAINT FK_Atribuicao_Professor FOREIGN KEY (ProfessorId) REFERENCES dbo.Professor (Id),
                CONSTRAINT FK_Atribuicao_Turma FOREIGN KEY (TurmaId) REFERENCES dbo.Turma (Id),
                CONSTRAINT FK_Atribuicao_Disciplina FOREIGN KEY (DisciplinaId) REFERENCES dbo.Disciplina (Id),
                CONSTRAINT FK_Atribuicao_SubDisciplina FOREIGN KEY (SubDisciplinaId, DisciplinaId)
                    REFERENCES dbo.SubDisciplina (Id, DisciplinaId)
            );');
    /* Uma atribuicao ativa por turma+disciplina+subdisciplina (NULL = disciplina inteira). */
    EXEC (N'CREATE UNIQUE INDEX UX_Atribuicao_Ativa ON dbo.Atribuicao (TurmaId, DisciplinaId, SubDisciplinaId) WHERE Ativo = 1;');
    EXEC (N'CREATE INDEX IX_Atribuicao_Professor ON dbo.Atribuicao (ProfessorId, Ativo) INCLUDE (TurmaId, DisciplinaId, SubDisciplinaId);');

    /* ---------- 6. Avaliacao: pertence a uma turma ---------- */
    EXEC (N'ALTER TABLE dbo.Avaliacao ADD TurmaId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Avaliacao ADD
            CONSTRAINT FK_Avaliacao_Turma FOREIGN KEY (TurmaId) REFERENCES dbo.Turma (Id),
            CONSTRAINT FK_Avaliacao_SubDisciplina FOREIGN KEY (SubDisciplinaId, DisciplinaId)
                REFERENCES dbo.SubDisciplina (Id, DisciplinaId),
            CONSTRAINT CK_Avaliacao_Peso CHECK (Peso > 0 AND Peso <= 100),
            CONSTRAINT CK_Avaliacao_Bimestre CHECK (Bimestre BETWEEN 1 AND 4);');
    EXEC (N'CREATE INDEX IX_Avaliacao_Turma_Disciplina_Bimestre ON dbo.Avaliacao (TurmaId, DisciplinaId, Bimestre) INCLUDE (Peso, Ativo);');

    /* ---------- 7. Nota: uma por aluno por avaliacao ---------- */
    EXEC (N'ALTER TABLE dbo.Nota DROP COLUMN DisciplinaId;');
    EXEC (N'ALTER TABLE dbo.Nota ALTER COLUMN AlunoId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Nota ALTER COLUMN Valor decimal(4,2) NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Nota ADD AvaliacaoId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Nota ADD
            CONSTRAINT FK_Nota_Aluno FOREIGN KEY (AlunoId) REFERENCES dbo.Aluno (Id),
            CONSTRAINT FK_Nota_Avaliacao FOREIGN KEY (AvaliacaoId) REFERENCES dbo.Avaliacao (Id),
            CONSTRAINT CK_Nota_Valor CHECK (Valor BETWEEN 0 AND 10);');
    EXEC (N'CREATE UNIQUE INDEX UX_Nota_Avaliacao_Aluno ON dbo.Nota (AvaliacaoId, AlunoId);');
    EXEC (N'CREATE INDEX IX_Nota_Aluno ON dbo.Nota (AlunoId);');

    /* ---------- 8. Presenca: chamada por turma, um registro por aluno por aula ---------- */
    EXEC (N'ALTER TABLE dbo.Presenca ALTER COLUMN AlunoId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Presenca ALTER COLUMN DisciplinaId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Presenca ALTER COLUMN Data date NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Presenca ALTER COLUMN Presente bit NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Presenca ADD TurmaId int NOT NULL;');
    EXEC (N'ALTER TABLE dbo.Presenca ADD
            CONSTRAINT FK_Presenca_Aluno FOREIGN KEY (AlunoId) REFERENCES dbo.Aluno (Id),
            CONSTRAINT FK_Presenca_Disciplina FOREIGN KEY (DisciplinaId) REFERENCES dbo.Disciplina (Id),
            CONSTRAINT FK_Presenca_Turma FOREIGN KEY (TurmaId) REFERENCES dbo.Turma (Id);');
    EXEC (N'CREATE UNIQUE INDEX UX_Presenca_Aula_Aluno ON dbo.Presenca (TurmaId, DisciplinaId, Data, AlunoId);');
    EXEC (N'CREATE INDEX IX_Presenca_Aluno ON dbo.Presenca (AlunoId);');

    /* ---------- 9. Demais chaves estrangeiras sem indice ---------- */
    EXEC (N'CREATE INDEX IX_SubDisciplina_Disciplina ON dbo.SubDisciplina (DisciplinaId);');
    EXEC (N'CREATE INDEX IX_Funcionario_Usuario ON dbo.Funcionario (UsuarioId);');

    INSERT INTO dbo.HistoricoScripts (Script) VALUES ('001_modelo_academico');

    COMMIT TRANSACTION;
    PRINT '001_modelo_academico aplicado com sucesso. Backup dos dados de teste no schema [bkp].';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Erro: nada foi alterado.';
    THROW;
END CATCH;
