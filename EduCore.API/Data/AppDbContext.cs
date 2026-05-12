using System;
using System.Collections.Generic;
using EduCore.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Data;

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

    public virtual DbSet<Contato> Contatos { get; set; }

    public virtual DbSet<Disciplina> Disciplinas { get; set; }

    public virtual DbSet<Endereco> Enderecos { get; set; }

    public virtual DbSet<Funcionario> Funcionarios { get; set; }

    public virtual DbSet<Matricula> Matriculas { get; set; }

    public virtual DbSet<Notum> Nota { get; set; }

    public virtual DbSet<Presenca> Presencas { get; set; }

    public virtual DbSet<Professor> Professors { get; set; }

    public virtual DbSet<SubDisciplina> SubDisciplinas { get; set; }

    public virtual DbSet<Turma> Turmas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=EscolaDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aluno__3214EC0745EFD30B");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Alunos).HasConstraintName("FK__Aluno__UsuarioId__3C69FB99");
        });

        modelBuilder.Entity<Contato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contato__3214EC07F006A48F");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Contatos).HasConstraintName("FK__Contato__Usuario__4222D4EF");
        });

        modelBuilder.Entity<Disciplina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Discipli__3214EC0782D49091");
        });

        modelBuilder.Entity<Endereco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Endereco__3214EC07BF7CF971");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Enderecos).HasConstraintName("FK__Endereco__Usuari__3F466844");
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Funciona__3214EC07C131EA4E");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Funcionarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Funcionar__Usuar__5812160E");
        });

        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Matricul__3214EC07702D739C");

            entity.HasOne(d => d.Aluno).WithMany(p => p.Matriculas).HasConstraintName("FK__Matricula__Aluno__49C3F6B7");

            entity.HasOne(d => d.Turma).WithMany(p => p.Matriculas).HasConstraintName("FK__Matricula__Turma__4AB81AF0");
        });

        modelBuilder.Entity<Notum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nota__3214EC07A7A801C1");

            entity.HasOne(d => d.Aluno).WithMany(p => p.Nota).HasConstraintName("FK__Nota__AlunoId__4D94879B");

            entity.HasOne(d => d.Disciplina).WithMany(p => p.Nota).HasConstraintName("FK__Nota__Disciplina__4E88ABD4");
        });

        modelBuilder.Entity<Presenca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Presenca__3214EC074F832E32");

            entity.HasOne(d => d.Aluno).WithMany(p => p.Presencas).HasConstraintName("FK__Presenca__AlunoI__5165187F");

            entity.HasOne(d => d.Disciplina).WithMany(p => p.Presencas).HasConstraintName("FK__Presenca__Discip__52593CB8");
        });

        modelBuilder.Entity<Professor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Professo__3214EC07B5375209");

            entity.HasOne(d => d.Funcionario).WithMany(p => p.Professors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Professor__Funci__5AEE82B9");
        });

        modelBuilder.Entity<SubDisciplina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SubDisci__3214EC0758CDCEA2");

            entity.HasOne(d => d.Disciplina).WithMany(p => p.SubDisciplinas).HasConstraintName("FK__SubDiscip__Disci__5535A963");
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
