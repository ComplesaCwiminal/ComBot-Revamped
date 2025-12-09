using Microsoft.AspNetCore.Identity;

namespace ComBot_Revamped.Data
{
    // This enum is a broad outline. You should use more fine grained control for each thing contained within
    [Flags]
    public enum UserPermissions
    {
        ViewUsers = 1, // At it's most basic allows one to see ALL users and inspect their properties, but NOT any modification
        ModifyUsers = 2, // At it's most basic allows modification; Including creation, or deletion; of other users at any time
        ViewCommands = 4, // At it's most basic allows the viewing of ANY COMMAND.
        RunCommands = 8, // At it's most basic allows a user to run ANY COMMAND.
    }

    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public UserPermissions permissions;
    }

}
