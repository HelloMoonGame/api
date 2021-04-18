namespace AuthenticationApi.Models.Email
{
    public abstract class EmailModel
    {
        public abstract string ViewName { get; }
        public abstract string Subject { get; }
    }
}
