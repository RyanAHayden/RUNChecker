namespace RUNChecker.Options
{
    public class ServiceAccountCheckerOptions
    {
        public const string ServiceAccountChecker = "ServiceAccountChecker";
        public int DaysRemainingToFlag { get; set; } = 28;
        public bool AlwaysCreateBacklog { get; set; } = false;
        public int DefaultPriority { get; set; } = 1;
        public int ExpiringPriority { get; set; } = 2;
        public int ExpiredPriority { get; set; } = 3;
        public bool CheckServiceAccounts { get; set; } = true;
    }
}