using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IFreelance
    {
        ContentResult AddFreelancer(FreelancerModel model);
        ContentResult GetAllFreelancer();
        ContentResult GetFreelancerDetail(int freelancerId);
        ContentResult UpdateFreelancerDetail(int freelancerId, FreelancerModel model);
        ContentResult DeleteFreelancer(int freelancerId);
    }
}
