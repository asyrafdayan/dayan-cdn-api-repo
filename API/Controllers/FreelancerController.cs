using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FreelancerController(ILogger<FreelancerController> logger, IFreelance freelanceService) : ControllerBase
    {

        private readonly ILogger<FreelancerController> _logger = logger;
        private readonly IFreelance _freelanceService = freelanceService;

        [HttpGet]
        public IActionResult FetchAllFreelancers([FromQuery] string? username, [FromQuery] bool sortdesc = false)
        {
            _logger.LogInformation("Freelancer - Start FetchAllFreelancers");

            ContentResult content = _freelanceService.GetAllFreelancer(username, sortdesc);

            _logger.LogInformation("Freelancer - Exit FetchAllFreelancers");
            return content;
        }

        [HttpGet("{FreelancerId}")]
        public IActionResult FetchFreelancerDetails(int FreelancerId)
        {
            _logger.LogInformation("Freelancer - Start FetchFreelancerDetails");

            ContentResult content = _freelanceService.GetFreelancerDetail(FreelancerId);
            
            _logger.LogInformation("Freelancer - Exit FetchFreelancerDetails");
            return content;
        }

        [HttpPost("Register")]
        public IActionResult AddFreelancer([FromBody] FreelancerModel model)
        {
            _logger.LogInformation("Freelancer - Start FetchFreelancerDetails");
            ContentResult content = _freelanceService.AddFreelancer(model);
            _logger.LogInformation("Freelancer - Exit FetchFreelancerDetails");
            return content;
        }

        [HttpPut("{FreelancerId}")]
        public IActionResult UpdateFreelancer(int FreelancerId, [FromBody] FreelancerModel model)
        {
            _logger.LogInformation("Freelancer - Start UpdateFreelancer");
            ContentResult content = _freelanceService.UpdateFreelancerDetail(FreelancerId, model);
            _logger.LogInformation("Freelancer - Exit UpdateFreelancer");
            return content;
        }

        [HttpDelete("{FreelancerId}")]
        public IActionResult DeleteFreelancer(int FreelancerId)
        {
            _logger.LogInformation("Freelancer - Start DeleteFreelancer");
            ContentResult content = _freelanceService.DeleteFreelancer(FreelancerId);
            _logger.LogInformation("Freelancer - Exit DeleteFreelancer");
            return content;
        }
    }
}
