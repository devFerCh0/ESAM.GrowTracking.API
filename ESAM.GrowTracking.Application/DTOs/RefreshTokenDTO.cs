namespace ESAM.GrowTracking.Application.DTOs
{
    public sealed record RefreshTokenDTO(string Identifier, string Token, int ExpiresIn, DateTime ExpiresAt);
}