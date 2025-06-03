using BLL.DTO;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;
        private readonly IUserService _userService;
        private readonly ILogger<ResumeController> _logger;

        public ResumeController(IResumeService resumeService, IUserService userService, ILogger<ResumeController> logger)
        {
            _resumeService = resumeService;
            _userService = userService;
            _logger = logger;
        }

        private async Task<UserDTO> GetCurrentUserAsync()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return await _userService.GetByUsernameAsync(username);
        }

        // GET: api/resume
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var resumes = await _resumeService.GetAllResumesAsync(user);
                return Ok(resumes);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all resumes");
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resume/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var resume = await _resumeService.GetResumeByIdAsync(id, user);
                return Ok(resume);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Resume not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resume by id: {ResumeId}", id);
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // POST: api/resume
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResumeDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Description))
                {
                    return BadRequest("Title and description are required.");
                }

                var user = await GetCurrentUserAsync();
                await _resumeService.AddResumeAsync(dto, user);
                return Ok(new { message = "Resume created successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resume");
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // PUT: api/resume/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResumeDTO dto)
        {
            try
            {
                _logger.LogInformation("Updating resume {ResumeId} with data: {@ResumeData}", id, dto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Description))
                {
                    _logger.LogWarning("Title or Description is empty");
                    return BadRequest("Title and description are required.");
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    _logger.LogWarning("User not found");
                    return Unauthorized("User not found");
                }

                _logger.LogInformation("Current user: {UserId}, {Username}", user.Id, user.UserName);

                // Встановлюємо правильний ID
                dto.Id = id;
                // НЕ встановлюємо UserId тут - це має робити сервіс

                await _resumeService.UpdateResumeAsync(dto, user);
                return Ok(new { message = "Resume updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resume not found: {ResumeId}", id);
                return NotFound("Resume not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to resume: {ResumeId}", id);
                return Forbid(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating resume {ResumeId}. Inner exception: {InnerException}",
                    id, ex.InnerException?.Message);

                return StatusCode(500, new
                {
                    message = "Database error occurred",
                    error = ex.InnerException?.Message ?? ex.Message,
                    details = "Check database constraints and field lengths"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating resume {ResumeId}", id);
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // DELETE: api/resume/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                await _resumeService.DeleteResumeAsync(id, user);
                return Ok(new { message = "Resume deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Resume not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resume {ResumeId}", id);
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resume/my - мої резюме (для працівників)
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResumes()
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (user.Role != Domain.Entities.UserRole.Worker)
                {
                    return Forbid("Only workers can view their own resumes.");
                }

                var resumes = await _resumeService.GetAllResumesAsync(user);
                return Ok(resumes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's resumes");
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/resume/search?keyword=developer - пошук резюме (для роботодавців та адміністраторів)
        [HttpGet("search")]
        public async Task<IActionResult> SearchResumes([FromQuery] string keyword)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                // Тільки роботодавці та адміни можуть шукати резюме
                if (user.Role != Domain.Entities.UserRole.Employer && user.Role != Domain.Entities.UserRole.Admin)
                {
                    return Forbid("Only employers and admins can search resumes.");
                }

                var allResumes = await _resumeService.GetAllResumesAsync(user);

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Ok(allResumes);
                }

                var filteredResumes = allResumes.Where(r =>
                    r.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                return Ok(filteredResumes);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching resumes");
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}