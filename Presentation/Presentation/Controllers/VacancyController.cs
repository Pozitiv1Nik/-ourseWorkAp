using BLL.DTO;
using BLL.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VacancyController : ControllerBase
    {
        private readonly IVacancyService _vacancyService;
        private readonly IUserService _userService;

        public VacancyController(IVacancyService vacancyService, IUserService userService)
        {
            _vacancyService = vacancyService;
            _userService = userService;
        }

        private async Task<UserDTO> GetCurrentUserAsync()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return await _userService.GetByUsernameAsync(username);
        }

        // GET: api/vacancy
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await GetCurrentUserAsync();

            // Тільки адміни та працівники можуть переглядати всі вакансії
            if (user.Role != UserRole.Admin && user.Role != UserRole.Worker)
                return StatusCode(403, new { message = "Only administrators and workers can view all vacancies" });

            var vacancies = await _vacancyService.GetAllVacanciesAsync(user);
            return Ok(vacancies);
        }

        // GET: api/vacancy/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await GetCurrentUserAsync();

            try
            {
                var vacancy = await _vacancyService.GetVacancyByIdAsync(id, user);
                return Ok(vacancy);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        // POST: api/vacancy
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VacancyDTO dto)
        {
            var user = await GetCurrentUserAsync();

            // Тільки роботодавці можуть створювати вакансії
            if (user.Role != UserRole.Employer)
                return StatusCode(403, new { message = "Only employers can create vacancies" });

            try
            {
                await _vacancyService.AddVacancyAsync(dto, user);
                return Ok("Vacancy created successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        // PUT: api/vacancy/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VacancyDTO dto)
        {
            var user = await GetCurrentUserAsync();
            dto.Id = id;

            try
            {
                await _vacancyService.UpdateVacancyAsync(dto, user);
                return Ok("Vacancy updated successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        // DELETE: api/vacancy/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUserAsync();

            try
            {
                await _vacancyService.DeleteVacancyAsync(id, user);
                return Ok("Vacancy deleted successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        // GET: api/vacancy/search?keyword=developer
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var user = await GetCurrentUserAsync();

            // Тільки адміни та працівники можуть шукати вакансії
            if (user.Role != UserRole.Admin && user.Role != UserRole.Worker)
                return StatusCode(403, new { message = "Only administrators and workers can search vacancies" });

            var vacancies = await _vacancyService.SearchVacanciesAsync(keyword, user);
            return Ok(vacancies);
        }

        // GET: api/vacancy/my - для роботодавців, щоб бачити свої вакансії
        [HttpGet("my")]
        public async Task<IActionResult> GetMyVacancies()
        {
            var user = await GetCurrentUserAsync();

            if (user.Role != UserRole.Employer)
                return StatusCode(403, new { message = "Only employers can view their vacancies" });

            var vacancies = await _vacancyService.GetVacanciesByEmployerAsync(user.Id);
            return Ok(vacancies);
        }
    }
}