using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using WebAPILibragy.Classes;
using WebAPILibragy.DataBase;
using WebAPILibragy.model.database;
//using CLoans = WebAPILibragy.model.custom.Loans_Min;
using CReaders = WebAPILibragy.model.custom.Readers;
namespace WebAPILibragy.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[Route("[controller]/v{version:apiVersion}")]
[ApiVersion("1")]
[ApiVersion("2")]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
[EnableRateLimiting("fixed")]
public class ReadersController : ControllerBase
{

    private readonly ILogger<ReadersController> logger;
    private DBConnect context;

    public ReadersController(DBConnect context, ILogger<ReadersController> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>Вывод списка читателей</summary>
    /// <response code="200">Вернул список читателей</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("")]
    [Authorize(Roles = "admin, Reading")]
    public async Task<IActionResult> GetAllUser()
    {
        try
        {
            List<Readers> Readers = context.Readers.ToList();
            List<CReaders> CReaders = new List<CReaders>();
            for (int i = 0; i < Readers.Count; i++)
            {
                CReaders read = new CReaders(Readers[i]);
                CReaders.Add(read);
            }
            return Ok(CReaders);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом пользователей в систему. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Вывод читателя по фамилии</summary>
    /// <response code="200">Вернул читателя по фамилии</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("{first_name}")]
    [Authorize(Roles = "admin, Reading")]
    public async Task<IActionResult> GetUser(string first_name)
    {
        try
        {
            Readers Readers = context.Readers.FirstOrDefault(p => p.first_name == first_name);
            CReaders read = new CReaders(Readers);
            return Ok(read);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом пользователя в систему. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Создать читателя</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     POST Readers/v1/add
    ///     {
    ///        "last_name": "Jengo",
    ///        "first_name": "Ravioris",
    ///        "patronymic": "",
    ///        "email": "Raviori@mail.ru",
    ///        "phone": "89467115274",
    ///        "address": "Moskow"
    ///     }
    /// </remarks>
    /// <response code="200">Создать читателя</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpPost("add")]
    [Idempotent(cacheTimeInMinutes: 60)]
    [Authorize(Roles = "admin, Reading")]
    public async Task<IActionResult> PostUser([FromBody] CReaders read)
    {
        try
        {
            Readers Readers = new Readers(read);
            context.Readers.AddRange(Readers);
            context.SaveChangesAsync();
            return Ok("Добавление пользователя успешно");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c добавлением пользователя в систему. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Обновить читателя</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     PUT Readers/v1/update
    ///     {
    ///        "last_name": "Jengo",
    ///        "first_name": "Ravioris",
    ///        "patronymic": "Goungan",
    ///        "email": "Raviori@mail.ru",
    ///        "phone": "89467115274",
    ///        "address": "Moskow"
    ///     }
    /// </remarks>
    /// <response code="200">Создать читателя</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpPut("update")]
    [Authorize(Roles = "admin, Reading")]
    public async Task<IActionResult> UpdateUser(CReaders read)
    {
        try
        {
            Readers Readers = new Readers(read);
            context.Readers.Update(Readers);
            context.SaveChangesAsync();
            return Ok("Обновление пользователя");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c обновлением данных пользователя в системе. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Удалить читателя</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     DELETE Readers/v1/delete
    ///     {
    ///        "last_name": "Jengo",
    ///        "first_name": "Ravioris",
    ///        "patronymic": "",
    ///        "email": "Raviori@mail.ru",
    ///        "phone": "89467115274",
    ///        "address": "Moskow"
    ///     }
    /// </remarks>
    /// <response code="200">Удалить читателя</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpDelete("delete")]
    [Authorize(Roles = "admin, Reading")]
    public async Task<IActionResult> DeleteUser(CReaders read)
    {
        try
        {
            Readers Readers = context.Readers.FirstOrDefault(
                p => p.last_name == read.last_name ||
                p.first_name == read.first_name ||
                p.patronymic == read.patronymic ||
                p.email == read.email ||
                p.phone == read.phone ||
                p.address == read.address
                );
            context.Readers.Remove(Readers);
            context.SaveChangesAsync();
            return Ok("Удаление завершено");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c удалением пользователя в системе. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
}
