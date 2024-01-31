using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using static Azure.Core.HttpHeader;

namespace IdentityApp.TagHelpers
{
	[HtmlTargetElement("td", Attributes = "asp-role-users")]
	public class RoleUsersTagHelper : TagHelper
	{
		private readonly RoleManager<AppRole> _roleManager;
		private readonly UserManager<AppUser> _userManager;

		public RoleUsersTagHelper(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
		{
			_roleManager = roleManager;
			_userManager = userManager;
		}
		[HtmlAttributeName("asp-role-users")]
		public string RoleId { get; set; } = null!;

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var role = await _roleManager.FindByIdAsync(RoleId);
			if (role != null)
			{
				var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
				var userNames = usersInRole.Select(u => u.UserName).ToList();

				output.Content.SetHtmlContent(userNames.Count == 0 ? "Kullanıcı yok" : setHtml(userNames));
			}
		}

		private string setHtml(List<string?> userNames)
		{
			var html = "<ul>";
			foreach (var user in userNames)
			{
				html += "<li>" + user + "</li>";
			}
			html += "<ul>";
			return html;
		}
	}
}
