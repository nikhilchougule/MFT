using System;
using System.Collections.Generic;

namespace Students.Models;

public partial class Aspnetrole
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public string ConcurrencyStamp { get; set; }

    public virtual ICollection<Aspnetroleclaim> Aspnetroleclaims { get; set; } = new List<Aspnetroleclaim>();

    public virtual ICollection<Aspnetuser> Users { get; set; } = new List<Aspnetuser>();
}
