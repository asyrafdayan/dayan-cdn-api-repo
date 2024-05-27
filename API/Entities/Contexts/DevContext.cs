using System;
using System.Collections.Generic;
using API.Entities.Tables;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Entities.Contexts;

public partial class DevContext : DbContext
{
    private readonly ILogger<DevContext> _logger;
    private readonly AppSettingsModel _appSettings;

    public DevContext(ILogger<DevContext> logger, IOptions<AppSettingsModel> appSettings)
    {
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public DevContext(DbContextOptions<DevContext> options, ILogger<DevContext> logger, IOptions<AppSettingsModel> appSettings)
        : base(options)
    {
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public virtual DbSet<TblFreelancerMst> TblFreelancerMsts { get; set; }

    public virtual DbSet<TblSkill> TblSkills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(_appSettings?.ConnectionStrings?.SQLite);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblFreelancerMst>(entity =>
        {
            entity.ToTable("tblFreelancerMst");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Email)
                .HasColumnType("TEXT(255)")
                .HasColumnName("email");
            entity.Property(e => e.Hobby).HasColumnName("hobby");
            entity.Property(e => e.Phonenumber).HasColumnName("phonenumber");
            entity.Property(e => e.Username)
                .HasColumnType("TEXT(255)")
                .HasColumnName("username");
        });

        modelBuilder.Entity<TblSkill>(entity =>
        {
            entity.ToTable("tblSkills");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FreelancerId).HasColumnName("freelancer_id");
            entity.Property(e => e.Skill).HasColumnName("skill");

            entity.HasOne(d => d.Freelancer).WithMany(p => p.TblSkills)
                .HasForeignKey(d => d.FreelancerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
