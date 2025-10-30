using System;
using System.Linq;
using System.Threading.Tasks;
using ExpenseSplitter.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> users)
        {
            await db.Database.EnsureCreatedAsync();

            // Users
            var alice = await EnsureUser(users, "alice@example.com", "Alice#1234");
            var bob = await EnsureUser(users, "bob@example.com", "Bob#1234");
            var charlie = await EnsureUser(users, "charlie@example.com", "Charlie#1234");

            if (!await db.Trips.AnyAsync())
            {
                var trip = new Trip
                {
                    TripName = "Goa Weekend",
                    Description = "Friends trip to Goa",
                    TotalBudget = 2000,
                    StartDate = DateTime.UtcNow.Date.AddDays(-2),
                    EndDate = DateTime.UtcNow.Date.AddDays(1),
                    CreatedBy = alice.Id,
                    IsActive = true
                };
                db.Trips.Add(trip);
                await db.SaveChangesAsync();

                var mAlice = new TripMember { TripId = trip.TripId, UserId = alice.Id, PersonalBudget = 1000, CurrentBalance = 0 };
                var mBob = new TripMember { TripId = trip.TripId, UserId = bob.Id, PersonalBudget = 700, CurrentBalance = 0 };
                var mCharlie = new TripMember { TripId = trip.TripId, UserId = charlie.Id, PersonalBudget = 500, CurrentBalance = 0 };
                db.TripMembers.AddRange(mAlice, mBob, mCharlie);
                await db.SaveChangesAsync();

                var dinner = new Expense
                {
                    TripId = trip.TripId,
                    Title = "Dinner at Olive Garden",
                    TotalAmount = 1000,
                    PaidBy = bob.Id,
                    ExpenseDate = DateTime.UtcNow.Date.AddDays(-1),
                    Category = ExpenseCategory.Food,
                    Description = "Seafood dinner",
                    IsSettled = false
                };
                db.Expenses.Add(dinner);
                await db.SaveChangesAsync();

                db.ExpenseSplits.AddRange(
                    new ExpenseSplit { ExpenseId = dinner.ExpenseId, UserId = alice.Id, AmountOwed = 250, AmountPaid = 0, IsPaid = false },
                    new ExpenseSplit { ExpenseId = dinner.ExpenseId, UserId = bob.Id, AmountOwed = 400, AmountPaid = 0, IsPaid = false },
                    new ExpenseSplit { ExpenseId = dinner.ExpenseId, UserId = charlie.Id, AmountOwed = 350, AmountPaid = 0, IsPaid = false }
                );
                await db.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser> EnsureUser(UserManager<ApplicationUser> users, string email, string password)
        {
            var user = await users.FindByEmailAsync(email);
            if (user != null) return user;
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return user;
        }
    }
}
