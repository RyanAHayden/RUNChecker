namespace RUNChecker.Options
{
    public class EmailServiceOptions
    {
        public const string EmailService = "EmailService";

        public string MailServer { get; set; } = String.Empty;
        public string Sender { get; set; } = String.Empty;
        public List<string>? Recipients { get; set; }

    }
}