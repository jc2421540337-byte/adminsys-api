using NewAdminSystem.Api.DTOs.Auth;
using FluentValidation;

namespace NewAdminSystem.Api.Validators.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator() 
        {
            RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
        }
    }
}
