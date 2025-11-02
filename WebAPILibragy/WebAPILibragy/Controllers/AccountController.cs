using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Xml.Linq;
using WebAPILibragy.Classes;
using WebAPILibragy.DataBase;
using WebAPILibragy.model.database;
using CAccount = WebAPILibragy.model.custom.Account;

namespace WebAPILibragy.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
[EnableRateLimiting("fixed")]
public class AccountController : ControllerBase
{

    private readonly ILogger<AccountController> logger;
    private DBConnect context;

    public AccountController(DBConnect context, ILogger<AccountController> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>Вход в систему</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     GET Account/Autorization
    ///     {        
    ///       "username": "JengoWins",
    ///       "password": "12345",       
    ///     }
    /// </remarks>
    /// <response code="200">Получить имя и роль пользователя</response>
    /// <response code="400">Не найден пользователь (стандарт. случай), либо ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("Autorization")]
    public async Task<IActionResult> GetAccount([FromQuery] CAccount modelAccount)
    {
        try
        {
            //Формирование данных по таблицам без связей
            Account account = context.Account.FirstOrDefault(p => p.username == modelAccount.username && p.password == modelAccount.password);

            if (account == null)
                return BadRequest("Даже не думайте зайти сюда без авторизации");

            role roles = context.role.FirstOrDefault(p => p.id == account.id_role);

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, account.username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, roles.roles)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Ok($"[username:{account.username},role:{roles.roles}]");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c добавлением книги в библиотеку. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }

    private static int _requestCount = 0;

    [HttpGet("TEST429")]
    public IActionResult Get429()
    {
        _requestCount++;
        return Ok(new
        {
            message = "Request successful",
            requestNumber = _requestCount,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("DontUseStageTable")]
    public async Task<IActionResult> PostData()
    {
        List<Genres> gen = [
            new Genres { name = "Фантастика" },
            new Genres { name = "Романтика" },
            new Genres { name = "Классика" }
        ];
        List<List_Read_Status> status = [
            new List_Read_Status { status = "Забранирован" },
            new List_Read_Status { status = "Занят" },
            new List_Read_Status { status = "Свободный" }
        ];
        List<Publish> publish = [
            new Publish { name = "Эксмо", city = "Москва" },
            new Publish { name = "Занзара", city = "Екатеринбург" }
        ];
        List<role> roles = [
            new role { roles = "Booking" },
            new role { roles = "admin" },
            new role {roles = "Reading" }
        ];
        Account account = new Account
        { 
            username = "JengoWins",
            password = "12345",
            id_role = roles[1].id
        };

        context.Genres.AddRange(gen);
        context.SaveChanges();
        context.List_Read_Status.AddRange(status);
        context.SaveChanges();
        context.Publish.AddRange(publish);
        context.SaveChanges();
        context.role.AddRange(roles);
        context.SaveChanges();

        context.Account.Add(account);
        context.SaveChanges();

        return Ok("Проверьте свою БД");
    }
}
