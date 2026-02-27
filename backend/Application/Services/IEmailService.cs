using Backend.Application.Messaging.Emails;
using Backend.Application.Services;

namespace Backend.Application.Services;
public interface IEmailService
{
    Task Send<T>(T Email) where T: EmailBase;
}
