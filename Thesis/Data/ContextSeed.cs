using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Thesis.Data
{
    public class ContextSeed
    {
        public enum Roles
        {
            Admin,
            Expert,
            Basic
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // seed roles
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Expert.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Basic.ToString()));
        }
    }
}
