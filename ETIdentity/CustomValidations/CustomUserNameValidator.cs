using ETIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETIdentity.CustomValidations
{
    public class CustomUserNameValidator : IUserValidator<AppUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            List<IdentityError> errors = new List<IdentityError>();

            string[] Digits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            foreach (string digit in Digits)
            {
                if (user.UserName.StartsWith(digit))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "UserNameContainsFirstLetterDigit",
                        Description = "Kullanıcı adının ilk karakteri sayısal karakter içeremez."
                    });
                }
            }

            if (errors.Count == 0)
            {
                return await Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return await Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }
        }
    }
}
