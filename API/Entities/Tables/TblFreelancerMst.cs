using System;
using System.Collections.Generic;

namespace API.Entities.Tables;

public partial class TblFreelancerMst
{
    public int Id { get; set; }

    public int Deleted { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public string? Hobby { get; set; }

    public virtual ICollection<TblSkill> TblSkills { get; set; } = new List<TblSkill>();
}
