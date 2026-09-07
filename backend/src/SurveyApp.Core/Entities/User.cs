namespace SurveyApp.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsSuperAdmin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
}

public enum UserRole
{
    Admin,
    User
}