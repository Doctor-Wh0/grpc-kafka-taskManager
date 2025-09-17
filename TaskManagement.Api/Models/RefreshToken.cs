using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskManagement.Api.Models;
public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;  // UUID или JWT string
        public string JwtId { get; set; } = string.Empty;  // ID из access token (jti claim)
        public bool IsUsed { get; set; } = false;  // Чтобы предотвратить reuse
        public bool IsRevoked { get; set; } = false;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }  // e.g., +7 дней
        public string ReplacedByToken { get; set; } = string.Empty;  // Для ротации
        public string UserId { get; set; } = string.Empty;  // IdentityUser.Id

        [MaxLength(255)]
        public string Device { get; set; } = string.Empty;  // "Browser" или "Mobile"
        [MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;  // Для дополнительной проверки
    }