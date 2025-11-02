using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using WebAPILibragy.Classes;
using WebAPILibragy.DataBase;
using WebAPILibragy.model.custom;
using WebAPILibragy.model.database;
using CLoans = WebAPILibragy.model.custom.Loans_Min;

namespace WebAPILibragy.Controllers;

[ApiController]
[Route("[controller]/v{version:apiVersion}")]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[ApiVersion("1")]
[ApiVersion("2")]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
[EnableRateLimiting("fixed")]
public class LoansController : ControllerBase
{
    private readonly ILogger<LoansController> logger;
    private DBConnect context;

    public LoansController(DBConnect context, ILogger<LoansController> logger)
    {
        this.context = context;
        this.logger = logger;
    }
    /// <summary>Получить весь список состояния учета книг и читателей</summary>
    /// <response code="200">Вернул список состояния читателей</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("")]
    [Authorize(Roles = "admin, Reading")]
    [MapToApiVersion("2")]
    public async Task<IActionResult> GetAllLeaseholder()
    {
        try
        {
            string query = "SELECT \"Readers\".last_name AS last_name,\"Readers\".first_name AS first_name,\"Readers\".phone AS phone,\"Name_Books\".name AS name_book,\"Genres\".name AS genres,\"Publish\".name AS publisher,\"List_Read_Status\".status AS status,time_of_issue AS time_of_issue FROM \"Loans\" JOIN \"Readers\" ON \"Loans\".id_readers = \"Readers\".id JOIN \"List_Read_Status\" ON \"Loans\".id_status = \"List_Read_Status\".id JOIN \"Books\" ON \"Loans\".id_books = \"Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_genres = \"Publish\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id";
            var client = context.Database.SqlQueryRaw<CLoans>(query).ToList();
            logger.LogDebug("Рычаг GetAllLeaseholder() - Вызвать всех клиентов");
            return Ok(client);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом клиентов. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Получить список состояния учета книг и читателей по фильтру</summary>
    /// <response code="200">Вернул список состояния читателей</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("{filter},{category}")]
    [Authorize(Roles = "admin, Reading")]
    [MapToApiVersion("2")]
    public async Task<IActionResult> GetLeaseholder(string filter = "Свободный", string category = "status")
    {
        try
        {
            string query = "SELECT \"Readers\".last_name AS last_name,\"Readers\".first_name AS first_name,\"Readers\".phone AS phone,\"Name_Books\".name AS name_book,\"Genres\".name AS genres,\"Publish\".name AS publisher,\"List_Read_Status\".status AS status,time_of_issue AS time_of_issue FROM \"Loans\" JOIN \"Readers\" ON \"Loans\".id_readers = \"Readers\".id JOIN \"List_Read_Status\" ON \"Loans\".id_status = \"List_Read_Status\".id JOIN \"Books\" ON \"Loans\".id_books = \"Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_genres = \"Publish\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id";
            string query_where = "";
            if(category == "first_name")
                query_where = $" where \"Readers\".first_name=={filter}";
            else if(category == "status")
                query_where = $" where \"List_Read_Status\".status=={filter}";
            var client = context.Database.SqlQueryRaw<CLoans>(query).ToList();

            logger.LogDebug("Рычаг GetLeaseholder() - Вызвать клиентов по фильтру");
            return Ok(client);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом клиента. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Создать учет книги и читателя</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     POST Loans/v2/create
    ///     {
    ///       "last_name": "Jengo",
    ///       "first_name": "Ravioris",
    ///       "phone": "89467115274",
    ///       "name_book": "Звездные Войны. Реван",
    ///       "genres": "Фантастика",
    ///       "publisher": "Эксмо",
    ///       "status": "Занят",
    ///       "time_of_issue": "2025-10-21T18:26:49.690Z"
    ///     }
    /// </remarks>
    /// <response code="200">Запись данных</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpPost("create")]
    [Idempotent(cacheTimeInMinutes: 60)]
    [Authorize(Roles = "admin, Reading")]
    [MapToApiVersion("2")]
    public async Task<IActionResult> PostLeaseholder(CLoans read)
    {
        try
        {
            //Формирование данных по таблицам без связей
            model.database.Readers readers = context.Readers.FirstOrDefault(p => p.first_name == read.first_name && p.phone == read.phone);
            Name_Books book_name = context.Name_Books.FirstOrDefault(p => p.name == read.name_book);
            model.database.Books books = context.Books.FirstOrDefault(p => p.id_name == book_name.id);

            Genres genres = context.Genres.FirstOrDefault(p => p.name == read.genres);
            Publish publish = context.Publish.FirstOrDefault(p => p.name == read.publisher);
            List_Read_Status status = context.List_Read_Status.FirstOrDefault(p=>p.status == read.status);

            Loans loans = new Loans { 
                id_books = books.id,
                id_readers=readers.id,
                id_status=status.id,
                time_of_issue = read.time_of_issue
            };

            //Распределение несвязных данных по таблицам
            context.Loans.Add(loans);
            context.SaveChanges();
            logger.LogDebug("Рычаг PostLeaseholder(CLoans модель) - Записал данные");
            context.SaveChanges();
            var uri = new Uri($"https://localhost:7154/Library/v1/GetBooks/{book_name.name}");
            return Redirect(uri.AbsoluteUri);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c добавлением клиента в базу. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Обновить учет книги и читателя</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     PUT Loans/v2/update
    ///     {
    ///       "last_name": "Jengo",
    ///       "first_name": "Ravioris",
    ///       "phone": "89467115274",
    ///       "name_book": "Звездные Войны. Старая Республика. Реван",
    ///       "genres": "Фантастика",
    ///       "publisher": "Эксмо",
    ///       "status": "Свободный",
    ///       "time_of_issue": "2025-10-21T18:26:49.690Z"
    ///     }
    /// </remarks>
    /// <response code="200">Запись обновленных данных</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpPut("update")]
    [Authorize(Roles = "admin, Reading")]
    [MapToApiVersion("2")]
    public async Task<IActionResult> UpdateLeaseholder(CLoans read)
    {
        try
        {
            //Формирование данных по таблицам без связей
            model.database.Readers readers = context.Readers.FirstOrDefault(p => p.first_name == read.first_name && p.last_name == read.last_name);
            //Формирование данных по таблицам без связей
            Loans loans = context.Loans.FirstOrDefault(p => p.id_readers == readers.id);

            Loans loans_reject = new Loans
            {
                id = loans.id,
                id_books = loans.id_books,
                id_readers = loans.id_readers,
                id_status = loans.id_status,
                time_of_issue = read.time_of_issue
            };

            context.Loans.Remove(loans);


            Name_Books? book_name = context.Name_Books.FirstOrDefault(p => p.name == read.name_book);
            model.database.Books book = context.Books.FirstOrDefault(p => p.id_name == book_name.id);
            List_Read_Status? status = context.List_Read_Status.FirstOrDefault(p => p.status == read.status);

            if (book != null)
                loans_reject.id_books = book.id;


            if (status != null)
                loans_reject.id_status = status.id;


            context.Loans.Add(loans_reject);
            context.SaveChanges();
            return Ok($"Обновление данных");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c обновлением данных клиента. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Удаление учет книги и читателя</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     DELETE Loans/v2/delete
    ///     {
    ///       "last_name": "Jengo",
    ///       "first_name": "Ravioris",
    ///       "phone": "89467115274",
    ///       "name_book": "Звездные Войны. Старая Республика. Реван",
    ///       "genres": "Фантастика",
    ///       "publisher": "Эксмо",
    ///       "status": "Занят",
    ///       "time_of_issue": "2025-10-21T18:26:49.690Z"
    ///     }
    /// </remarks>
    /// <response code="200">Удаление данных</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpDelete("delete")]
    [Authorize(Roles = "admin, Reading")]
    [MapToApiVersion("2")]
    public async Task<IActionResult> DeleteLeaseholder(CLoans read)
    {
        try
        {
            //Формирование данных по таблицам без связей
            model.database.Readers readers = context.Readers.FirstOrDefault(p => p.first_name == read.first_name && p.last_name == read.last_name);
            //Формирование данных по таблицам без связей
            Loans loans = context.Loans.FirstOrDefault(p => p.id_readers == readers.id);

            context.Loans.Remove(loans);
            context.SaveChanges();
            return Ok($"Удаление {read.first_name} из базу данных");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c удалением клиента из базы. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
}
