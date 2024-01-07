namespace WebProject.Models;

public class SendEmailViewModel
{
    public List<EmailTemplateModel> EmailTemplates { get; set; }
    public string et_id { get; set; }
    public List<string> mailto { get; set; }
}