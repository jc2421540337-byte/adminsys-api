using NewAdminSystem.Api.DTOs.Users;
using FluentValidation;

namespace NewAdminSystem.Api.Validators.Users
{
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator() 
        {
            RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
            .NotEmpty()
            .Length(5, 21);
        }
    }
}
