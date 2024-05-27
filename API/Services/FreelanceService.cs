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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;

namespace API.Services
{
    public class FreelanceService(ILogger<FreelanceService> logger, DevContext context, IRedisService redisService) : IFreelance
    {
        private readonly ILogger<FreelanceService> _logger = logger;
        private readonly DevContext _context = context;
        private readonly IRedisService _redisService = redisService;

        public ContentResult AddFreelancer(FreelancerModel model)
        {
            _logger.LogInformation("FreelanceService - Starting AddFreelancer");

            ResultDTO res = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
            };
            using var context = _context;
            try
            {
                TblFreelancerMst freelancer = new()
                {
                    Username = model.Username!,
                    Email = model.Email!,
                    Phonenumber = model.PhoneNumber!,
                    Hobby = model.Hobby!,
                    Deleted = 0
                };

                context.TblFreelancerMsts.Add(freelancer);
                int insertedFreelancer = context.SaveChanges();

                if (insertedFreelancer == 0)
                {
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    res.Remark = "Freelancer information not inserted successfully";
                    res.Result = null;
                }
                else
                {
                    if (model.SkillSets.Count > 0)
                    {
                        // Insert skills using the inserted freelancer id
                        List<TblSkill> skillSets = model.SkillSets.Select(skill => 
                            new TblSkill {
                                FreelancerId = freelancer.Id,
                                Skill = skill,
                            }).ToList();

                        context.TblSkills.AddRange(skillSets);
                        context.SaveChanges();
                    }

                    var queryRes = from tfm in context.TblFreelancerMsts
                                    join ts in context.TblSkills
                                    on tfm.Id equals ts.FreelancerId into freelancerSkills
                                    from ts in freelancerSkills.DefaultIfEmpty()
                                    where tfm.Id.Equals(freelancer.Id)
                                    select new 
                                    {
                                        tfm.Id,
                                        tfm.Username,
                                        tfm.Email,
                                        tfm.Phonenumber,
                                        skill = ts! != null ? ts.Skill : null
                                    };

                    var insertedFreelancerDetails = queryRes.ToList();

                    FreelancerModel freelancerModel = new()
                    {
                        Id = insertedFreelancerDetails.First().Id,
                        Username = insertedFreelancerDetails.First().Username,
                        Email = insertedFreelancerDetails.First().Email,
                        PhoneNumber = insertedFreelancerDetails.First().Phonenumber,
                        SkillSets = insertedFreelancerDetails.Select(s => s.skill).ToList()
                    };

                    res.StatusCode = (int)HttpStatusCode.OK;
                    res.Remark = "Freelancer information inserted";
                    res.Result = freelancerModel;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _logger.LogInformation("FreelanceService - Exit AddFreelancer");
                context.Dispose();
            }

            return CommonUtils.jsonResponse(res);
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
                // Delete from Redis Cache (If it exists)
                _redisService.DeleteData(RedisKeysConstants.FREELANCER_DETAIL_BY_ID + freelancerId);

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
            _logger.LogInformation("FreelanceService - Starting GetFreelancerDetail");

            ResultDTO res = new();

            try
            {
                List<FreelancerModel> freelancers = _context.TblFreelancerMsts
                    .Where(f => f.Deleted == 0)
                    .Select(f => new FreelancerModel
                    {
                        Id = f.Id,
                        Username = f.Username,
                        Email = f.Email,
                        PhoneNumber = f.Phonenumber
                    }).ToList();

                res.StatusCode = (int)HttpStatusCode.OK;
                res.Result = freelancers;
            }
            catch (Exception)
            {
                throw;
            }

            _logger.LogInformation("FreelanceService - Exit GetFreelancerDetail");

            return CommonUtils.jsonResponse(res);
        }

        public ContentResult GetFreelancerDetail(int freelancerId)
        {
            _logger.LogInformation("FreelanceService - Starting GetFreelancerDetail");
            
            FreelancerModel? freelancer = new();

            try
            {

                freelancer = _redisService.GetData<FreelancerModel>(RedisKeysConstants.FREELANCER_DETAIL_BY_ID + freelancerId);

                if (freelancer == null)
                {
                    TblFreelancerMst? tfm = _context.TblFreelancerMsts
                        .Where(e => e.Id == freelancerId)
                        .FirstOrDefault() ?? throw new NotFoundException(ErrorMessageConstants.ERR_USER_NOT_FOUND);

                    List<TblSkill>? skills = _context.TblSkills
                        .Where(e => e.FreelancerId == freelancerId)
                        .ToList();

                    FreelancerModel freelancerModel = new()
                    {
                        Username = tfm.Username,
                        Email = tfm.Email,
                        Hobby = tfm.Hobby,
                        PhoneNumber = tfm.Phonenumber,
                        SkillSets = skills.Select(e => e.Skill).ToList()
                    };

                    freelancer = freelancerModel;
                    bool? saved = _redisService.SetData(RedisKeysConstants.FREELANCER_DETAIL_BY_ID + freelancerId, freelancer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                throw;
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
