using System;
using System.Collections.Generic;

namespace RUNChecker.Models;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public int ApplicationId { get; set; }

    public int AppEnvironmentId { get; set; }

    public string HostName { get; set; } = null!;

    public string? CurrentSubject { get; set; }

    public string? CurrentSans { get; set; }

    public DateTimeOffset? CurrentIssueOn { get; set; }

    public DateTimeOffset? CurrentExpiresOn { get; set; }

    public string? CurrentThumbprint { get; set; }

    public string? CurrentProtocol { get; set; }

    public DateTimeOffset? LastCheckedOn { get; set; }

    public string? ErrorMessage { get; set; }

    public bool? Error { get; set; }

    public bool? Expired { get; set; }

    public bool? Expiring { get; set; }

    public virtual AppEnvironment AppEnvironment { get; set; } = null!;

    public virtual Application Application { get; set; } = null!;
}
