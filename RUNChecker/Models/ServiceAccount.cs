using System;
using System.Collections.Generic;

namespace RUNChecker.Models;

public partial class ServiceAccount
{
    public int ServiceAccountId { get; set; }

    public int ApplicationId { get; set; }

    public int AppEnvironmentId { get; set; }

    public bool? Enabled { get; set; }

    public string AccountName { get; set; } = null!;

    public int DaysToExpire { get; set; }

    public string? CyberArkSafe { get; set; }

    public DateTimeOffset? CurrentExpiresOn { get; set; }

    public DateTimeOffset? LastLogon { get; set; }

    public DateTimeOffset? LastPasswordSet { get; set; }

    public string? EmployeeType { get; set; }

    public DateTimeOffset? LastCheckedOn { get; set; }

    public string? ErrorMessage { get; set; }

    public bool? Error { get; set; }

    public bool? Expired { get; set; }

    public bool? Expiring { get; set; }

    public virtual AppEnvironment AppEnvironment { get; set; } = null!;

    public virtual Application Application { get; set; } = null!;
}
