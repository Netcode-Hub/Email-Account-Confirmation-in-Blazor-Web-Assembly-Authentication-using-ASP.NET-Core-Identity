using JWTDemo.Server.Models;

namespace JWTDemo.Server.Service
{
    public interface IEmailService
    {
        Task<string> SendEmail(RequestDTO request);
    }
}
