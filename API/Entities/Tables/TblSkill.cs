using System;
using System.Collections.Generic;

namespace API.Entities.Tables;

public partial class TblSkill
{
    public int Id { get; set; }

    public int FreelancerId { get; set; }

    public string Skill { get; set; } = null!;

    public virtual TblFreelancerMst Freelancer { get; set; } = null!;
}
