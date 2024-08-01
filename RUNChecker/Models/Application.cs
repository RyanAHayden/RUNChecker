using System;
using System.Collections.Generic;

namespace RUNChecker.Models;

public partial class Application
{
    public int ApplicationId { get; set; }

    public string Name { get; set; } = null!;

    public int AreaId { get; set; }

    public int? CurrentWorkItemCert { get; set; }

    public int? CurrentWorkItemAcc { get; set; }

    public int? CurrentIncident { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<ServiceAccount> ServiceAccounts { get; set; } = new List<ServiceAccount>();
}
