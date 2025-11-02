using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WebAPILibragy.Classes;
using WebAPILibragy.DataBase;
using WebAPILibragy.model.database;
using CBooks = WebAPILibragy.model.custom.Books;

namespace WebAPILibragy.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[Route("[controller]/v{version:apiVersion}")]
[ApiVersion("1")]
[ApiVersion("2")]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
[EnableRateLimiting("fixed")]
public class LibraryController : ControllerBase
{

    private readonly ILogger<LibraryController> logger;
    private DBConnect context;

    public LibraryController(DBConnect context, ILogger<LibraryController> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>Получение всех книг</summary>
    /// <response code="200">Получить список книг</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("GetBooks")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks()
    {
        try
        {
            string query = "SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Genres\".name AS genres, \"Publish\".name AS publisher, \"Publish\".city AS city, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_publish = \"Publish\".id";
            var books = context.Database.SqlQueryRaw<CBooks>(query).ToList();
            logger.LogDebug("Рычаг GetBooks() - Вызвать все книги");
            return Ok(books);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом книг. Уровень ошибки в районе SQL-Запроса.  Тип ошибки: " + ex);
        }
    }
    /*
    /// <summary>Получение книги по опционалу</summary>
    /// <response code="200">Получить книгу</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("GetBooks/{count_list},{includes}")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks(int count_list, [FromQuery]string[] includes)
    {
        try
        {
            string query = "";
            string query_where = "";
            int elementCount = includes.Length;

            switch (elementCount)
            {
                case 0:
                    query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Books\".pages AS pages FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id";
                    query_where = $" where \"Books\".pages>={count_list}";
                    query += query_where;
                    break;
                case 1:
                    query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Books\".pages AS pages FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id";
                    query_where = $" where \"Author\".first_name=={includes[0]} && \"Books\".pages>={count_list}";
                    query += query_where;
                    break;
                case 2:
                    query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id";
                    query_where = $" where \"Author\".first_name=={includes[0]} && \"Name_Books\".name=={includes[1]} && \"Books\".pages>={count_list}";
                    query += query_where;
                    break;
                case 3:
                    query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Genres\".name AS genres, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id";
                    query_where = $" where \"Author\".first_name=={includes[0]} && \"Name_Books\".name=={includes[1]} && \"Genres\".name=={includes[2]} && \"Books\".pages>={count_list}";
                    query += query_where;
                    break;
            }
            
            //query_where = $" where \"Name_Books\".name=={name_book}";

            var books = context.Database.SqlQueryRaw<CBooks>(query);
            logger.LogDebug("Рычаг GetBooks(s) - Вызвать все книги по названию");
            return Ok(books);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом книги по его Названию. Уровень ошибки в районе SQL-Запроса.  Тип ошибки: " + ex);
        }
    }
    */
    [HttpGet("GetBooks/{name}")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks(string name)
    {
        try
        {
            Name_Books book = context.Name_Books.FirstOrDefault(p => p.name == name);
            logger.LogDebug("Рычаг GetBooks() - Вызвать все книги");
            return Ok(book.id);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом книг. Уровень ошибки в районе SQL-Запроса.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Получение книги через пагинацию</summary>
    /// <response code="200">Получить книги с ограниченном кол-во</response>
    /// <response code="400">Ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpGet("GetBooks/{limit},{offset}")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks(int limit,int offset)
    {
        try
        {
            string query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Genres\".name AS genres, \"Publish\".name AS publisher, \"Publish\".city AS city, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_publish = \"Publish\".id";
            string query_where = "";
            query_where = $" LIMIT {limit} OFFSET {offset}";

            var books = context.Database.SqlQueryRaw<CBooks>(query+query_where);
            logger.LogDebug("Рычаг GetBooks(s) - Вызвать все книги по названию");
            return Ok(books);
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c выводом книги по его Названию. Уровень ошибки в районе SQL-Запроса.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Создать книгу</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     POST Library/v1/CreateBook
    ///     {
    ///         "last_name": "Дрю",
    ///         "first_name": "Карпишен",
    ///         "patronymic": "",
    ///         "name": "Звездные Войны. Старая Республика. Реван",
    ///         "genres": "Фантастика",
    ///         "publisher": "Эксмо",
    ///         "city": "Москва",
    ///         "pages": 0,
    ///         "description": "История о джедае героя, предателя, завоевателя и спасителя. Окунитесь в путешествие старого джедая, который бродил по космосу, в поисках воспоминаний",
    ///         "years": "2025-10-05T18:31:48.059Z"
    ///     }
    /// </remarks>
    /// <response code="200">Получить имя и роль пользователя</response>
    /// <response code="400">Не найден пользователь (стандарт. случай), либо ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpPost("CreateBook")]
    [Idempotent(cacheTimeInMinutes: 60)]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> PostBook(CBooks modelBook)
    {
        try
        {
            //Формирование данных по таблицам без связей
            Author author = new Author { first_name = modelBook.first_name, last_name = modelBook.last_name, patronymic = modelBook.patronymic };

            Name_Books? name = context.Name_Books.FirstOrDefault(p => p.name == modelBook.name);
            if (name == null)
                name = new Name_Books { name = modelBook.name };

            Genres genres = context.Genres.FirstOrDefault(p => p.name == modelBook.genres);
            Publish publish = context.Publish.FirstOrDefault(p => p.name == modelBook.publisher);
            logger.LogDebug("Рычаг PostBook(CBooks модель) - Вложил данные о авторе, имени, категории и публикации");

            //Распределение несвязных данных по таблицам
            context.Author.AddRange(author);
            context.Name_Books.AddRange(name);
            //context.Genres.AddRange(genres);
            //context.Publish.AddRange(publish);
            context.SaveChanges();
            logger.LogDebug("Рычаг PostBook(CBooks модель) - Записал данные");

            //Формирование данных по таблицам со связями
            Books books = new Books { id_author = author.id, id_name = name.id, id_genres = genres.id, id_publish = publish.id, pages = modelBook.pages, description = modelBook.description, years = modelBook.years };
            logger.LogDebug("Рычаг PostBook(CBooks модель) - Вложил данные о книге (фулл)");

            //Распределение связных данных по таблицам
            context.Books.UpdateRange(books);
            context.SaveChanges();

            return Ok($"Запись книги {name.name} в базу данных");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c добавлением книги в библиотеку. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Обновить книгу</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     PUT Library/v1/UpdateBook
    ///     {
    ///         "last_name": "Дрю",
    ///         "first_name": "Карпишен",
    ///         "patronymic": "",
    ///         "name": "Звездные Войны. Реван",
    ///         "genres": "Фантастика",
    ///         "publisher": "Эксмо",
    ///         "city": "Москва",
    ///         "pages": 0,
    ///         "description": "История о джедае-спасителе.",
    ///         "years": "2025-10-05T18:31:48.059Z"
    ///     }
    /// </remarks>
    /// <response code="200">Получить имя и роль пользователя</response>
    /// <response code="400">Не найден элемент книги (стандарт. случай, уточнение смотрите в состоянии ошибки), либо ошибка (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpPut("UpdateBook")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> UpdateBook(CBooks modelBook)
    {
        try
        {

            //Общее представление старой модели данных. Имя и Фамилия автора являются неизменными. По ним ведется поиск
            string query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Genres\".name AS genres, \"Publish\".name AS publisher, \"Publish\".city AS city, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_publish = \"Publish\".id";
            var book = context.Database.SqlQueryRaw<CBooks>(query).FirstOrDefault(b => b.last_name == modelBook.last_name);

            //Нахождение данных книги по отдельным таблицам
            Author author = context.Author.FirstOrDefault(p => p.last_name == modelBook.last_name);
            Books books_original = context.Books.FirstOrDefault(p => p.id_author == author.id);
            Books books_reject = new Books
            {
                id = books_original.id,
                id_name = books_original.id_name,
                id_genres = books_original.id_genres,
                id_author = books_original.id_author,
                id_publish = books_original.id_publish,
                description = books_original.description,
                pages = books_original.pages,
                years = books_original.years
            };
            //Удаление связей,во избежания ошибок (Данные сохраняются в объекте запроса)
            context.Books.Remove(books_original);
            Name_Books? name;
            Genres? genres;
            Publish? publish;

            if (book.name != modelBook.name)
            {
                logger.LogDebug("Рычаг UpdateBook(CBooks модель) - Name");
                //Удалить старые данные, чтобы не засоряли бд
                Name_Books old_name = context.Name_Books.FirstOrDefault(p => p.name == book.name);
                context.Name_Books.Remove(old_name);
                //Новые данные
                name = new Name_Books {name = modelBook.name };
                context.Name_Books.Add(name);
                context.SaveChanges();
                Name_Books new_name = context.Name_Books.FirstOrDefault(p => p.name == name.name);
                books_reject.id_name = new_name.id;
            }

            if (book.genres != modelBook.genres)
            {
                logger.LogDebug("Рычаг UpdateBook(CBooks модель) - Genres");
                genres = context.Genres.FirstOrDefault(p => p.name == modelBook.genres);
                if (genres != null)
                    books_reject.id_genres = genres.id;
                else
                    logger.LogWarning("Genres оказался пустой");
            }


            if (book.publisher != modelBook.publisher)
            {
                logger.LogDebug("Рычаг UpdateBook(CBooks модель) - Publisher");
                publish = context.Publish.FirstOrDefault(p => p.name == modelBook.publisher);
                if (publish != null)
                    books_reject.id_publish = publish.id;
                else
                    logger.LogWarning("Publish оказался пустой");
            }


            books_reject.description = modelBook.description;
            books_reject.pages = modelBook.pages;
            books_reject.years = modelBook.years;
            logger.LogDebug("Рычаг UpdatePBook(CBooks модель) - Обновление данные");

            //Распределение связных данных по таблицам

            context.Books.Add(books_reject);
            context.SaveChanges();

            return Ok("Обновление прошло успешно");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c обновлением книги в библиотеку. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
    /// <summary>Удалить книгу</summary>
    /// <remarks>
    /// Простой пример данных:
    /// 
    ///     DELETE Library/v1/DeleteBook
    ///     {
    ///         "last_name": "Дрю",
    ///         "first_name": "Карпишен",
    ///         "patronymic": "",
    ///         "name": "Звездные Войны. Старая Республика. Реван",
    ///         "genres": "Фантастика",
    ///         "publisher": "Эксмо",
    ///         "city": "Москва",
    ///         "pages": 0,
    ///         "description": "История о джедае героя, предателя, завоевателя и спасителя. Окунитесь в путешествие старого джедая, который бродил по космосу, в поисках воспоминаний",
    ///         "years": "2025-10-05T18:31:48.059Z"
    ///     }
    /// </remarks>
    /// <response code="200">Удаление аккаунта</response>
    /// <response code="400">Ошибка удаления (смотрите исключение)</response>
    /// <response code="429">Превышен лимит запросов</response>
    [HttpDelete("DeleteBook")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> DeleteBook(CBooks modelBook)
    {
        try
        {
            logger.LogDebug("Рычаг DeleteBook(CBooks модель) - подготовка");
            Books books = context.Books.FirstOrDefault(p => p.description == modelBook.description);
            Author authors = context.Author.FirstOrDefault(p => p.last_name == modelBook.last_name);
            Name_Books name_books = context.Name_Books.FirstOrDefault(p => p.name == modelBook.name);
            logger.LogDebug("Рычаг DeleteBook(CBooks модель) - удаление");
            context.Books.Remove(books);
            context.SaveChanges();
            context.Author.Remove(authors);
            context.SaveChanges(); 
            context.Name_Books.Remove(name_books);
            context.SaveChanges();

            //context.SaveChanges();
            return Ok($"Книга удалена {modelBook.name} из базы данных");
        }
        catch (Exception ex)
        {
            return BadRequest("Произошла ошибка c удалением книги из библиотеку. Уровень ошибки в районе Бекенда.  Тип ошибки: " + ex);
        }
    }
}
