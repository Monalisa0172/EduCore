using EduCore.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Data;

/// <summary>
/// Banco database-first: o esquema é mantido pelos scripts em database/scripts.
/// Este mapeamento deve refletir o que os scripts criam (nomes, índices únicos e filtros).
/// </summary>
public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aluno> Alunos { get; set; }

    public virtual DbSet<Atribuicao> Atribuicoes { get; set; }

    public virtual DbSet<Avaliacao> Avaliacoes { get; set; }

    public virtual DbSet<Contato> Contatos { get; set; }

    public virtual DbSet<Disciplina> Disciplinas { get; set; }

    public virtual DbSet<Endereco> Enderecos { get; set; }

    public virtual DbSet<Funcionario> Funcionarios { get; set; }

    public virtual DbSet<Matricula> Matriculas { get; set; }

    public virtual DbSet<Nota> Notas { get; set; }

    public virtual DbSet<Presenca> Presencas { get; set; }

    public virtual DbSet<Professor> Professors { get; set; }

    public virtual DbSet<RedefinicaoSenha> RedefinicoesSenha { get; set; }

    public virtual DbSet<SubDisciplina> SubDisciplinas { get; set; }

    public virtual DbSet<Turma> Turmas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RedefinicaoSenha>(entity =>
        {
            entity.Property(e => e.TokenHash).IsFixedLength();
            entity.HasIndex(e => e.TokenHash, "UX_RedefinicaoSenha_TokenHash").IsUnique();
            entity.HasIndex(e => new { e.UsuarioId, e.CriadoEm }, "IX_RedefinicaoSenha_Usuario");
        });

        modelBuilder.Entity<Aluno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aluno__3214EC0745EFD30B");

            entity.HasIndex(e => e.UsuarioId, "UX_Aluno_Usuario")
                .IsUnique()
                .HasFilter("[UsuarioId] IS NOT NULL");

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Alunos)
                .HasConstraintName("FK__Aluno__UsuarioId__3C69FB99");
        });

        modelBuilder.Entity<Atribuicao>(entity =>
        {
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            // Uma atribuicao ativa por turma + disciplina + subdisciplina (NULL = disciplina inteira).
            entity.HasIndex(e => new { e.TurmaId, e.DisciplinaId, e.SubDisciplinaId }, "UX_Atribuicao_Ativa")
                .IsUnique()
                .HasFilter("[Ativo] = 1");

            entity.HasIndex(e => new { e.ProfessorId, e.Ativo }, "IX_Atribuicao_Professor");
        });

        modelBuilder.Entity<Avaliacao>(entity =>
        {
            entity.HasIndex(e => new { e.TurmaId, e.DisciplinaId, e.Bimestre }, "IX_Avaliacao_Turma_Disciplina_Bimestre");
        });

        modelBuilder.Entity<Contato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contato__3214EC07F006A48F");

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Contatos)
                .HasConstraintName("FK__Contato__Usuario__4222D4EF");
        });

        modelBuilder.Entity<Disciplina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Discipli__3214EC0782D49091");
        });

        modelBuilder.Entity<Endereco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Endereco__3214EC07BF7CF971");

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Enderecos)
                .HasConstraintName("FK__Endereco__Usuari__3F466844");
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Funciona__3214EC07C131EA4E");

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Funcionarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Funcionar__Usuar__5812160E");
        });

        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Matricul__3214EC07702D739C");

            entity.HasIndex(e => new { e.TurmaId, e.AlunoId }, "UX_Matricula_Turma_Aluno").IsUnique();
        });

        modelBuilder.Entity<Nota>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nota__3214EC07A7A801C1");

            entity.HasIndex(e => new { e.AvaliacaoId, e.AlunoId }, "UX_Nota_Avaliacao_Aluno").IsUnique();
        });

        modelBuilder.Entity<Presenca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Presenca__3214EC074F832E32");

            entity.HasIndex(e => new { e.TurmaId, e.DisciplinaId, e.Data, e.AlunoId }, "UX_Presenca_Aula_Aluno").IsUnique();
        });

        modelBuilder.Entity<Professor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Professo__3214EC07B5375209");

            entity.HasIndex(e => e.FuncionarioId, "UX_Professor_Funcionario").IsUnique();

            entity.HasOne(d => d.Funcionario)
                .WithOne(p => p.Professor)
                .HasForeignKey<Professor>(d => d.FuncionarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Professor__Funci__5AEE82B9");
        });

        modelBuilder.Entity<SubDisciplina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SubDisci__3214EC0758CDCEA2");

            entity.HasOne(d => d.Disciplina)
                .WithMany(p => p.SubDisciplinas)
                .HasConstraintName("FK__SubDiscip__Disci__5535A963");
        });

        modelBuilder.Entity<Turma>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Turma__3214EC07BB0775FB");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC077A86FEDD");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
