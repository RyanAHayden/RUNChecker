namespace RUNChecker.Options
{
    public class CertificateCheckerOptions
    {
        public const string CertificateChecker = "CertificateChecker";

        public int DaysRemainingToFlag { get; set; } = 28;
        public bool AlwaysCreateBacklog { get; set; } = false;
        public int DefaultPriority { get; set; } = 1;
        public int ExpiringPriority { get; set; } = 2;
        public int ExpiredPriority { get; set; } = 3;
        public bool CheckCertificates { get; set; } = true;
    }
}
