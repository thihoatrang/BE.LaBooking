using Users.Infrastructure.Models;

namespace Users.Infrastructure.Models.Saga
{
    public enum UserSagaState
    {
        Started,
        UserCreated,
        EmailSent,
        ProfileCreated,
        Completed,
        Failed,
        Compensating
    }

    public class UserSagaData
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public UserSagaState State { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsLawyer { get; set; }
        public int? LawyerProfileId { get; set; }
    }
}
