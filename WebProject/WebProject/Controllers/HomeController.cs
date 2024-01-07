using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebProject.Models;
using WebProject.Functions;

namespace WebProject.Controllers;

public class HomeController : Controller
{
    // private readonly string connectionString = "Server=localhost;Port=5432;Username=erdemkurt;Password=353535;Database=phishing;";
    private readonly string connectionString = "Server=localhost;Port=49152;Username=senayilmaz;Password=2002;Database=webprojectdb;";
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
        viewModel.EmailTemplates = emailTemplates;
        return View(viewModel);
    }
    
    [HttpPost]
    public IActionResult SendEmail(SendEmailViewModel data)
    {
        SendEmailViewModel viewModel = new SendEmailViewModel();
        
        List<EmailTemplateModel> emailTemplates = GetEmailTemplatesFromDatabase();
        var template_id = data.et_id;
        var emails = data.mailto;
        var selectedTemplate = emailTemplates[0]; // default
        Debug.WriteLine("SENAAAAAA");
        for (int i = 0; i < data.mailto.Count; i++)
        {
            InsertSendedEmailsToDatabase(data.mailto[i], int.Parse(template_id), DateTime.Now);
            Debug.WriteLine(data.mailto[i]);
        }
        Debug.WriteLine(data.et_id);
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
        List<SendMail> sentuserListmails = new List<SendMail>();
        sentuserListmails = GetSentEmailFromDatabase();
        Dictionary<int, int> platformCounts = GetPlatformCountsFromDatabase();
        ViewBag.PlatformCounts = platformCounts;
        List<string> templateNames = new List<string>();
        for (int i = 0; i < sentuserListmails.Count; i++)
        {
            templateNames.Add(GetEmailTemplateName(sentuserListmails[i].et_id));
        }
        ViewBag.TemplateNames = templateNames;
        ViewBag.TotalEmails = sentuserListmails.Count;
        ViewBag.TotalUsers = GetUserFromDatebase().Count;
        ViewBag.PlatformCounts = GetPlatformsFromDatabase();
        return View(sentuserListmails);
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
    
    public List<SendMail> GetSentEmailFromDatabase()
    {
        string selectQuery = "SELECT * FROM sent_emails";
        List<SendMail> sentEmails = new List<SendMail>();
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SendMail sentEmail = new SendMail
                        {
                            et_id = Convert.ToInt32(reader["et_id"]),
                            mailto = reader["mailto"].ToString(),
                            date_time = Convert.ToDateTime(reader["date_time"])
                        };
                        sentEmails.Add(sentEmail);
                    }
                }
            }
        }
        return sentEmails;
    }
    private void InsertSendedEmailsToDatabase(string s, int parse, DateTime now)
    {
        string insertQuery = "INSERT INTO sent_emails(et_id, mailto, date_time) VALUES(@TemplateId, @MailTo, @Date)";
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@TemplateId", parse);
                command.Parameters.AddWithValue("@MailTo", s);
                command.Parameters.AddWithValue("@Date", DateTime.Now);
                command.ExecuteNonQuery();
            }
        }
    }

    public string GetEmailTemplateName(int et_id)
    {
        string selectQuery = "SELECT template_name FROM email_templates WHERE et_id = @TemplateId";
        string templateName = "";
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@TemplateId", et_id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        templateName = reader["template_name"].ToString();
                    }
                }
            }
        }
        return templateName;
    }  
    public Dictionary<int, int> GetPlatformsFromDatabase()
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
                            if (!platformCounts.ContainsKey(platformId))
                            {
                                platformCounts.Add(platformId, count);
                            }
                            else
                            {
                                platformCounts[platformId] += count;
                            }
                        }
                    }
                }
            }
        }
        return platformCounts;
    }
}