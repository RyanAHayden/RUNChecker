namespace RUNChecker
{
    public class CertificateProperties
    {
        public string HostName { get; set; } = string.Empty;
        public string CurrentSubject { get; set; } = string.Empty;
        public DateTime CurrentExpiresOn { get; set; }
        public string CurrentThumbprint { get; set; } = string.Empty;
        public string CurrentProtocol { get; set; } = string.Empty;
        public DateTimeOffset? CurrentIssueOn { get; set; }
        public string ProjectName {  get; set; } = string.Empty;
        public string AreaName {  get; set; } = string.Empty;

    }
}
