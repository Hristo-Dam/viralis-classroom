using Microsoft.AspNetCore.Identity;
using Viralis.Data.Models;

namespace Viralis.Web.Validators
{
    public class NoSpacesPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            if (password != null && password.Contains(' '))
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordContainsSpaces",
                    Description = "Passwords must not contain spaces."
                }));

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
