using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(_roleManager.Roles);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppRole model)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }



            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var role=await _roleManager.FindByIdAsync(id);

            if (role != null && role.Name!=null)
            {
                ViewBag.Users = await _userManager.GetUsersInRoleAsync(role.Name);
                return View(role);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AppRole model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);  //Uses _roleManager to find the role in the database by its ID.
                if (role != null)
                {   
                    role.Name = model.Name; //If the role is found, it updates the role's name with the value from the model.
                    var result = await _roleManager.UpdateAsync(role);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    if(role.Id != null)
                    { ViewBag.Users = await _userManager.GetUsersInRoleAsync(role.Name); }  //Calls _userManager.GetUsersInRoleAsync(role.Name)
                                                                                            //to get a list of users in the role

                }
            }

            return View(model) ;
        }
    }
}
