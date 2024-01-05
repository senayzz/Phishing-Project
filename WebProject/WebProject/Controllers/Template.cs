using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebProject.Models;

namespace WebProject.Controllers;

public class Template : Controller
{
    private readonly string connectionString = "Server=localhost;Port=5432;Username=erdemkurt;Password=353535;Database=phishing;";
    // GET
    public IActionResult CreateTemplate()
    {
        return View();
    }
    public IActionResult EditTemplate(int id)
    {
        EmailTemplateModel emailTemplate = GetEmailTemplateByIdFromDatabase(id);
        return View(emailTemplate);
    }
    
    public IActionResult SaveTemplate(EmailTemplateModel emailTemplate)
    {
        // Debug.WriteLine("SENAAAAAAAAAA");
        // Debug.WriteLine(emailTemplate.template_content);
        bool isExecuteCorrect = UpdateEmailTemplateInDatabase(emailTemplate);
        if (isExecuteCorrect)
        {
            ViewData["SuccessMessage"]= "Template is updated";
        }
        else
        {
            ViewData["ErrorMessage"] = "Template is not updated";
        }
        return RedirectToAction("EmailTemplates", "Home");
    }
    
    //CreateTemplate function name is create new template
    public IActionResult CreateTemplateEmail(EmailTemplateModel emailTemplate)
    {
        bool isExecuteCorrect = InsertEmailTemplateToDatabase(emailTemplate);
        if (isExecuteCorrect)
        {
            ViewData["SuccessMessage"]= "Template is created";
        }
        else
        {
            ViewData["ErrorMessage"] = "Template is not created";
        }
        return RedirectToAction("EmailTemplates", "Home");
    }
    
    private EmailTemplateModel GetEmailTemplateByIdFromDatabase(int id)
    {
        string selectQuery = "SELECT * FROM email_templates WHERE et_id = @TemplateId";
        EmailTemplateModel emailTemplate = null;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@TemplateId", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        emailTemplate = new EmailTemplateModel
                        {
                            et_id = Convert.ToInt32(reader["et_id"]),
                            template_name = reader["template_name"].ToString(),
                            template_content = reader["template_content"].ToString(),
                            platform_id = Convert.ToInt32(reader["platform_id"]),
                            click_count = Convert.ToInt32(reader["click_count"])
                        };
                    }
                }
            }
        }
        return emailTemplate;
    }
    
    private bool UpdateEmailTemplateInDatabase(EmailTemplateModel emailTemplate)
    {
        Debug.WriteLine("2323232332323");
        Debug.WriteLine(emailTemplate.template_content);
        string updateQuery = "UPDATE email_templates SET template_name = @TemplateName, template_content = @TemplateContent WHERE et_id = @TemplateId";
        var isExecuteCorrect = false;
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@TemplateId", emailTemplate.et_id);
                command.Parameters.AddWithValue("@TemplateName", emailTemplate.template_name);
                command.Parameters.AddWithValue("@TemplateContent", emailTemplate.template_content);
                var query = command.ExecuteNonQuery();
                if (query != -1)
                {
                    isExecuteCorrect = true;
                }
            }  
        }
        return isExecuteCorrect;
    }
    private bool InsertEmailTemplateToDatabase(EmailTemplateModel emailTemplate)
    { 
        string insertQuery =
            "INSERT INTO email_templates (template_name, template_content, platform_id, click_count) VALUES (@TemplateName, @TemplateContent, @PlatformId, @ClickCount)";
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@TemplateName", emailTemplate.template_name);
                command.Parameters.AddWithValue("@TemplateContent", emailTemplate.template_content);
                command.Parameters.AddWithValue("@PlatformId", emailTemplate.platform_id);
                command.Parameters.AddWithValue("@ClickCount", emailTemplate.click_count);
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }



}