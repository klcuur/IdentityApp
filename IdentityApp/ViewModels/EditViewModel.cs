using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels
{
	public class EditViewModel
	{

		public string? Id { get; set; }
		public string UserName { get; set; }


		public string FullName { get; set; }


		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string? Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Parola eslesmiyor.")]
		public string? ConfirmPassword { get; set; }

		public IList<string>? SelectedRoles { get; set; }
	}
}
