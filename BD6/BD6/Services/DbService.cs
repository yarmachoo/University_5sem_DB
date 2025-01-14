using BD6.Entities;
using BD6.Models;
using Npgsql;
using System.Collections.Generic;

namespace BD6.Services
{
    public class DbService
    {
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _contextAccessor;

        public DbService(IConfiguration configuration,
            IHttpContextAccessor contextAccessor)
        {
            _connectionString = configuration.GetValue<string>("ConnectionString")
                ?? throw new ArgumentNullException("Connection is not defined");
            _contextAccessor = contextAccessor;
        }
        public async Task CheckEmployeeIdInSession()
        {

            await using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = "SHOW app.employee_id;";
                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    Console.WriteLine($"Current app.employee_id: {result}");

                    if (result == null || result.ToString() == "")
                    {
                        Console.WriteLine("Error: app.employee_id is not set or is empty.");
                    }
                }

            }

        }
        public async Task<List<Book>> GetAllBooksWithoutDetails()
        {
            var books = new List<Book>();

            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                        SELECT 
                            b.book_id, b.book_name, b.description, b.price, b.assesment, 
                            p.publishing_name, p.date_of_publish, 
                            fap.street, fap.city, fap.country
                        FROM book b
                        LEFT JOIN publishing p ON b.publishing_id = p.publishing_id
                        LEFT JOIN full_address_of_publish fap ON p.place_of_publish_id = fap.full_address_of_publish_id;";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                books.Add(new Book
                                {
                                    BookId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Price = reader.GetDecimal(3),
                                    Assesment = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                    Publishing = new Publishing
                                    {
                                        Name = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        DateOfPublish = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                        PlaceOfPublish = new FullAddressOfPublish
                                        {
                                            Street = reader.IsDBNull(7) ? null : reader.GetString(7),
                                            City = reader.IsDBNull(8) ? null : reader.GetString(8),
                                            Country = reader.IsDBNull(9) ? null : reader.GetString(9)
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return books;
        }
        public async Task<Book> GetBookById(int bookId)
        {
            Book book = null;

            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                        SELECT 
                            b.book_id, b.book_name, b.description, b.price, b.assesment, 
                            p.publishing_name, p.date_of_publish, 
                            fap.street, fap.city, fap.country
                        FROM book b
                        LEFT JOIN publishing p ON b.publishing_id = p.publishing_id
                        LEFT JOIN full_address_of_publish fap ON p.place_of_publish_id = fap.full_address_of_publish_id
                        WHERE b.book_id = @BookId;";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookId", bookId);

                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                book = new Book
                                {
                                    BookId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Price = reader.GetDecimal(3),
                                    Assesment = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                    Publishing = new Publishing
                                    {
                                        Name = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        DateOfPublish = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                        PlaceOfPublish = new FullAddressOfPublish
                                        {
                                            Street = reader.IsDBNull(7) ? null : reader.GetString(7),
                                            City = reader.IsDBNull(8) ? null : reader.GetString(8),
                                            Country = reader.IsDBNull(9) ? null : reader.GetString(9)
                                        }
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return book;
        }
        public async Task<bool> AddBook(Book book)
        {
            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                        INSERT INTO book (book_name, description, price, assesment, publishing_id) 
                        VALUES (@Name, @Description, @Price, @Assesment, @PublishingId);";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", book.Name);
                        cmd.Parameters.AddWithValue("@Description", (object)book.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Price", book.Price);
                        cmd.Parameters.AddWithValue("@Assesment", (object)book.Assesment ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PublishingId", (object)book.Publishing?.PublishingId ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateBook(Book book)
        {
            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                        UPDATE book 
                        SET book_name = @Name, description = @Description, price = @Price, assesment = @Assesment, publishing_id = @PublishingId
                        WHERE book_id = @BookId;";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookId", book.BookId);
                        cmd.Parameters.AddWithValue("@Name", book.Name);
                        cmd.Parameters.AddWithValue("@Description", (object)book.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Price", book.Price);
                        cmd.Parameters.AddWithValue("@Assesment", (object)book.Assesment ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PublishingId", (object)book.Publishing?.PublishingId ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteBook(int bookId)
        {
            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // Удаление связей с авторами
                    var deleteAuthorBookQuery = "DELETE FROM author_book WHERE book_id = @BookId;";
                    await using (var authorBookCmd = new NpgsqlCommand(deleteAuthorBookQuery, conn))
                    {
                        authorBookCmd.Parameters.AddWithValue("@BookId", bookId);
                        await authorBookCmd.ExecuteNonQueryAsync();
                    }

                    // Удаление связей с категориями
                    var deleteCategoryBookQuery = "DELETE FROM category_book WHERE book_id = @BookId;";
                    await using (var categoryBookCmd = new NpgsqlCommand(deleteCategoryBookQuery, conn))
                    {
                        categoryBookCmd.Parameters.AddWithValue("@BookId", bookId);
                        await categoryBookCmd.ExecuteNonQueryAsync();
                    }

                    // Удаление книги
                    var deleteBookQuery = "DELETE FROM book WHERE book_id = @BookId;";
                    await using (var bookCmd = new NpgsqlCommand(deleteBookQuery, conn))
                    {
                        bookCmd.Parameters.AddWithValue("@BookId", bookId);
                        await bookCmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Publishing>> GetAllPublishingsAsync()
        {
            var publishings = new List<Publishing>();

            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                SELECT 
                    p.publishing_id, p.publishing_name, p.date_of_publish, 
                    fap.full_address_of_publish_id, fap.street, fap.home, fap.corpus, fap.city, fap.country, fap.apartment_number
                FROM publishing p
                LEFT JOIN full_address_of_publish fap 
                    ON p.place_of_publish_id = fap.full_address_of_publish_id;";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                publishings.Add(new Publishing
                                {
                                    PublishingId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    DateOfPublish = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                    PlaceOfPublish = reader.IsDBNull(3) ? null : new FullAddressOfPublish
                                    {
                                        FullAddressOfPublishId = reader.GetInt32(3),
                                        Street = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        Home = reader.GetInt32(5),
                                        Corpus = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                        City = reader.IsDBNull(7) ? null : reader.GetString(7),
                                        Country = reader.IsDBNull(8) ? null : reader.GetString(8),
                                        ApartmentNumber = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9)
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return publishings;
        }
        public async Task<List<Author>> GetAllAuthorsAsync()
        {
            var authors = new List<Author>();

            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = "SELECT author_id, first_name, last_name, date_of_birth, city, country FROM author;";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                authors.Add(new Author
                                {
                                    AuthorId = reader.GetInt32(0),
                                    FirstName = reader.GetString(1),
                                    LastName = reader.GetString(2),
                                    DateOfBirth = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                    City = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Country = reader.IsDBNull(5) ? null : reader.GetString(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return authors;
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = new List<Category>();

            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // Исправлено имя столбца на "category"
                    var query = "SELECT category_id, category FROM category;";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                categories.Add(new Category
                                {
                                    CategoryId = reader.GetInt32(0),
                                    Name = reader.GetString(1) // Это соответствует столбцу "category"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return categories;
        }

        public async Task<bool> AddBookWithDetails(Book book, List<int> authorIds, List<int> categoryIds)
        {
            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // Вставка книги
                    var insertBookQuery = @"
                INSERT INTO book (book_id, book_name, description, price, assesment, publishing_id) 
                VALUES (@BookId, @Name, @Description, @Price, @Assesment, @PublishingId) 
                RETURNING book_id;";

                    int bookId;

                    await using (var bookCmd = new NpgsqlCommand(insertBookQuery, conn))
                    {
                        bookId = 13;
                        bookCmd.Parameters.AddWithValue("@BookId", bookId);
                        bookCmd.Parameters.AddWithValue("@Name", book.Name);
                        bookCmd.Parameters.AddWithValue("@Description", (object)book.Description ?? DBNull.Value);
                        bookCmd.Parameters.AddWithValue("@Price", book.Price);
                        bookCmd.Parameters.AddWithValue("@Assesment", (object)book.Assesment ?? DBNull.Value);
                        bookCmd.Parameters.AddWithValue("@PublishingId", (object)book.Publishing?.PublishingId ?? DBNull.Value);
                        await bookCmd.ExecuteNonQueryAsync();
                    }

                    // Вставка связей с авторами
                    foreach (var authorId in authorIds)
                    {
                        var insertAuthorBookQuery = "INSERT INTO author_book (author_id, book_id, author_book_id) VALUES (@AuthorId, @BookId, @AuthorBookId);";
                        await using (var authorBookCmd = new NpgsqlCommand(insertAuthorBookQuery, conn))
                        {
                            authorBookCmd.Parameters.AddWithValue("@AuthorId", authorId);
                            authorBookCmd.Parameters.AddWithValue("@BookId", bookId);
                            authorBookCmd.Parameters.AddWithValue("@AuthorBookId", bookId);
                            await authorBookCmd.ExecuteNonQueryAsync();
                        }
                    }

                    // Вставка связей с категориями
                    foreach (var categoryId in categoryIds)
                    {
                        var insertCategoryBookQuery = "INSERT INTO category_book (category_book_id, category_id, book_id) VALUES (@CategoryBookId, @CategoryId, @BookId);";
                        await using (var categoryBookCmd = new NpgsqlCommand(insertCategoryBookQuery, conn))
                        {
                            categoryBookCmd.Parameters.AddWithValue("@CategoryId", categoryId);
                            categoryBookCmd.Parameters.AddWithValue("@BookId", bookId);
                            categoryBookCmd.Parameters.AddWithValue("@CategoryBookId", bookId);
                            await categoryBookCmd.ExecuteNonQueryAsync();
                        }
                    }

                    var insertActionQuery = "CALL insert_data_into_action(@ActionId, @ActionMessage);";

                    await using (var actionCmd = new NpgsqlCommand(insertActionQuery, conn))
                    {
                        actionCmd.Parameters.AddWithValue("@ActionId", 7); // ActionId
                        actionCmd.Parameters.AddWithValue("@ActionMessage", "Попытка создания книги");
                        await actionCmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }



        public async Task<List<Book>> GetAllBooks()
        {
            var books = new List<Book>();

            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
SELECT 
    b.book_id, 
    b.book_name, 
    b.description, 
    b.price, 
    b.assesment, 
    p.publishing_name, 
    p.date_of_publish, 
    fap.street, 
    fap.city, 
    fap.country,
    STRING_AGG(DISTINCT CONCAT(a.first_name, ' ', a.last_name), ', ') AS authors,
    STRING_AGG(DISTINCT c.category, ', ') AS categories
FROM book b
LEFT JOIN publishing p ON b.publishing_id = p.publishing_id
LEFT JOIN full_address_of_publish fap ON p.place_of_publish_id = fap.full_address_of_publish_id
LEFT JOIN author_book ab ON b.book_id = ab.book_id
LEFT JOIN author a ON ab.author_id = a.author_id
LEFT JOIN category_book cb ON b.book_id = cb.book_id
LEFT JOIN category c ON cb.category_id = c.category_id
GROUP BY 
    b.book_id, 
    b.book_name, 
    b.description, 
    b.price, 
    b.assesment, 
    p.publishing_name, 
    p.date_of_publish, 
    fap.street, 
    fap.city, 
    fap.country;

";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                books.Add(new Book
                                {
                                    BookId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Price = reader.GetDecimal(3),
                                    Assesment = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                    Publishing = new Publishing
                                    {
                                        Name = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        DateOfPublish = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                        PlaceOfPublish = new FullAddressOfPublish
                                        {
                                            Street = reader.IsDBNull(7) ? null : reader.GetString(7),
                                            City = reader.IsDBNull(8) ? null : reader.GetString(8),
                                            Country = reader.IsDBNull(9) ? null : reader.GetString(9)
                                        }
                                    },
                                    Authors = reader.IsDBNull(10) ? null : reader.GetString(10).Split(", ").ToList(),
                                    Categories = reader.IsDBNull(11) ? null : reader.GetString(11).Split(", ").ToList()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return books;
        }
        public async Task<List<Feedback>> GetFeedbacksByBookIdAsync(int bookId)
        {
            var feedbacks = new List<Feedback>();

            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
SELECT 
    f.feedback_id, 
    f.user_id, 
    u.email, 
    f.book_id, 
    f.assesment
FROM feedback f
INNER JOIN ""USER"" u ON f.user_id = u.user_id
WHERE f.book_id = @BookId
ORDER BY f.feedback_id DESC;
";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookId", bookId);

                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                feedbacks.Add(new Feedback
                                {
                                    FeedbackId = reader.GetInt32(0),
                                    UserId = reader.GetInt32(1),
                                    User = new User
                                    {
                                        UserId = reader.GetInt32(1),
                                        Email = reader.GetString(2)  // Assuming 'username' is a property of 'User'
                                    },
                                    BookId = reader.GetInt32(3),
                                    Assesment = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return feedbacks;
        }


        public async Task<bool> RegisterUser(RegisterModel model)
        {
            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // Проверка уникальности email
                    var checkEmailQuery = "SELECT COUNT(*) FROM \"USER\" WHERE email = @Email";
                    await using (var cmd = new NpgsqlCommand(checkEmailQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        var count = (long)await cmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            return false; // Email уже занят
                        }
                    }

                    // Создание пользователя
                    var insertUserQuery = @"
                    INSERT INTO ""USER"" (user_id, email, password, roleId, user_profile_id) 
                        VALUES(@UserId, @Email, @Password, @RoleId, (SELECT user_profile_id FROM user_profile WHERE phone_number = @PhoneNumber LIMIT 1)); ";
                    int userId;

                    await using (var userCmd = new NpgsqlCommand(insertUserQuery, conn))
                    {
                        userId = 13;
                        userCmd.Parameters.AddWithValue("@Email", model.Email);
                        userCmd.Parameters.AddWithValue("@Password", model.Password); // Для простоты пароли не хэшируются в данном примере
                        userCmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                        userCmd.Parameters.AddWithValue("@RoleId", 1); // По умолчанию роль 'User'
                        userCmd.Parameters.AddWithValue("@UserId", userId); // По умолчанию роль 'User'
                        await userCmd.ExecuteScalarAsync();
                    }

                    // Создание профиля пользователя
                    var insertProfileQuery = @"
                    INSERT INTO USER_PROFILE (user_profile_id, phone_number, user_id)
                    VALUES (@userProfileId, @PhoneNumber, @UserId)";
                    await using (var profileCmd = new NpgsqlCommand(insertProfileQuery, conn))
                    {
                        profileCmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                        profileCmd.Parameters.AddWithValue("@UserId", userId);
                        profileCmd.Parameters.AddWithValue("@userProfileId", userId);
                        await profileCmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<User> AuthenticateUser(LoginModel model)
        {
            try
            {
                await using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                    SELECT u.user_id, u.email, u.password, r.role_name, up.user_profile_id, up.phone_number, up.date_of_registration
                    FROM ""USER"" u
                        JOIN role_of_user r ON u.roleid = r.role_of_user_id
                        JOIN user_profile up ON u.user_profile_id = up.user_profile_id
                        WHERE u.email = @Email AND u.password = @Password
                        LIMIT 1; ";

                    await using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@Password", model.Password); // Для простоты пароли не хэшируются в данном примере

                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    UserId = reader.GetInt32(0),
                                    Email = reader.GetString(1),
                                    Password = reader.GetString(2),
                                    Role = new RoleOfUser
                                    {
                                        RoleName = reader.GetString(3)
                                    },
                                    UserProfile = new UserProfile
                                    {
                                        UserProfileId = reader.GetInt32(4),
                                        PhoneNumber = reader.GetString(5),
                                        DateOfRegistration = reader.GetDateTime(6)
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null;
        }


        public async Task AddBookToOrderAsync(int userId, int bookId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Проверяем, существует ли у пользователя активный заказ
                var checkOrderQuery = @"
                SELECT order_id FROM ""ORDER""
                WHERE user_id = @UserId AND order_date IS NULL;";

                int? orderId = await ExecuteScalarAsync<int?>(conn, checkOrderQuery, new { UserId = userId });

                if (orderId == null)
                {
                    // Если нет, создаем новый заказ
                    var createOrderQuery = @"
                    INSERT INTO ""ORDER"" (user_id, order_date, cost)
                    VALUES (@UserId, @OrderDate, @Cost)
                    RETURNING order_id;";

                    orderId = await ExecuteScalarAsync<int>(conn, createOrderQuery, new { UserId = userId, OrderDate = DateTime.Now, Cost = 0 });
                }

                // Добавляем книгу в заказ
                var addBookToOrderQuery = @"
                INSERT INTO order_book (book_id, order_id)
                VALUES (@BookId, @OrderId);";

                await ExecuteNonQueryAsync(conn, addBookToOrderQuery, new { BookId = bookId, OrderId = orderId });
            }
        }

        private async Task<T> ExecuteScalarAsync<T>(NpgsqlConnection conn, string query, object parameters)
        {
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                foreach (var param in parameters.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue(param.Name.Replace("_", ""), param.GetValue(parameters));
                }
                return (T)await cmd.ExecuteScalarAsync();
            }
        }

        private async Task ExecuteNonQueryAsync(NpgsqlConnection conn, string query, object parameters)
        {
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                foreach (var param in parameters.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue(param.Name.Replace("_", ""), param.GetValue(parameters));
                }
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = new List<Order>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
            SELECT o.order_id, o.user_id, o.full_address_of_order_id, o.discount_id, o.order_date, o.cost,
                   fa.street, fa.home, fa.corpus, fa.city, fa.country, fa.apartment_number,
                   d.code, d.percents, d.start_date, d.end_date, d.is_active
            FROM ""ORDER"" o
            LEFT JOIN full_address_of_order fa ON o.full_address_of_order_id = fa.full_address_of_order_id
            LEFT JOIN discount d ON o.discount_id = d.discount_id
            WHERE o.user_id = @UserId;
        ";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var order = new Order
                            {
                                OrderId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                FullAddressOfOrder = new FullAddressOfOrder
                                {
                                    FullAddressOfOrderId = reader.GetInt32(2),
                                    Street = reader.GetString(3),
                                    Home = reader.GetInt32(4),
                                    Corpus = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                    City = reader.GetString(6),
                                    Country = reader.GetString(7),
                                    ApartmentNumber = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8)
                                },
                                Discount = new Discount
                                {
                                    DiscountId = reader.GetInt32(9),
                                    Code = reader.GetString(10),
                                    Percents = reader.IsDBNull(11) ? (decimal?)null : reader.GetDecimal(11),
                                    StartDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
                                    EndDate = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13),
                                    IsActive = reader.IsDBNull(14) ? (bool?)null : reader.GetBoolean(14)
                                },
                                OrderDate = reader.GetDateTime(15),
                                Cost = reader.GetDecimal(16)
                            };

                            // Получаем книги, которые входят в текущий заказ
                            var booksQuery = @"
                        SELECT b.book_id, b.book_name, b.price
                        FROM order_book ob
                        JOIN book b ON ob.book_id = b.book_id
                        WHERE ob.order_id = @OrderId;
                    ";

                            using (var booksCmd = new NpgsqlCommand(booksQuery, conn))
                            {
                                booksCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                                await using (var booksReader = await booksCmd.ExecuteReaderAsync())
                                {
                                    var books = new List<Book>();
                                    while (await booksReader.ReadAsync())
                                    {
                                        books.Add(new Book
                                        {
                                            BookId = booksReader.GetInt32(0),
                                            Name = booksReader.GetString(1),
                                            Price = booksReader.GetDecimal(2)
                                        });
                                    }
                                    order.Books = books;
                                }
                            }

                            orders.Add(order);
                        }
                    }
                }
            }

            return orders;
        }

        public async Task AddAuthorAsync(Author author)
        {
            const string query = @"
                INSERT INTO author (author_id, first_name, last_name, date_of_birth, city, country)
                VALUES (@author_id, @FirstName, @LastName, @DateOfBirth, @City, @Country)";

            await using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    int author_id = 12;

                    cmd.Parameters.AddWithValue("@FirstName", author.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", author.LastName);
                    cmd.Parameters.AddWithValue("@DateOfBirth", (object?)author.DateOfBirth ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", author.City);
                    cmd.Parameters.AddWithValue("@Country", author.Country);
                    cmd.Parameters.AddWithValue("@author_id", author_id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAuthorAsync(int authorId)
        {
            const string query = "DELETE FROM author WHERE author_id = @AuthorId";

            await using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AuthorId", authorId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

    }
}