namespace WebProject.Models;

public class EmailTemplateModel
{
    public int et_id { get; set; }
    public string template_name { get; set; }
    public string template_content { get; set; }
    public int platform_id { get; set; }
    public int click_count { get; set; }
}