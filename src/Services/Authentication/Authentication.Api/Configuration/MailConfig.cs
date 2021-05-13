namespace Authentication.Api.Configuration
{
    public class MailConfig
    {
        public string FromMailAddress { get; set; }
        public string FromName { get; set; }
        
        public DeliveryMethod DeliveryMethod { get; set; }
        public string PickupDirectoryLocation { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUserName { get; set; }
        public string SmtpPassword { get; set; }
    }
    
    public enum DeliveryMethod
    {
        Network,
        SpecifiedPickupDirectory
    }
}
