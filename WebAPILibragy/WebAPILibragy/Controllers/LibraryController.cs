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

    /// <summary>��������� ���� ����</summary>
    /// <response code="200">�������� ������ ����</response>
    /// <response code="400">������ (�������� ����������)</response>
    /// <response code="429">�������� ����� ��������</response>
    [HttpGet("GetBooks")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks()
    {
        try
        {
            string query = "SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Genres\".name AS genres, \"Publish\".name AS publisher, \"Publish\".city AS city, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_publish = \"Publish\".id";
            var books = context.Database.SqlQueryRaw<CBooks>(query).ToList();
            logger.LogDebug("����� GetBooks() - ������� ��� �����");
            return Ok(books);
        }
        catch (Exception ex)
        {
            return BadRequest("��������� ������ c ������� ����. ������� ������ � ������ SQL-�������.  ��� ������: " + ex);
        }
    }
    /// <summary>��������� ����� �� ���������</summary>
    /// <response code="200">�������� �����</response>
    /// <response code="400">������ (�������� ����������)</response>
    /// <response code="429">�������� ����� ��������</response>
    [HttpGet("GetBooks/{count_list},{includes}")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks([FromBody] int count_list, [FromForm]string[] includes)
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
            logger.LogDebug("����� GetBooks(s) - ������� ��� ����� �� ��������");
            return Ok(books);
        }
        catch (Exception ex)
        {
            return BadRequest("��������� ������ c ������� ����� �� ��� ��������. ������� ������ � ������ SQL-�������.  ��� ������: " + ex);
        }
    }
    [HttpGet("GetBooks/{name}")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks(string name)
    {
        try
        {
            Name_Books book = context.Name_Books.FirstOrDefault(p => p.name == name);
            logger.LogDebug("����� GetBooks() - ������� ��� �����");
            return Ok(book.id);
        }
        catch (Exception ex)
        {
            return BadRequest("��������� ������ c ������� ����. ������� ������ � ������ SQL-�������.  ��� ������: " + ex);
        }
    }
    /// <summary>��������� ����� ����� ���������</summary>
    /// <response code="200">�������� ����� � ������������ ���-��</response>
    /// <response code="400">������ (�������� ����������)</response>
    /// <response code="429">�������� ����� ��������</response>
    [HttpGet("GetBooks/{limit},{offset}")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> GetBooks(int limit,int offset)
    {
        try
        {
            string query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Genres\".name AS genres, \"Publish\".name AS publisher, \"Publish\".city AS city, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_publish = \"Publish\".id";
            string query_where = "";
            query_where = $"LIMIT {limit} OFFSET {offset}";

            var books = context.Database.SqlQueryRaw<CBooks>(query);
            logger.LogDebug("����� GetBooks(s) - ������� ��� ����� �� ��������");
            return Ok(books);
        }
        catch (Exception ex)
        {
            return BadRequest("��������� ������ c ������� ����� �� ��� ��������. ������� ������ � ������ SQL-�������.  ��� ������: " + ex);
        }
    }
    /// <summary>������� �����</summary>
    /// <remarks>
    /// ������� ������ ������:
    /// 
    ///     POST Library/v1/CreateBook
    ///     {
    ///         "last_name": "���",
    ///         "first_name": "��������",
    ///         "patronymic": "",
    ///         "name": "�������� �����. ������ ����������. �����",
    ///         "genres": "����������",
    ///         "publisher": "�����",
    ///         "city": "������",
    ///         "pages": 0,
    ///         "description": "������� � ������ �����, ���������, ����������� � ���������. ��������� � ����������� ������� ������, ������� ������ �� �������, � ������� ������������",
    ///         "years": "2025-10-05T18:31:48.059Z"
    ///     }
    /// </remarks>
    /// <response code="200">�������� ��� � ���� ������������</response>
    /// <response code="400">�� ������ ������������ (��������. ������), ���� ������ (�������� ����������)</response>
    /// <response code="429">�������� ����� ��������</response>
    [HttpPost("CreateBook")]
    [Idempotent(cacheTimeInMinutes: 60)]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> PostBook(CBooks modelBook)
    {
        try
        {
            //������������ ������ �� �������� ��� ������
            Author author = new Author { first_name = modelBook.first_name, last_name = modelBook.last_name, patronymic = modelBook.patronymic };

            Name_Books? name = context.Name_Books.FirstOrDefault(p => p.name == modelBook.name);
            if (name == null)
                name = new Name_Books { name = modelBook.name };

            Genres genres = context.Genres.FirstOrDefault(p => p.name == modelBook.genres);
            Publish publish = context.Publish.FirstOrDefault(p => p.name == modelBook.publisher);
            logger.LogDebug("����� PostBook(CBooks ������) - ������ ������ � ������, �����, ��������� � ����������");

            //������������� ��������� ������ �� ��������
            context.Author.AddRange(author);
            context.Name_Books.AddRange(name);
            //context.Genres.AddRange(genres);
            //context.Publish.AddRange(publish);
            context.SaveChanges();
            logger.LogDebug("����� PostBook(CBooks ������) - ������� ������");

            //������������ ������ �� �������� �� �������
            Books books = new Books { id_author = author.id, id_name = name.id, id_genres = genres.id, id_publish = publish.id, pages = modelBook.pages, description = modelBook.description, years = modelBook.years };
            logger.LogDebug("����� PostBook(CBooks ������) - ������ ������ � ����� (����)");

            //������������� ������� ������ �� ��������
            context.Books.UpdateRange(books);
            context.SaveChanges();

            return Ok($"������ ����� {name.name} � ���� ������");
        }
        catch (Exception ex)
        {
            return BadRequest("��������� ������ c ����������� ����� � ����������. ������� ������ � ������ �������.  ��� ������: " + ex);
        }
    }
    /// <summary>�������� �����</summary>
    /// <remarks>
    /// ������� ������ ������:
    /// 
    ///     PUT Library/v1/UpdateBook
    ///     {
    ///         "last_name": "���",
    ///         "first_name": "��������",
    ///         "patronymic": "",
    ///         "name": "�������� �����. �����",
    ///         "genres": "����������",
    ///         "publisher": "�����",
    ///         "city": "������",
    ///         "pages": 0,
    ///         "description": "������� � ������-���������.",
    ///         "years": "2025-10-05T18:31:48.059Z"
    ///     }
    /// </remarks>
    /// <response code="200">�������� ��� � ���� ������������</response>
    /// <response code="400">�� ������ ������� ����� (��������. ������, ��������� �������� � ��������� ������), ���� ������ (�������� ����������)</response>
    /// <response code="429">�������� ����� ��������</response>
    [HttpPut("UpdateBook")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> UpdateBook(CBooks modelBook)
    {
        try
        {

            //����� ������������� ������ ������ ������. ��� � ������� ������ �������� �����������. �� ��� ������� �����
            string query = $"SELECT \"Author\".last_name AS last_name, \"Author\".first_name AS first_name, \"Author\".patronymic AS patronymic, \"Name_Books\".name AS name, \"Genres\".name AS genres, \"Publish\".name AS publisher, \"Publish\".city AS city, \"Books\".pages AS pages, \"Books\".description AS description, \"Books\".years AS years FROM \"Books\" JOIN \"Author\" ON \"Books\".id_author = \"Author\".id JOIN \"Name_Books\" ON \"Books\".id_name = \"Name_Books\".id JOIN \"Genres\" ON \"Books\".id_genres = \"Genres\".id JOIN \"Publish\" ON \"Books\".id_publish = \"Publish\".id";
            var book = context.Database.SqlQueryRaw<CBooks>(query).FirstOrDefault(b => b.last_name == modelBook.last_name);

            //���������� ������ ����� �� ��������� ��������
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
            //�������� ������,�� ��������� ������ (������ ����������� � ������� �������)
            context.Books.Remove(books_original);
            Name_Books? name;
            Genres? genres;
            Publish? publish;

            if (book.name != modelBook.name)
            {
                logger.LogDebug("����� UpdateBook(CBooks ������) - Name");
                //������� ������ ������, ����� �� �������� ��
                Name_Books old_name = context.Name_Books.FirstOrDefault(p => p.name == book.name);
                context.Name_Books.Remove(old_name);
                //����� ������
                name = new Name_Books {name = modelBook.name };
                context.Name_Books.Add(name);
                context.SaveChanges();
                Name_Books new_name = context.Name_Books.FirstOrDefault(p => p.name == name.name);
                books_reject.id_name = new_name.id;
            }

            if (book.genres != modelBook.genres)
            {
                logger.LogDebug("����� UpdateBook(CBooks ������) - Genres");
                genres = context.Genres.FirstOrDefault(p => p.name == modelBook.genres);
                if (genres != null)
                    books_reject.id_genres = genres.id;
                else
                    logger.LogWarning("Genres �������� ������");
            }


            if (book.publisher != modelBook.publisher)
            {
                logger.LogDebug("����� UpdateBook(CBooks ������) - Publisher");
                publish = context.Publish.FirstOrDefault(p => p.name == modelBook.publisher);
                if (publish != null)
                    books_reject.id_publish = publish.id;
                else
                    logger.LogWarning("Publish �������� ������");
            }


            books_reject.description = modelBook.description;
            books_reject.pages = modelBook.pages;
            books_reject.years = modelBook.years;
            logger.LogDebug("����� UpdatePBook(CBooks ������) - ���������� ������");

            //������������� ������� ������ �� ��������

            context.Books.Add(books_reject);
            context.SaveChanges();

            return Ok("���������� ������ �������");
        }
        catch (Exception ex)
        {
            return BadRequest("��������� ������ c ����������� ����� � ����������. ������� ������ � ������ �������.  ��� ������: " + ex);
        }
    }
    /// <summary>������� �����</summary>
    /// <remarks>
    /// ������� ������ ������:
    /// 
    ///     DELETE Library/v1/DeleteBook
    ///     {
    ///         "last_name": "���",
    ///         "first_name": "��������",
    ///         "patronymic": "",
    ///         "name": "�������� �����. ������ ����������. �����",
    ///         "genres": "����������",
    ///         "publisher": "�����",
    ///         "city": "������",
    ///         "pages": 0,
    ///         "description": "������� � ������ �����, ���������, ����������� � ���������. ��������� � ����������� ������� ������, ������� ������ �� �������, � ������� ������������",
    ///         "years": "2025-10-05T18:31:48.059Z"
    ///     }
    /// </remarks>
    /// <response code="200">�������� ��������</response>
    /// <response code="400">������ �������� (�������� ����������)</response>
    /// <response code="429">�������� ����� ��������</response>
    [HttpDelete("DeleteBook")]
    [Authorize(Roles = "admin, Booking")]
    public async Task<IActionResult> DeleteBook(CBooks modelBook)
    {
        try
        {
            logger.LogDebug("����� DeleteBook(CBooks ������) - ����������");
            Books books = context.Books.FirstOrDefault(p => p.description == modelBook.description);
            Author authors = context.Author.FirstOrDefault(p => p.last_name == modelBook.last_name);
            Name_Books name_books = context.Name_Books.FirstOrDefault(p => p.name == modelBook.name);
            logger.LogDebug("����� DeleteBook(CBooks ������) - ��������");
            context.Books.Remove(books);
            context.SaveChanges();
            context.Author.Remove(authors);
            context.SaveChanges(); 
            context.Name_Books.Remove(name_books);
            context.SaveChanges();

            //context.SaveChanges();
            return Ok($"����� ������� {modelBook.name} �� ���� ������");
        }
        catch (Exception ex)
        {
            return BadRequest("��������� ������ c ��������� ����� �� ����������. ������� ������ � ������ �������.  ��� ������: " + ex);
        }
    }
}
