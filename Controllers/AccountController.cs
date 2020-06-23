using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using EmailService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanificatorSali.Models;
using PlanificatorSali.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using PlanificatorSali.Data;
using Microsoft.EntityFrameworkCore;

namespace PlanificatorSali.Controllers
{
    public class AccountController : Controller
    {

        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly PlanificatorSaliContext _context;

        //mail:


        public AccountController(IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, PlanificatorSaliContext context)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordModel);

            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);

            var message = new Message(new string[] { user.Email }, "Reset password token", callback, null);
            await _emailSender.SendEmailAsync(message);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// mail
        /// </summary>
      
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegistrationModel userModel)
        {

            if (!ModelState.IsValid)
            {
                return View(userModel); // daca modelul nu este valid se returneaza acelasi view cu modelul invalidat
            }

            var user = _mapper.Map<ApplicationUser>(userModel);

            var result = await _userManager.CreateAsync(user, userModel.Password); // createAsync are o parolă, execută verificări suplimentare ale utilizatorului și returnează un rezultat.
            if (!result.Succeeded) //daca se valideaza mapam modelul de inregistrare la utilizator
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return View(userModel);
            }

            if (userModel.esteAdmin)
            {
                await _userManager.AddToRoleAsync(user, "Administrator");

                return RedirectToAction("Register", "Account");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "User");

                return RedirectToAction("Register", "Account");
            }
            
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLoginModel userModel, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(userModel);
            }

            var result = await _signInManager.PasswordSignInAsync(userModel.Email, userModel.Password, userModel.RememberMe, false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Invalid UserName or Password");
                return View();
            }
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(CalendarController.Calendar), "Calendar");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);   //se verifica validitatea modelului

            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);  //se verifica daca utilizatorul exista in baza de date
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation)); // se redirectioneaza catre aceasta metoda

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded) //executăm acțiunea de resetare a parolei cu metoda ResetPasswordAsync. Dacă acțiunea eșuează, adăugăm erori la starea modelului și returnăm o vizualizare
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return View();
            }

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        //Acțiunea ResetPassword HttpGet va accepta o solicitare din e-mail, va extrage valorile de jeton și e-mail și va crea o vizualizare.
        //Acțiunea HttpPost ResetPassword este aici pentru logica principală.

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        public IActionResult GetAllUsers ()
        {
            List<Models.ApplicationUser> userlist = new List<Models.ApplicationUser>();
            userlist = (from users in _context.ApplicationUser select users).ToList();
            return View(userlist);
        }
        
      

        [HttpPost]
        [Route("delete")]
        public IActionResult Delete (string id)
        {
            var user = _context.ApplicationUser.Find(id);
            _context.ApplicationUser.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("GetAllUsers");
        }

        [HttpGet]
        
        public IActionResult Find (string id)
        {
            var user = _context.ApplicationUser.Find(id);
            return new JsonResult(user);
        }

        [HttpPost]
        [Route("update")]
        public IActionResult Update(string id, string nume,string prenume, string email)
        {
            var user = _context.ApplicationUser.Find(id);

            user.Nume = nume;
            user.Prenume = prenume;
            user.Email=email;
            _context.SaveChanges();
            return RedirectToAction("GetAllUsers");
        }

        [HttpGet]
        public IActionResult ProfilulMeu()
        {

            var userid = _userManager.GetUserId(HttpContext.User);
            if(userid==null)
            {
                return RedirectToAction("Login","Account");
            }
            else
            {

            
            ApplicationUser user = _userManager.FindByIdAsync(userid).Result;
            return View(user);
            }
        }



        [HttpPost]
        public IActionResult ProfilulMeu(string Id, string Nume, string Prenume, string PhoneNumber, string Email)
        {
            var user = _context.ApplicationUser.Find(Id);
            user.Nume = Nume;
            user.Prenume = Prenume;
            user.PhoneNumber = PhoneNumber;
            user.Email = Email;
            _context.SaveChanges();
            return RedirectToAction("ProfilulMeu");
        }
    }
}
//Pt public Async Task IActionResult Login
// În primul rând, verificăm dacă modelul este invalid și, dacă este, redăm doar o vizualizare cu modelul. După aceea, folosim metoda FindByEmailAsync de la UserManager
// pentru a returna un utilizator prin e-mail. Dacă utilizatorul există și parola se potrivește cu parola hashed din baza de date, creăm obiectul ClaimsIdentity cu două
// revendicări din interior (Id și UserName). Apoi, conectăm utilizatorul cu metoda SignInAsync, furnizând parametrul schemei și principalul revendicărilor.
// Acest lucru va crea cookie-ul Identity.Aplication în browserul nostru. În final, redirecționăm utilizatorul către acțiunea Index.