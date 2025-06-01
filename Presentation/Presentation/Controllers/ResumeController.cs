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
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;
        private readonly IUserService _userService;

        public ResumeController(IResumeService resumeService, IUserService userService)
        {
            _resumeService = resumeService;
            _userService = userService;
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
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // PUT: api/resume/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResumeDTO dto)
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
                dto.Id = id;
                await _resumeService.UpdateResumeAsync(dto, user);
                return Ok(new { message = "Resume updated successfully" });
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
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
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
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}