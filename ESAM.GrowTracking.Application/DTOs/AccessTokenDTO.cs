namespace ESAM.GrowTracking.Application.DTOs
{
    public sealed record AccessTokenDTO(string Token, int ExpiresIn, DateTime ExpiresAt);
}