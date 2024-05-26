using API.Constants;
using API.Entities.Contexts;
using API.Entities.Tables;
using API.Exceptions;
using API.Interfaces;
using API.Models;
using API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using System.Net;

namespace API.Services
{
    public class FreelanceService(ILogger<FreelanceService> logger, DevContext context) : IFreelance
    {
        private readonly ILogger<FreelanceService> _logger = logger;
        private readonly DevContext _context = context;

        public ContentResult AddFreelancer(FreelancerModel model)
        {
            throw new NotImplementedException();
        }

        public ContentResult DeleteFreelancer(int freelancerId)
        {
            _logger.LogInformation("FreelanceService - Starting GetFreelancerDetail");

            ResultDTO res = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Remark = "Freelancer deleted succesfully"
            };

            try
            {
                // Fetch freelancer info
                TblFreelancerMst? freelancerDetail = _context.TblFreelancerMsts.Where(e => e.Id == freelancerId).FirstOrDefault();

                // Fetch skills
                List<TblSkill> freelancerSkills = _context.TblSkills.Where(e => e.FreelancerId == freelancerId).ToList();

                if (freelancerSkills.Count > 0)
                {
                    _context.TblSkills.RemoveRange(freelancerSkills);
                }

                if (freelancerDetail != null)
                {
                    _context.TblFreelancerMsts.Remove(freelancerDetail);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                res.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                _context.SaveChanges();
                _logger.LogInformation("FreelanceService - Exit GetFreelancerDetail");
            }

            
            return CommonUtils.jsonResponse(res);
        }

        public ContentResult GetAllFreelancer()
        {
            throw new NotImplementedException();
        }

        public ContentResult GetFreelancerDetail(int freelancerId)
        {
            _logger.LogInformation("FreelanceService - Starting GetFreelancerDetail");
            
            FreelancerModel freelancer = new();

            try
            {
                TblFreelancerMst? tfm = _context.TblFreelancerMsts
                    .Where(e => e.Id == freelancerId)
                    .FirstOrDefault();

                if (tfm == null)
                {
                    throw new NotFoundException(ErrorMessageConstants.ERR_USER_NOT_FOUND);
                }
                
                List<TblSkill> skills = _context.TblSkills
                    .Where(e => e.FreelancerId == freelancerId)
                    .ToList();

                freelancer.Username = tfm.Username;
                freelancer.Email = tfm.Email;
                freelancer.Hobby = tfm.Hobby;
                freelancer.PhoneNumber = tfm.Phonenumber;
                skills.ForEach(e => { freelancer.SkillSets.Add(e.Skill); });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
            }
            finally
            {
                _logger.LogInformation("FreelanceService - Exit GetFreelancerDetail");
            }

            ResultDTO res = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Result = freelancer
            };

            return CommonUtils.jsonResponse(res);
        }

        public ContentResult UpdateFreelancerDetail(int freelancerId, FreelancerModel model)
        {
            throw new NotImplementedException();
        }
    }
}
