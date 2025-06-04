using BLL.DTO;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
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

        // GET: api/resumevacancylink/all - для адмінів, всі заявки
        [HttpGet("all")]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user.Role != 0) // Only admin
                {
                    return StatusCode(403, new { message = "Only administrators can view all applications" });
                }

                var links = await _linkService.GetAllLinksAsync(user);
                return Ok(links);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
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
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // POST: api/resumevacancylink/apply - працівник подає резюме на вакансію
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyResumeToVacancy([FromBody] ApplyResumeRequest request)
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
        public async Task<IActionResult> OfferVacancyToResume([FromBody] OfferVacancyRequest request)
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
                return StatusCode(403, new { message = ex.Message });
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

        // GET: api/resumevacancylink/received-applications - отримані заявки (для роботодавців)
        [HttpGet("received-applications")]
        public async Task<IActionResult> GetReceivedApplications()
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (user.Role != Domain.Entities.UserRole.Employer)
                {
                    return Forbid("Only employers can view received applications.");
                }

                var userVacancies = await _vacancyService.GetVacanciesByEmployerAsync(user.Id);
                var allApplications = new List<ResumeVacancyLinkDTO>();

                foreach (var vacancy in userVacancies)
                {
                    var vacancyLinks = await _linkService.GetLinksByVacancyAsync(vacancy.Id, user);
                    allApplications.AddRange(vacancyLinks);
                }

                return Ok(allApplications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resumevacancylink/dashboard - дашборд для всіх ролей
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                object dashboardData;

                switch (user.Role)
                {
                    case Domain.Entities.UserRole.Admin:
                        var allLinks = await _linkService.GetAllLinksAsync(user);
                        dashboardData = new
                        {
                            Role = "Admin",
                            TotalLinks = allLinks.Count(),
                            RecentLinks = allLinks.OrderByDescending(l => l.SubmittedAt).Take(10),
                            Stats = new
                            {
                                ApplicationsToday = allLinks.Count(l => l.SubmittedAt.Date == DateTime.Today),
                                ApplicationsThisWeek = allLinks.Count(l => l.SubmittedAt >= DateTime.Today.AddDays(-7))
                            }
                        };
                        break;

                    case Domain.Entities.UserRole.Worker:
                        var myApplications = await GetMyApplicationsData(user);
                        dashboardData = new
                        {
                            Role = "Worker",
                            MyApplications = myApplications.Count,
                            RecentApplications = myApplications.OrderByDescending(l => l.SubmittedAt).Take(5),
                            Stats = new
                            {
                                ApplicationsThisMonth = myApplications.Count(l => l.SubmittedAt.Month == DateTime.Now.Month)
                            }
                        };
                        break;

                    case Domain.Entities.UserRole.Employer:
                        var receivedApplications = await GetReceivedApplicationsData(user);
                        dashboardData = new
                        {
                            Role = "Employer",
                            ReceivedApplications = receivedApplications.Count,
                            RecentApplications = receivedApplications.OrderByDescending(l => l.SubmittedAt).Take(5),
                            Stats = new
                            {
                                NewApplicationsToday = receivedApplications.Count(l => l.SubmittedAt.Date == DateTime.Today)
                            }
                        };
                        break;

                    default:
                        return Forbid("Invalid user role");
                }

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        private async Task<List<ResumeVacancyLinkDTO>> GetMyApplicationsData(UserDTO user)
        {
            var userResumes = await _resumeService.GetAllResumesAsync(user);
            var allApplications = new List<ResumeVacancyLinkDTO>();

            foreach (var resume in userResumes)
            {
                var resumeLinks = await _linkService.GetLinksByResumeAsync(resume.Id, user);
                allApplications.AddRange(resumeLinks);
            }

            return allApplications;
        }

        private async Task<List<ResumeVacancyLinkDTO>> GetReceivedApplicationsData(UserDTO user)
        {
            var userVacancies = await _vacancyService.GetVacanciesByEmployerAsync(user.Id);
            var allApplications = new List<ResumeVacancyLinkDTO>();

            foreach (var vacancy in userVacancies)
            {
                var vacancyLinks = await _linkService.GetLinksByVacancyAsync(vacancy.Id, user);
                allApplications.AddRange(vacancyLinks);
            }

            return allApplications;
        }
    }

    // DTOs for requests
    public class ApplyResumeRequest
    {
        public int ResumeId { get; set; }
        public int VacancyId { get; set; }
    }

    public class OfferVacancyRequest
    {
        public int VacancyId { get; set; }
        public int ResumeId { get; set; }
    }
}