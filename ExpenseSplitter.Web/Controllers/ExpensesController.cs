using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseSplitter.Web.Data;
using ExpenseSplitter.Web.Models;
using ExpenseSplitter.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter.Web.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public ExpensesController(ApplicationDbContext db, UserManager<ApplicationUser> users)
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

        // Step 1
        [HttpGet]
        public async Task<IActionResult> Create(int tripId)
        {
            var trip = await _db.Trips.Include(t => t.Members).FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip == null) return NotFound();

            var memberOptions = new List<MemberOption>();
            foreach (var m in trip.Members)
            {
                var u = await _users.FindByIdAsync(m.UserId);
                memberOptions.Add(new MemberOption
                {
                    UserId = m.UserId,
                    Display = GetDisplay(u, m.UserId)
                });
            }

            var vm = new ExpenseStep1Vm
            {
                TripId = tripId,
                ExpenseDate = DateTime.UtcNow.Date,
                Members = memberOptions
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExpenseStep1Vm vm)
        {
            if (vm.TotalAmount <= 0)
            {
                ModelState.AddModelError(nameof(vm.TotalAmount), "Amount must be greater than 0");
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            return RedirectToAction(nameof(Split), vm);
        }

        // Step 2
        [HttpGet]
        public async Task<IActionResult> Split(ExpenseStep1Vm vm)
        {
            // reload members
            var trip = await _db.Trips.Include(t => t.Members).FirstOrDefaultAsync(t => t.TripId == vm.TripId);
            if (trip == null) return NotFound();
            vm.Members = new List<MemberOption>();
            foreach (var m in trip.Members)
            {
                var u = await _users.FindByIdAsync(m.UserId);
                vm.Members.Add(new MemberOption { UserId = m.UserId, Display = GetDisplay(u, m.UserId) });
            }
            if (vm.Members.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "This trip has no members. Add members before creating an expense.");
            }
            if (string.IsNullOrWhiteSpace(vm.PaidBy) && vm.Members.Count > 0)
            {
                vm.PaidBy = vm.Members.First().UserId;
            }
            var vm2 = new ExpenseStep2Vm
            {
                TripId = vm.TripId,
                Title = vm.Title,
                TotalAmount = vm.TotalAmount,
                Category = vm.Category,
                ExpenseDate = vm.ExpenseDate,
                Description = vm.Description,
                PaidBy = vm.PaidBy,
                Members = vm.Members,
                SplitMethod = SplitMethod.EqualAll
            };
            return View(vm2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Split(ExpenseStep2Vm vm)
        {
            if (vm.TotalAmount <= 0)
            {
                ModelState.AddModelError(nameof(vm.TotalAmount), "Amount must be greater than 0");
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            return RedirectToAction(nameof(Confirm), vm);
        }

        // Step 3
        [HttpGet]
        public async Task<IActionResult> Confirm(ExpenseStep2Vm vm)
        {
            // Ensure Members are present for the confirm view
            if (vm.Members == null || vm.Members.Count == 0)
            {
                var trip = await _db.Trips.Include(t => t.Members).FirstOrDefaultAsync(t => t.TripId == vm.TripId);
                if (trip != null)
                {
                    vm.Members = new List<MemberOption>();
                    foreach (var m in trip.Members)
                    {
                        var u = await _users.FindByIdAsync(m.UserId);
                        vm.Members.Add(new MemberOption { UserId = m.UserId, Display = GetDisplay(u, m.UserId) });
                    }
                }
            }

            var vm3 = new ExpenseStep3Vm
            {
                TripId = vm.TripId,
                Title = vm.Title,
                TotalAmount = vm.TotalAmount,
                Category = vm.Category,
                ExpenseDate = vm.ExpenseDate,
                Description = vm.Description,
                PaidBy = vm.PaidBy,
                Members = vm.Members,
                SplitMethod = vm.SplitMethod,
                SelectedUserIds = vm.SelectedUserIds ?? new List<string>(),
                CustomSplits = vm.CustomSplits ?? new List<MemberOption>(),
                IsSettled = false
            };
            return View(vm3);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ExpenseStep3Vm vm)
        {
            if (vm.TotalAmount <= 0)
            {
                ModelState.AddModelError(nameof(vm.TotalAmount), "Amount must be greater than 0");
            }
            if (!ModelState.IsValid)
            {
                // reload members to render the view correctly
                var trip0 = await _db.Trips.Include(t => t.Members).FirstOrDefaultAsync(t => t.TripId == vm.TripId);
                vm.Members = new List<MemberOption>();
                if (trip0 != null)
                {
                    foreach (var m in trip0.Members)
                    {
                        var u = await _users.FindByIdAsync(m.UserId);
                        vm.Members.Add(new MemberOption { UserId = m.UserId, Display = GetDisplay(u, m.UserId) });
                    }
                }
                vm.SelectedUserIds ??= new List<string>();
                vm.CustomSplits ??= new List<MemberOption>();
                return View(vm);
            }

            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == vm.TripId);
            if (trip == null) return NotFound();
            // Ensure members are loaded
            var members = await _db.TripMembers.Where(x => x.TripId == vm.TripId).ToListAsync();
            if (members.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "This trip has no members. Add members before creating an expense.");
                return await Confirm(new ExpenseStep2Vm
                {
                    TripId = vm.TripId,
                    Title = vm.Title,
                    TotalAmount = vm.TotalAmount,
                    Category = vm.Category,
                    ExpenseDate = vm.ExpenseDate,
                    Description = vm.Description,
                    PaidBy = vm.PaidBy,
                    Members = vm.Members,
                    SplitMethod = vm.SplitMethod,
                    SelectedUserIds = vm.SelectedUserIds,
                    CustomSplits = vm.CustomSplits
                });
            }
            if (!members.Any(x => x.UserId == vm.PaidBy))
            {
                ModelState.AddModelError(nameof(vm.PaidBy), "Paid By must be one of the trip members.");
                return await Confirm(new ExpenseStep2Vm
                {
                    TripId = vm.TripId,
                    Title = vm.Title,
                    TotalAmount = vm.TotalAmount,
                    Category = vm.Category,
                    ExpenseDate = vm.ExpenseDate,
                    Description = vm.Description,
                    PaidBy = vm.PaidBy,
                    Members = vm.Members,
                    SplitMethod = vm.SplitMethod,
                    SelectedUserIds = vm.SelectedUserIds,
                    CustomSplits = vm.CustomSplits
                });
            }

            var expense = new Expense
            {
                TripId = vm.TripId,
                Title = vm.Title,
                TotalAmount = vm.TotalAmount,
                PaidBy = vm.PaidBy,
                ExpenseDate = vm.ExpenseDate,
                Category = vm.Category,
                Description = vm.Description,
                IsSettled = vm.IsSettled
            };
            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();

            vm.SelectedUserIds ??= new List<string>();
            vm.CustomSplits ??= new List<MemberOption>();
            IEnumerable<(string userId, decimal amount)> shares = CalculateShares(vm);
            foreach (var (userId, amount) in shares)
            {
                _db.ExpenseSplits.Add(new ExpenseSplit
                {
                    ExpenseId = expense.ExpenseId,
                    UserId = userId,
                    AmountOwed = amount,
                    AmountPaid = vm.IsSettled ? amount : 0,
                    IsPaid = vm.IsSettled
                });
            }
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = vm.TripId });
        }

        private IEnumerable<(string userId, decimal amount)> CalculateShares(ExpenseStep3Vm vm)
        {
            var members = vm.Members ?? new List<MemberOption>();
            if (vm.SplitMethod == SplitMethod.EqualAll)
            {
                var count = Math.Max(1, members.Count);
                var each = Math.Round(vm.TotalAmount / count, 2, MidpointRounding.AwayFromZero);
                return members.Select(m => (m.UserId, each));
            }
            else if (vm.SplitMethod == SplitMethod.EqualSelected)
            {
                var selected = members.Where(m => vm.SelectedUserIds.Contains(m.UserId)).ToList();
                if (!selected.Any()) selected = members;
                var count = Math.Max(1, selected.Count);
                var each = Math.Round(vm.TotalAmount / count, 2, MidpointRounding.AwayFromZero);
                return selected.Select(m => (m.UserId, each));
            }
            else // Custom
            {
                var map = new Dictionary<string, decimal>();
                foreach (var c in vm.CustomSplits)
                {
                    if (c.CustomAmount.HasValue)
                    {
                        map[c.UserId] = Math.Max(0, Math.Round(c.CustomAmount.Value, 2));
                    }
                }
                var sum = map.Values.Sum();
                if (sum != vm.TotalAmount)
                {
                    // normalize proportionally if mismatch
                    if (sum > 0)
                    {
                        var factor = vm.TotalAmount / sum;
                        foreach (var k in map.Keys.ToList())
                        {
                            map[k] = Math.Round(map[k] * factor, 2);
                        }
                    }
                    else
                    {
                        var count = Math.Max(1, members.Count);
                        var each = Math.Round(vm.TotalAmount / count, 2, MidpointRounding.AwayFromZero);
                        return members.Select(m => (m.UserId, each));
                    }
                }
                return map.Select(kv => (kv.Key, kv.Value));
            }
        }
    }
}
