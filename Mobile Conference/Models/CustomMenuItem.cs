namespace MobileConference.Models
{
    /// <summary>
    /// Link to some page
    /// </summary>
    public class CustomMenuItem
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public object Param { get; set; }
    }
}