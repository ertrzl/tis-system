namespace Tis.Application.DTOs.Users;

public class GetUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Parol hələ təyin olunmayıb: dəvət gözlənilir.
    public bool InvitationPending { get; set; }
    public DateTime CreatedAt { get; set; }
}
