using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    private readonly UserManager<User> _userManager;

    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        ApplicationContext applicationContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _applicationContext = applicationContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    async Task AddUerToRole(User user, string role)
    {
        var isExists = await _roleManager.RoleExistsAsync(role);

        if (!isExists)
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
                throw new Exception("cannot create role");
        }

        var addingRole = await _userManager.AddToRoleAsync(user, role);

        if (!addingRole.Succeeded)
            throw new Exception("cannot create role");
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] UserPost user)
    {
        await user.Avatar.WriteFileToDirectory();
        var userWr = new User
        {
            AvatarUrl = user.Avatar.FileName,
            Fio = user.Fio,
            Specialities = user.Specialities,
            TelegramBotUrl = user.TelegramBotUrl,
            UserName = user.Username,
            Email = user.Email
        };
        var result = await _userManager.CreateAsync(userWr);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        

        var role = user.Username == "moderator" ? "модератор"  : "спикер";

        await AddUerToRole(userWr, role);

        var token = GetToken(userWr, role);

        return Ok(new { userWr.Id, Token = token });
    }

    string GetToken(User user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName!),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
        };
        var bOpt = new BearerAccessTokenOptions();
        return bOpt.GetBearerToken(claims);
    }

    [Authorize(Roles = "модератор")]
    [HttpPut]
    public async Task<IActionResult> Put([FromForm] UserPut user)
    {
        await user.Avatar.WriteFileToDirectory();

        var fuser = await _userManager.FindByIdAsync(user.Id);

        if (fuser is null)
            throw new Exception("user not found");

        fuser.AvatarUrl = user.Avatar.FileName;
        fuser.Fio = user.Fio;
        fuser.Specialities = user.Specialities;
        fuser.TelegramBotUrl = user.TelegramBotUrl;
        fuser.UserName = user.Username;
        fuser.Email = user.Email;
        var result = await _userManager.UpdateAsync(fuser);
        if (result.Succeeded)
            return Ok();
        return BadRequest(result.Errors);
    }

    [Authorize(Roles = "модератор")]
    [HttpPost("moderator")]
    public async Task<IActionResult> PostModerator([FromForm] UserPost user)
    {
        await user.Avatar.WriteFileToDirectory();
        var userWr = new User
        {
            AvatarUrl = user.Avatar.FileName,
            Fio = user.Fio,
            Specialities = user.Specialities,
            TelegramBotUrl = user.TelegramBotUrl,
            UserName = user.Username
        };

        var result = await _userManager.CreateAsync(userWr);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var role = "модератор";

        await AddUerToRole(userWr, role);
        var token = GetToken(userWr, role);
        return Ok(new { userWr.Id, Token = token });
    }

    [AllowAnonymous]
    [HttpGet("signin-google")]
    public IActionResult SignInWithGoogle()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(SignInWithGoogleCallback)) };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet("signin-google-callback")]
    public async Task<IActionResult> SignInWithGoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (result?.Succeeded != true)
        {
            return BadRequest("Ошибка аутентификации Google");
        }

        // Извлеките информацию о пользователе из результата аутентификации
        var claims = result.Principal!.Identities
            .FirstOrDefault(y => y.AuthenticationType == GoogleDefaults.AuthenticationScheme)?
            .Claims;


        var email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)!.Value;
        var name = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Name)!.Value;

        var user = await _userManager.FindByEmailAsync(email!);
        var role = "спикер";
        if(user is null)
        {
            user = new User
            {
                Fio = name!,
                Specialities = string.Empty,
                TelegramBotUrl = string.Empty,
                AvatarUrl = $"https://www.gravatar.com/avatar/{BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(email!))).Replace("-", "").ToLowerInvariant()}?d=identicon",
                UserName = name,
                Email = email
            };
            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            

           

            await AddUerToRole(user, role);
        }
        

        var token = GetToken(user, role);

        

        // Здесь вы можете создать JWT или другой токен для аутентификации в вашем приложении
        // и отправить его пользователю.

        return Ok(new {
            Token = token,
            User = user
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if(user is null)
            return BadRequest();

        var role = await _userManager.GetRolesAsync(user);
        var token = GetToken(user, role.First());

        return Ok(new {
            token,
            user
        });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUserData(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
            return BadRequest();

        var role = await _userManager.GetRolesAsync(user);
        return Ok(new { user, role });
    }

    [Authorize]
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
            return BadRequest();

        var stats = await _applicationContext.GetStatistic(id);

        var achievmnets = _applicationContext.Achievments.Where(c => c.UserId == id);

        return Ok(new { Stats = stats, Achievments = achievmnets });
    }
}
