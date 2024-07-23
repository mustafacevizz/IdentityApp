using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models
{
    public static class IdentitySeedData
    {
        private const string adminUser = "admin";
        private const string adminPassword = "Admin_123";
        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
            if (context.Database.GetAppliedMigrations().Any())
            {
                context.Database.Migrate();
            }
            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.FindByNameAsync(adminUser);
            if (user == null)
            {
                // If a user with the username "Admin" is not found, create a new IdentityUser object.
                user = new AppUser
                {
                    FullName="Mustafa Ceviz",
                    UserName = adminUser,
                    Email = "admin@mcvz.com",
                    PhoneNumber = "123456789"
                };
                await userManager.CreateAsync(user, adminPassword);// CreateAsync method securely hashes and stores the password in the database.
                                                                   // The password hashing and storage are handled by the Identity library to ensure security.

            }
        }
    }
}
