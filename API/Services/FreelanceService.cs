using API.Constants;
using API.Entities.Contexts;
using API.Entities.Tables;
using API.Exceptions;
using API.Interfaces;
using API.Models;
using API.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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

            ResultDTO res = new();
            using var context = _context;

            try
            {
                if (string.IsNullOrEmpty(model.Username)
                    || string.IsNullOrEmpty(model.Email))
                {
                    throw new InvalidModelException(ErrorMessageConstants.ERR_INVALID_MODEL);
                }

                // Check if Username exists
                TblFreelancerMst? checkUsername = _context.TblFreelancerMsts.Where(e => e.Username == model.Username).FirstOrDefault();

                if (checkUsername != null)
                {
                    throw new UserExistsException(ErrorMessageConstants.ERR_USER_ALREADY_EXISTS);
                }

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
                                        tfm.Hobby,
                                        skill = ts! != null ? ts.Skill : null
                                    };

                    var insertedFreelancerDetails = queryRes.ToList();

                    FreelancerModel freelancerModel = new()
                    {
                        Id = insertedFreelancerDetails.First().Id,
                        Username = insertedFreelancerDetails.First().Username,
                        Email = insertedFreelancerDetails.First().Email,
                        PhoneNumber = insertedFreelancerDetails.First().Phonenumber,
                        Hobby = insertedFreelancerDetails.First().Hobby,
                        SkillSets = insertedFreelancerDetails.Select(s => s.skill).ToList()
                    };

                    res.StatusCode = (int)HttpStatusCode.OK;
                    res.Remark = "Freelancer information inserted";
                    res.Result = freelancerModel;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
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
                // Fetch freelancer information
                TblFreelancerMst? freelancerDetail = _context.TblFreelancerMsts
                    .Where(e => e.Id == freelancerId)
                    .FirstOrDefault() 
                    ?? throw new NotFoundException(ErrorMessageConstants.ERR_USER_NOT_FOUND);

                // Delete from Redis Cache (If it exists)
                _redisService.DeleteData(RedisKeysConstants.FREELANCER_DETAIL_BY_ID + freelancerId);

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

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                throw;
            }

            _logger.LogInformation("FreelanceService - Exit GetFreelancerDetail");
            return CommonUtils.jsonResponse(res);
        }

        public ContentResult GetAllFreelancer(string? username, bool sortDesc = false)
        {
            _logger.LogInformation("FreelanceService - Starting GetFreelancerDetail");

            ResultDTO res = new();

            try
            {
                var query = _context.TblFreelancerMsts
                    .Where(f => f.Deleted == 0);


                if (!string.IsNullOrEmpty(username))
                {
                    query = query.Where(f => f.Username.StartsWith(username));
                }

                if (sortDesc == true)
                {
                    query = query.OrderByDescending(f => f.Username);
                }

                List<FreelancerModel> freelancers = query
                    .Select(f => new FreelancerModel
                    {
                        Id = f.Id,
                        Username = f.Username,
                        Email = f.Email,
                        Hobby = f.Hobby,
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
            _logger.LogInformation("FreelanceService - Starting UpdateFreelancerDetail");

            ResultDTO res = new();

            try
            {
                if (freelancerId == 0 
                    || string.IsNullOrEmpty(model.Username)
                    || string.IsNullOrEmpty(model.Email))
                {
                    throw new InvalidModelException(ErrorMessageConstants.ERR_INVALID_MODEL);
                }

                TblFreelancerMst? freelancerDetail = _context.TblFreelancerMsts.Where(e => e.Id == freelancerId).FirstOrDefault();

                if (freelancerDetail == null)
                {
                    throw new NotFoundException(ErrorMessageConstants.ERR_USER_NOT_FOUND);
                }
                else
                {
                    freelancerDetail.Hobby = model.Hobby;
                    freelancerDetail.Email = model.Email;
                    freelancerDetail.Phonenumber = model.PhoneNumber;
                }

                List<TblSkill> skills = _context.TblSkills.Where(e => e.FreelancerId == freelancerId).ToList();
                
                _context.TblSkills.RemoveRange(skills);
                
                if (model.SkillSets.Count > 0)
                {

                    List<TblSkill> newSkills = new();

                    foreach (var s in model.SkillSets)
                    {
                        TblSkill sk = new()
                        {
                            FreelancerId = freelancerId,
                            Skill = s
                        };

                        newSkills.Add(sk);
                    }

                    _context.TblSkills.AddRange(newSkills);
                }

                _context.SaveChanges();

                var queryRes = from tfm in context.TblFreelancerMsts
                               join ts in context.TblSkills
                               on tfm.Id equals ts.FreelancerId into freelancerSkills
                               from ts in freelancerSkills.DefaultIfEmpty()
                               where tfm.Id.Equals(freelancerDetail.Id)
                               select new
                               {
                                   tfm.Id,
                                   tfm.Username,
                                   tfm.Email,
                                   tfm.Phonenumber,
                                   tfm.Hobby,
                                   skill = ts! != null ? ts.Skill : null
                               };

                var updatedFreelancerDetails = queryRes.ToList();

                FreelancerModel freelancerModel = new()
                {
                    Id = updatedFreelancerDetails.First().Id,
                    Username = updatedFreelancerDetails.First().Username,
                    Email = updatedFreelancerDetails.First().Email,
                    PhoneNumber = updatedFreelancerDetails.First().Phonenumber,
                    Hobby = updatedFreelancerDetails.First().Hobby,
                    SkillSets = updatedFreelancerDetails.Select(s => s.skill).ToList()
                };

                res.StatusCode = (int)HttpStatusCode.OK;
                res.Remark = "Freelancer information updated";
                res.Result = freelancerModel;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Exception : {ex.Message}");
                throw; 
            }

            _logger.LogInformation("FreelanceService - Exit UpdateFreelancerDetail");
            return CommonUtils.jsonResponse(res);
        }
    }
}
