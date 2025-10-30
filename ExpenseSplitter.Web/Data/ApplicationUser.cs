using System;
using Microsoft.AspNetCore.Identity;

namespace ExpenseSplitter.Web.Data
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
