﻿using System;
using System.Collections.Generic;

namespace Students.Models;

public partial class Aspnetuserclaim
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

    public virtual Aspnetuser User { get; set; }
}
