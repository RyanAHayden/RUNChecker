using System;
using System.Collections.Generic;

namespace RUNChecker.Models;

public partial class AppEnvironment
{
    public int AppEnvironmentId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<ServiceAccount> ServiceAccounts { get; set; } = new List<ServiceAccount>();
}
