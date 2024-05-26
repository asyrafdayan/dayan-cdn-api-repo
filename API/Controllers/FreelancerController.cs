using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FreelancerController : ControllerBase
    {

        private readonly ILogger<FreelancerController> _logger;
        private readonly IFreelance _freelanceService;

        public FreelancerController(ILogger<FreelancerController> logger, IFreelance freelanceService) 
        {
            _logger = logger;
            _freelanceService = freelanceService;
        }

        [HttpGet]
        public IActionResult FetchAllFreelancers()
        {
            return Ok();
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
            return Ok();
        }

        [HttpPut("{FreelancerId}")]
        public IActionResult Put(int FreelancerId, [FromBody] FreelancerModel model)
        {
            return Ok();
        }

        [HttpDelete("{FreelancerId}")]
        public IActionResult Delete(int FreelancerId)
        {
            return Ok();
        }
    }
}
