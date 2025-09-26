
namespace AuthService.Model.Entities
{
    public class User
    {
            public int Id { get; set; }
            public required string Username { get; set; } = null!;
            public required string Email { get; set; } = null!;
            public required string Password { get; set; } = null!;
            public string Role { get; set; } = "User"; // "User" hoặc "Admin"

            public DateTime CreatedAt { get; set; }

    }
}
