using Infrastructure.Contexts;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize]

public class AccountController : Controller
{

    private readonly UserManager<UserEntity> _userManager;
    private readonly ApplicationContext _context;

    public AccountController(UserManager<UserEntity> userManager, ApplicationContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Details()
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var user = await _context.Users.Include(i => i.Adress).FirstOrDefaultAsync(x => x.Id == nameIdentifier);



        var viewModel = new AccountDetailsViewModel
        {
            Basic = new AccountBasicInfo
            {
                FirstName = user!.FirstName,
                LastName = user!.LastName,
                Email = user!.Email!,
                PhoneNumber = user!.PhoneNumber,
                Bio = user.Bio,
            },
            Adress = new AccountAdressInfo
            {
                AdressLine_1 = user.Adress?.AdressLine_1!,
                AdressLine_2 = user.Adress?.AdressLine_2!,
                PostalCode = user.Adress?.PostalCode!,
                City = user.Adress?.City!
            }

        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateBasicInfo(AccountDetailsViewModel model)
    {

        if (TryValidateModel(model.Basic!))
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.FirstName = model.Basic!.FirstName;
                user.LastName = model.Basic!.LastName;
                user.Email = model.Basic!.Email;
                user.PhoneNumber = model.Basic!.PhoneNumber;
                user.UserName = model.Basic!.Email;
                user.Bio = model.Basic!.Bio;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["StatusMessage"] = "Updated basic information successfully.";
                }
                else
                {
                    TempData["StatusMessage"] = "Unable to save basic information.";
                }

            }

        }
        else
        {
            TempData["StatusMessage"] = "Unable to save basic information.";
        }

        return RedirectToAction("Details", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAddressInfo(AccountDetailsViewModel model)
    {

        if (TryValidateModel(model.Adress!))
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var user = await _context.Users.Include(i => i.Adress).FirstOrDefaultAsync(x => x.Id == nameIdentifier);
            if (user != null)
            {
                try
                {
                    if (user.Adress != null)
                    {
                        user.Adress.AdressLine_1 = model.Adress!.AdressLine_1;
                        user.Adress.AdressLine_2 = model.Adress!.AdressLine_2;
                        user.Adress.PostalCode = model.Adress!.PostalCode;
                        user.Adress.City = model.Adress!.City;
                    }
                    else
                    {
                        user.Adress = new AdressEntity
                        {
                            AdressLine_1 = model.Adress!.AdressLine_1,
                            AdressLine_2 = model.Adress!.AdressLine_2,
                            PostalCode = model.Adress!.PostalCode,
                            City = model.Adress!.City
                        };
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Updated address information successfully.";
                }

                catch
                {
                    TempData["StatusMessage"] = "Unable to save address information.";

                }

            }

        }
        else
        {
            TempData["StatusMessage"] = "Unable to save address information.";
        }

        return RedirectToAction("Details", "Account");
    }

    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        var user = await _userManager.GetUserAsync(User);
        
        
        
        
        
        if (user != null && file != null && file.Length != 0)
        {
            var fileName = $"p_{user.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/uploads/profiles", fileName);

            using var fs = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fs);

            user.ProfileImage = fileName;
            await _userManager.UpdateAsync(user);

        }
        else
        {
            TempData["StatusMessage"] = "Unable to upload profile image,";
        }
        return RedirectToAction("Details", "Account");
    }

}
