using System.ComponentModel.DataAnnotations;

namespace ETIdentity.ViewModels
{
    public class PasswordResetViewModel
    {
        [Required(ErrorMessage = "E-posta Adresi gereklidir.")]
        [Display(Name = "E-posta Adresi")]
        [EmailAddress(ErrorMessage = "E-posta Adresiniz doğru formatta değil.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakter olmalıdır.")]
        public string Password { get; set; }
    }
}
