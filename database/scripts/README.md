# Scripts do banco

O projeto é **database-first**: o esquema do SQL Server é mantido por estes scripts (não há migrations do EF Core). O `AppDbContext` reflete o que eles criam.

## Como aplicar

1. Abra o script no SSMS conectado ao banco do EduCore (ex.: `EscolaDB`).
2. Execute (F5). Aplique os scripts **em ordem numérica**.

Cada script:

- roda numa **transação**: se algo falhar, nada é alterado;
- registra a execução em `dbo.HistoricoScripts`. Rodar de novo não faz nada;
- quando precisa descartar dados, copia-os antes para o schema `bkp`.

Para ver o que já foi aplicado:

```sql
SELECT * FROM dbo.HistoricoScripts ORDER BY Script;
```

## Scripts

| Script | O que faz |
|---|---|
| `001_modelo_academico.sql` | Cria a tabela `Atribuicao`, que substitui `ProfessorTurma` e `ProfessorSubDisciplina`. Avaliação passa a ter turma, nota passa a ser por avaliação e presença passa a ter turma. O banco passa a impedir matrícula duplicada, nota fora de 0–10, peso fora de 0–100 e subdisciplina de outra disciplina. Cria os índices nas chaves estrangeiras. As notas, presenças e avaliações antigas não tinham turma nem avaliação; elas são copiadas para `bkp.*_001` e removidas. |

| `002_dados_demonstracao.sql` | Dados de demonstração: turma **9º Ano A (2026)** com 5 professores, 8 alunos, atribuições (Português dividida em Gramática e Literatura), avaliações dos 4 bimestres com pesos somando 100, notas e chamadas. Reaproveita as disciplinas que já existem pelo nome. Só gera notas de avaliações já aplicadas e chamadas até a data em que o script roda. |

## Acesso de demonstração

Todos os usuários criados pelo `002` usam a senha **`Demo@2026`**:

| Perfil | E-mail | O que mostra |
|---|---|---|
| Professora | `renata.alves@demo.educore.com.br` | Matemática no 9º Ano A |
| Professor | `paulo.mendes@demo.educore.com.br` | Só a parte de Gramática de Português |
| Aluna | `sofia.ribeiro@demo.educore.com.br` | Desempenho alto (verde) |
| Aluno | `rafael.oliveira@demo.educore.com.br` | Perto do limite (atenção) |
| Aluno | `enzo.carvalho@demo.educore.com.br` | Média abaixo de 6 (risco) |
| Aluna | `valentina.gomes@demo.educore.com.br` | Frequência abaixo de 75% (risco) |

## Regras do modelo acadêmico

- **Atribuição:** cada disciplina de uma turma é atribuída **inteira** a um professor **ou dividida por subdisciplinas** entre professores, nunca as duas formas ao mesmo tempo. Fica no máximo uma atribuição ativa por turma + disciplina + subdisciplina.
- **Avaliação:** pertence a uma turma, disciplina e bimestre e só pode ser criada se a disciplina tiver professor na turma. Se a disciplina estiver dividida, a avaliação indica a subdisciplina. Os pesos das avaliações ativas de turma + disciplina + bimestre somam no máximo 100, e a média do bimestre fecha quando a soma chega a 100.
- **Nota:** uma por aluno por avaliação, de 0 a 10.
- **Presença:** uma por aluno, turma, disciplina e dia.
