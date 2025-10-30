using System;
using System.Linq;
using System.Threading.Tasks;
using ExpenseSplitter.Web.Data;
using ExpenseSplitter.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter.Web.Controllers
{
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public TripsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        private static string GetDisplay(ApplicationUser? u, string fallbackId)
        {
            if (u == null) return fallbackId;
            if (!string.IsNullOrWhiteSpace(u.UserName) && u.UserName!.Contains('@') == false) return u.UserName!;
            if (!string.IsNullOrWhiteSpace(u.UserName)) return u.UserName!;
            if (!string.IsNullOrWhiteSpace(u.Email)) return u.Email!;
            return fallbackId;
        }

        public async Task<IActionResult> Index()
        {
            var trips = await _db.Trips
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return View(trips);
        }

        public IActionResult Create()
        {
            return View(new Trip { StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trip trip)
        {
            if (!ModelState.IsValid)
            {
                return View(trip);
            }

            var current = await _users.GetUserAsync(User);
            if (current == null)
            {
                // Guest mode: ensure a fallback user exists
                var guest = await _users.FindByEmailAsync("guest@local");
                if (guest == null)
                {
                    guest = new ApplicationUser { UserName = "guest@local", Email = "guest@local", EmailConfirmed = true };
                    await _users.CreateAsync(guest, "guest123");
                }
                trip.CreatedBy = guest.Id;
            }
            else
            {
                trip.CreatedBy = current.Id;
            }
            trip.CreatedAt = DateTime.UtcNow;
            _db.Trips.Add(trip);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var trip = await _db.Trips
                .Include(t => t.Members)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.TripId == id);
            if (trip == null) return NotFound();
            ViewBag.UserDisplay = await BuildUserDisplayMap(trip);
            return View(trip);
        }

        // Members management
        public async Task<IActionResult> Members(int id)
        {
            var trip = await _db.Trips
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.TripId == id);
            if (trip == null) return NotFound();
            ViewBag.UserDisplay = await BuildUserDisplayMap(trip);
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int tripId, string email, decimal personalBudget)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip == null) return NotFound();

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("email", "Email is required");
                return await Members(tripId);
            }

            var user = await _users.FindByEmailAsync(email);
            if (user == null)
            {
                // Create a placeholder user so membership and splits can reference an identity without requiring login
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var createResult = await _users.CreateAsync(user, "guest123");
                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError("email", string.Join("; ", createResult.Errors.Select(e => e.Description)));
                    return await Members(tripId);
                }
            }

            var exists = await _db.TripMembers.AnyAsync(m => m.TripId == tripId && m.UserId == user.Id);
            if (!exists)
            {
                _db.TripMembers.Add(new TripMember
                {
                    TripId = tripId,
                    UserId = user.Id,
                    PersonalBudget = Math.Max(0, personalBudget),
                    CurrentBalance = 0
                });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Members), new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBudget(int tripMemberId, decimal personalBudget)
        {
            var member = await _db.TripMembers.FindAsync(tripMemberId);
            if (member == null) return NotFound();
            member.PersonalBudget = Math.Max(0, personalBudget);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Members), new { id = member.TripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int tripMemberId)
        {
            var member = await _db.TripMembers.FindAsync(tripMemberId);
            if (member == null) return NotFound();
            var tripId = member.TripId;
            _db.TripMembers.Remove(member);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Members), new { id = tripId });
        }

        private async Task<Dictionary<string, string>> BuildUserDisplayMap(Trip trip)
        {
            var map = new Dictionary<string, string>();
            var ids = new HashSet<string>();
            foreach (var m in trip.Members) ids.Add(m.UserId);
            foreach (var e in trip.Expenses) ids.Add(e.PaidBy);

            foreach (var id in ids)
            {
                var u = await _users.FindByIdAsync(id);
                map[id] = GetDisplay(u, id);
            }
            return map;
        }
    }
}
