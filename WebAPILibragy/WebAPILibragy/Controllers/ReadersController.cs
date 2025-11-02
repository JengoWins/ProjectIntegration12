using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
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
    /*
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
    */
    /// <summary>Получение книги по опционалу</summary>
    /// <response code="200">Получить книгу</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("filters/{first_name}")]
    [Authorize(Roles = "admin, Reading")]
    public async Task<IActionResult> GetUser(string first_name, [FromQuery] string[] includes)
    {
        try
        {
            int elementCount = includes.Length;
            List<Readers?> Readers = null;
            switch (elementCount)
            {
                case 0:
                    Readers = context.Readers.Where(p => p.first_name == first_name).ToList();
                    break;
                case 1:
                    Readers = context.Readers.Where(p => p.first_name == first_name && p.last_name == includes[0]).ToList();
                    break;
                case 2:
                    Readers = context.Readers.Where(p => p.first_name == first_name && p.last_name == includes[0] && p.patronymic == includes[1]).ToList();
                    break;
                case 3:
                     Readers = context.Readers.Where(p => p.first_name == first_name && p.last_name == includes[0] && p.patronymic == includes[1] && p.email == includes[2]).ToList();
                    break;
            }


            if (Readers != null)
            {
                List<CReaders> CReaders = new List<CReaders>();
                for (int i = 0; i < Readers.Count; i++)
                {
                    CReaders read = new CReaders(Readers[i]);
                    CReaders.Add(read);
                }
                return Ok(CReaders);
            }
            else
                return BadRequest("Серваку не удалось найти данные");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом книги по его Названию. Уровень ошибки в районе SQL-Запроса.  Тип ошибки: " + ex);
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
            Readers red = context.Readers.FirstOrDefault(p => p.first_name == read.first_name);
            Readers Readers = red;
            context.Readers.Remove(red);
            context.SaveChanges();

            if (read.patronymic != Readers.patronymic)
                Readers.patronymic = read.patronymic;
            if (read.email != Readers.email)
                Readers.email = read.email;
            if (read.phone != Readers.phone)
                Readers.phone = read.phone;
            if (read.address != Readers.address)
                Readers.address = read.address;
            
            context.Readers.Add(Readers);
            context.SaveChanges();
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
