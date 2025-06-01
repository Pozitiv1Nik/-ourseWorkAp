using BLL.DTO;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/resumevacancylink")]
    [ApiController]
    [Authorize]
    public class ResumeVacancyLinkController : ControllerBase
    {
        private readonly IResumeVacancyLinkService _linkService;
        private readonly IUserService _userService;
        private readonly IResumeService _resumeService;
        private readonly IVacancyService _vacancyService;

        public ResumeVacancyLinkController(
            IResumeVacancyLinkService linkService,
            IUserService userService,
            IResumeService resumeService,
            IVacancyService vacancyService)
        {
            _linkService = linkService;
            _userService = userService;
            _resumeService = resumeService;
            _vacancyService = vacancyService;
        }

        private async Task<UserDTO> GetCurrentUserAsync()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return await _userService.GetByUsernameAsync(username);
        }

        // GET: api/resumevacancylink - тільки для адмінів
        [HttpGet]
        public async Task<IActionResult> GetAllLinks()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var links = await _linkService.GetAllLinksAsync(user);
                return Ok(links);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resumevacancylink/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLinkById(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var link = await _linkService.GetLinkByIdAsync(id, user);
                return Ok(link);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Link not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // POST: api/resumevacancylink/apply - працівник подає резюме на вакансію
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyResumeToVacancy([FromBody] ApplyRequest request)
        {
            try
            {
                if (request == null || request.ResumeId <= 0 || request.VacancyId <= 0)
                {
                    return BadRequest("Invalid request data.");
                }

                var user = await GetCurrentUserAsync();
                await _linkService.ApplyResumeToVacancyAsync(request.ResumeId, request.VacancyId, user);
                return Ok(new { message = "Resume successfully applied to vacancy" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // POST: api/resumevacancylink/offer - роботодавець пропонує вакансію по резюме
        [HttpPost("offer")]
        public async Task<IActionResult> OfferVacancyToResume([FromBody] OfferRequest request)
        {
            try
            {
                if (request == null || request.ResumeId <= 0 || request.VacancyId <= 0)
                {
                    return BadRequest("Invalid request data.");
                }

                var user = await GetCurrentUserAsync();
                await _linkService.OfferVacancyToResumeAsync(request.VacancyId, request.ResumeId, user);
                return Ok(new { message = "Vacancy successfully offered to resume" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resumevacancylink/resume/{resumeId} - зв'язки для конкретного резюме
        [HttpGet("resume/{resumeId}")]
        public async Task<IActionResult> GetLinksByResume(int resumeId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var links = await _linkService.GetLinksByResumeAsync(resumeId, user);
                return Ok(links);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resumevacancylink/vacancy/{vacancyId} - зв'язки для конкретної вакансії
        [HttpGet("vacancy/{vacancyId}")]
        public async Task<IActionResult> GetLinksByVacancy(int vacancyId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var links = await _linkService.GetLinksByVacancyAsync(vacancyId, user);
                return Ok(links);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // DELETE: api/resumevacancylink/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLink(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                await _linkService.DeleteLinkAsync(id, user);
                return Ok(new { message = "Link deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Link not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resumevacancylink/check-exists?resumeId=1&vacancyId=2
        [HttpGet("check-exists")]
        public async Task<IActionResult> CheckLinkExists([FromQuery] int resumeId, [FromQuery] int vacancyId)
        {
            try
            {
                if (resumeId <= 0 || vacancyId <= 0)
                {
                    return BadRequest("Invalid resumeId or vacancyId");
                }

                var exists = await _linkService.LinkExistsAsync(resumeId, vacancyId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resumevacancylink/my-applications - мої подачі (для працівників)
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (user.Role != Domain.Entities.UserRole.Worker)
                {
                    return Forbid("Only workers can view their applications.");
                }

                // Отримуємо всі резюме користувача та їх зв'язки
                var userResumes = await _resumeService.GetAllResumesAsync(user);
                var allApplications = new List<ResumeVacancyLinkDTO>();

                foreach (var resume in userResumes)
                {
                    var resumeLinks = await _linkService.GetLinksByResumeAsync(resume.Id, user);
                    allApplications.AddRange(resumeLinks);
                }

                return Ok(allApplications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resumevacancylink/my-offers - мої пропозиції (для роботодавців)
        [HttpGet("my-offers")]
        public async Task<IActionResult> GetMyOffers()
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (user.Role != Domain.Entities.UserRole.Employer)
                {
                    return Forbid("Only employers can view their offers.");
                }

                // Отримуємо всі вакансії користувача та їх зв'язки
                var userVacancies = await _vacancyService.GetVacanciesByEmployerAsync(user.Id);
                var allOffers = new List<ResumeVacancyLinkDTO>();

                foreach (var vacancy in userVacancies)
                {
                    var vacancyLinks = await _linkService.GetLinksByVacancyAsync(vacancy.Id, user);
                    allOffers.AddRange(vacancyLinks);
                }

                return Ok(allOffers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }

    // DTOs for requests
    public class ApplyRequest
    {
        public int ResumeId { get; set; }
        public int VacancyId { get; set; }
    }

    public class OfferRequest
    {
        public int VacancyId { get; set; }
        public int ResumeId { get; set; }
    }
}