using DatabaseControllerProvider;
using FormPortal.Core.Models;
using FormPortal.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Claims;
using System.Web;

namespace FormularPortal.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly DbProviderService _dbProviderService;
        private readonly UserService _userService;

        [BindProperty]
        public LoginInput Input { get; set; } = new LoginInput();
        public string? ReturnUrl { get; set; }

        public LoginModel(DbProviderService dbProviderService, UserService userService)
        {
            _dbProviderService = dbProviderService;
            _userService = userService;
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = GetReturnUrl(returnUrl);

            try
            {
                // Clear the existing external cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }

        }


        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl = GetReturnUrl(returnUrl);

            if (ModelState.IsValid)
            {

                // Erst pr�fen wir gegen die Datenbank
                IDbController dbController = _dbProviderService.GetDbController(AppdatenService.DbProvider, AppdatenService.ConnectionString);
                User? user = AppdatenService.IsLocalLoginEnabled ? await _userService.GetAsync(Input.Username, dbController) : null;

                // Lokale Konten m�ssen als ersten gepr�ft werden.
                if (user is not null)
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();

                    string passwordHashed = hasher.HashPassword(user, Input.Password + user.Salt);

                    PasswordVerificationResult result = hasher.VerifyHashedPassword(user, user.Password, Input.Password + user.Salt);
                    // Das Handling l�uft sp�ter auf Basis des Objektes ab.
                    if (result is PasswordVerificationResult.Failed)
                    {
                        user = null;
                    }
                }
                else
                {
                    // Wenn kein lokales Konto gefunden wurde, dann pr�fen wir das Active-Directory
                    if (AppdatenService.IsLdapLoginEnabled)
                    {
                        try
                        {
                            using var connection = new LdapConnection(AppdatenService.LdapServer);

                            var networkCredential = new NetworkCredential(Input.Username, Input.Password, AppdatenService.LdapDomainServer);
                            connection.SessionOptions.SecureSocketLayer = false; // Warnung kann ignoriert werden, ist ein Fehler vom Package
                            connection.AuthType = AuthType.Negotiate;
                            connection.Bind(networkCredential);

                            var searchRequest = new SearchRequest
                                (
                                AppdatenService.LdapDistinguishedName,
                $"(SAMAccountName={Input.Username})",
                                SearchScope.Subtree, new string[]
                                {
                                "cn",
                                "mail",
                                "givenName",
                                "sn",
                                "objectGUID"
                                });

                            SearchResponse directoryResponse = (SearchResponse)connection.SendRequest(searchRequest);

                            SearchResultEntry searchResultEntry = directoryResponse.Entries[0];

                            Dictionary<string, string> attributes = new Dictionary<string, string>();
                            Guid? guid = null;
                            foreach (DirectoryAttribute userReturnAttribute in searchResultEntry.Attributes.Values)
                            {
                                if (userReturnAttribute.Name == "objectGUID")
                                {
                                    byte[] guidByteArray = (byte[])userReturnAttribute.GetValues(typeof(byte[]))[0];
                                    guid = new Guid(guidByteArray);
                                    attributes.Add("guid", ((Guid)guid).ToString());
                                }
                                else
                                {
                                    attributes.Add(userReturnAttribute.Name, (string)userReturnAttribute.GetValues(typeof(string))[0]);
                                }
                            }

                            if (!attributes.ContainsKey("mail"))
                            {
                                attributes.Add("mail", string.Empty);
                            }

                            if (!attributes.ContainsKey("sn"))
                            {
                                attributes.Add("sn", string.Empty);
                            }

                            if (!attributes.ContainsKey("givenName"))
                            {
                                attributes.Add("givenName", string.Empty);
                            }

                            if (guid is null)
                            {
                                throw new InvalidOperationException();
                            }

                            user = await _userService.GetAsync((Guid)guid, dbController);

                            if (user is null)
                            {
                                user = new User
                                {
                                    Username = Input.Username.ToUpper(),
                                    ActiveDirectoryGuid = (Guid)guid,
                                    Email = attributes["mail"],
                                    DisplayName = $"{attributes["givenName"]} {attributes["sn"]}",
                                    Origin = "ad"
                                };

                                // Give the first user of the app all permissions
                                if (!AppdatenService.FirstUserExists)
                                {
                                    foreach (var permission in AppdatenService.Permissions)
                                    {
                                        user.Permissions.Add(permission);
                                    }
                                }

                                await _userService.CreateAsync(user, dbController);
                                AppdatenService.FirstUserExists = true;
                            }



                        }
                        catch (LdapException ex)
                        {

                        }
                    }
                }

                // Wenn wir ein Mitarbeiter Objekt haben, dann k�nnen wir uns einloggen, ansonsten ist irgendwas schief gelaufen
                if (user is not null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("userId", user.UserId.ToString()),
                    };

                    foreach (var permission in user.Permissions)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, permission.Identifier));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = Input.RememberMe,
                        RedirectUri = returnUrl
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    if (!AppdatenService.IsLdapLoginEnabled && !AppdatenService.IsLocalLoginEnabled)
                    {
                        ModelState.AddModelError("login-error", "Es wurde kein Provider zum einloggen gefunden. Bitte wenden Sie sich an Ihren Administrator.");
                    }
                    else
                    {
                        ModelState.AddModelError("login-error", "Username oder Passwort ist falsch.");
                    }
                }
            }
            return Page();
        }

        private string GetReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return Url.Content("~/");
            }
            else
            {
                string[] parts = returnUrl.Split('/');

                string url = string.Empty;

                foreach (var item in parts)
                {
                    url += $"/{HttpUtility.UrlEncode(item)}";
                }

                return url.Replace("//", "/");
            }
        }
    }

    public class LoginInput
    {
        [Required]
        public string Username { get; set; } = String.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = String.Empty;
        [Display(Name = "Eingeloggt bleiben")]
        public bool RememberMe { get; set; }
    }
}