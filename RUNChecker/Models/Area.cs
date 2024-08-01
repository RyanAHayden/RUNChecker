using System;
using System.Collections.Generic;

namespace RUNChecker.Models;

public partial class Area
{
    public int AreaId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? AreaName { get; set; }

    public string? IterationPath { get; set; }

    public string BacklogTeam { get; set; } = null!;
}
