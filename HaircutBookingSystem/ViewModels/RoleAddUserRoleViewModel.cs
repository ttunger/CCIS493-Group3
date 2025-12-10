using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaircutBookingSystem.ViewModels
{
    public class RoleAddUserRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public string Role { get; set; }
        public SelectList RoleList { get; set; }

    }
}
