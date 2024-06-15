using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Plan_A_Plant.Models;
using Plan_A_Plant.Utility;

namespace Plan_A_Plant.Identity.Pages.Account
{
    [AllowAnonymous]

    public class VerifyOTPModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<VerifyOTPModel> _logger;
        private readonly IEmailService _emailSender;


        public VerifyOTPModel(UserManager<IdentityUser> userManager, ILogger<VerifyOTPModel> logger, IEmailService emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public TwoStepModel TwoStepModel { get; set; }

        public string Email { get; set; }
        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }

        // Property to hold the time when OTP was generated
        public DateTime OTPGeneratedTime { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            Email = email;
            ReturnUrl = returnUrl;

            // Clear error message
            ErrorMessage = null;

            // Store the time when OTP is generated in TempData
            TempData["OTPGeneratedTime"] = DateTime.UtcNow.ToString();

            return Page();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(string email, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid user.");
                return Page();
            }

            var isTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", TwoStepModel.TwoFactorCode);
            if (!isTokenValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid OTP.");
                ErrorMessage = "Invalid OTP.";
                ModelState.Remove("TwoStepModel.TwoFactorCode");
                return RedirectToPage(new { email = email, returnUrl = returnUrl, errorMessage = ErrorMessage });
            }

            // Retrieve the timestamp when OTP is generated from TempData
            var tokenTimestampString = TempData.Peek("OTPGeneratedTime") as string;

            if (tokenTimestampString == null || !DateTime.TryParse(tokenTimestampString, out var tokenTimestamp))
            {
                // Handle the case where the token timestamp is not available
                ModelState.AddModelError(string.Empty, "OTP timestamp not available.");
                ErrorMessage = "OTP timestamp not available.";
                ModelState.Remove("TwoStepModel.TwoFactorCode");
                return RedirectToPage(new { email = email, returnUrl = returnUrl, errorMessage = ErrorMessage });
            }

            // Check if OTP is expired
            if (DateTime.UtcNow.Subtract(tokenTimestamp).TotalMinutes > 1)
            {
                ModelState.AddModelError(string.Empty, "OTP has expired.");
                ErrorMessage = "OTP has expired.";
                ModelState.Remove("TwoStepModel.TwoFactorCode");
                return RedirectToPage(new { email = email, returnUrl = returnUrl, errorMessage = ErrorMessage });
            }

            // OTP is valid, disable Two-Factor Authentication
            await _userManager.SetTwoFactorEnabledAsync(user, false);

            _logger.LogInformation("OTP successfully verified for user: {Email}", email);

            // Redirect user to the login page
            return Redirect("~/Identity/Account/Login?returnUrl=" + ReturnUrl);
        }

        public async Task<IActionResult> OnPostResendOTPAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Invalid user.");
            }

            // Generate new OTP
            var newToken = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            // Send email with new OTP
            // Assume _emailSender is your email sending service
            var mes = new Message(new string[] { user.Email }, "Authentication token", newToken);
            _emailSender.SendEmail(mes);

            _logger.LogInformation("New OTP sent for user: {Email}", email);

            // Store the time when OTP is generated in TempData
            TempData["OTPGeneratedTime"] = DateTime.UtcNow.ToString();

            // Redirect user back to the VerifyOTP page
            return RedirectToPage(new { email = email, returnUrl = returnUrl });
        }
    }
}
