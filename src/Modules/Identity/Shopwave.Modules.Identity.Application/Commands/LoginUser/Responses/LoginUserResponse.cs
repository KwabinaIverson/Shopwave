namespace Shopwave.Modules.Identity.Application.Commands.LoginUser.Responses;

public record LoginUserResponse(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Token
);