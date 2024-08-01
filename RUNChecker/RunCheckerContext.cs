using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RUNChecker.Models;
using RUNChecker.Options;

namespace RUNChecker;

public partial class RunCheckerContext : DbContext
{
    private readonly ConnectionStringsOptions _connectionStringsOptions;

    public RunCheckerContext(IOptions<ConnectionStringsOptions> connectionStringsOptions)
    {
        _connectionStringsOptions = connectionStringsOptions.Value;
    }

    public virtual DbSet<AppEnvironment> AppEnvironments { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<ServiceAccount> ServiceAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(_connectionStringsOptions.RunCheckerContext);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppEnvironment>(entity =>
        {
            entity.HasKey(e => e.AppEnvironmentId).HasName("PK_Environments");

            entity.ToTable("AppEnvironments", "RunChecker");

            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Applications", "RunChecker");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Areas", "RunChecker");

            entity.Property(e => e.AreaId).ValueGeneratedOnAdd();
            entity.Property(e => e.AreaName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BacklogTeam)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("RUN Team Test");
            entity.Property(e => e.IterationPath)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProjectName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.ToTable("Certificates", "RunChecker");

            entity.Property(e => e.CurrentProtocol)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CurrentSans)
                .IsUnicode(false)
                .HasColumnName("CurrentSANs");
            entity.Property(e => e.CurrentSubject)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CurrentThumbprint)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.HostName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.AppEnvironment).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.AppEnvironmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_AppEnvironments");

            entity.HasOne(d => d.Application).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_Applications");
        });

        modelBuilder.Entity<ServiceAccount>(entity =>
        {
            entity.ToTable("ServiceAccounts", "RunChecker");

            entity.Property(e => e.AccountName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CyberArkSafe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EmployeeType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.AppEnvironment).WithMany(p => p.ServiceAccounts)
                .HasForeignKey(d => d.AppEnvironmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceAccounts_AppEnvironments");

            entity.HasOne(d => d.Application).WithMany(p => p.ServiceAccounts)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceAccounts_Applications");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
