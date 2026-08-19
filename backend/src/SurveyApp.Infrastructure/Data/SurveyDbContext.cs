using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;

namespace SurveyApp.Infrastructure.Data;

public class SurveyDbContext : DbContext
{
    public SurveyDbContext(DbContextOptions<SurveyDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AnswerTemplate> AnswerTemplates => Set<AnswerTemplate>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyQuestion> SurveyQuestions => Set<SurveyQuestion>();
    public DbSet<SurveyAssignment> SurveyAssignments => Set<SurveyAssignment>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<SurveyAssignment>()
            .HasIndex(sa => new { sa.SurveyId, sa.UserId })
            .IsUnique();
        modelBuilder.Entity<SurveyResponse>()
            .HasIndex(sr => new { sr.SurveyId, sr.UserId, sr.QuestionId })
            .IsUnique();
    }
}