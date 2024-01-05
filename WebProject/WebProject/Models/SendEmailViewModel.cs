namespace WebProject.Models;

public class SendEmailViewModel
{
    public List<UserModel> Users
    {
        get; set;
    }

    public List<EmailTemplateModel> EmailTemplates
    {
        get; set;
    }
}