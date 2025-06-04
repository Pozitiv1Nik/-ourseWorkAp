using NUnit.Framework;
using Moq;
using BLL.Services;
using BLL.DTO;
using Domain.Entities;
using DAL;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using BLL.Interfaces;
using NUnit.Framework.Legacy;
using System.Linq.Expressions;

namespace BLL.Tests
{
	[TestFixture]
	public class VacancyServiceTests
	{
		private Mock<IUnitOfWork> _unitOfWorkMock;
		private Mock<IVacancyRepository> _vacancyRepoMock;
		private Mock<IMapper> _mapperMock;
		private IVacancyService _vacancyService;

		[SetUp]
		public void SetUp()
		{
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_vacancyRepoMock = new Mock<IVacancyRepository>();
			_mapperMock = new Mock<IMapper>();

			_unitOfWorkMock.Setup(u => u.Vacancies).Returns(_vacancyRepoMock.Object);
			_vacancyService = new VacancyService(_unitOfWorkMock.Object, _mapperMock.Object);
		}

		[Test]
		public async Task GetAllVacanciesAsync_Admin_ReturnsAllVacancies()
		{
			var vacancies = new List<Vacancy> {
				new Vacancy { Id = 1, Title = "Dev" },
				new Vacancy { Id = 2, Title = "QA" }
			};
			var requester = new UserDTO { Role = UserRole.Admin };

			_vacancyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(vacancies);
			_mapperMock.Setup(m => m.Map<VacancyDTO>(It.IsAny<Vacancy>())).Returns<Vacancy>(v => new VacancyDTO { Id = v.Id, Title = v.Title });

			var result = await _vacancyService.GetAllVacanciesAsync(requester);

			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.IsTrue(result.Any(v => v.Title == "Dev"));
		}

		[Test]
		public async Task GetAllVacanciesAsync_Employer_ReturnsOnlyOwnVacancies()
		{
			var vacancies = new List<Vacancy> {
				new Vacancy { Id = 1, Title = "Dev", UserId = 5 },
				new Vacancy { Id = 2, Title = "QA", UserId = 10 }
			};
			var requester = new UserDTO { Id = 5, Role = UserRole.Employer };

			_vacancyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(vacancies);
			_mapperMock.Setup(m => m.Map<VacancyDTO>(It.IsAny<Vacancy>())).Returns<Vacancy>(v => new VacancyDTO { Id = v.Id });

			var result = await _vacancyService.GetAllVacanciesAsync(requester);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(1, result.First().Id);
		}

		[Test]
		public async Task GetVacancyByIdAsync_Worker_ReturnsVacancy()
		{
			var vacancy = new Vacancy { Id = 1, Title = "Dev" };
			var requester = new UserDTO { Role = UserRole.Worker };

			_vacancyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vacancy);
			_mapperMock.Setup(m => m.Map<VacancyDTO>(vacancy)).Returns(new VacancyDTO { Id = 1 });

			var result = await _vacancyService.GetVacancyByIdAsync(1, requester);

			ClassicAssert.AreEqual(1, result.Id);
		}

		[Test]
		public void GetVacancyByIdAsync_EmployerOtherUser_ThrowsUnauthorizedAccess()
		{
			var vacancy = new Vacancy { Id = 1, UserId = 99 };
			var requester = new UserDTO { Role = UserRole.Employer, Id = 5 };

			_vacancyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vacancy);

			Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
				await _vacancyService.GetVacancyByIdAsync(1, requester));
		}

		[Test]
		public async Task AddVacancyAsync_ValidEmployer_AddsSuccessfully()
		{
			var requester = new UserDTO { Id = 5, Role = UserRole.Employer };
			var vacancyDTO = new VacancyDTO { Title = "New Job" };

			_mapperMock.Setup(m => m.Map<Vacancy>(It.IsAny<VacancyDTO>()))
				.Returns<VacancyDTO>(v => new Vacancy { Title = v.Title, UserId = v.UserId });

			await _vacancyService.AddVacancyAsync(vacancyDTO, requester);

			_vacancyRepoMock.Verify(r => r.AddAsync(It.Is<Vacancy>(v => v.Title == "New Job" && v.UserId == 5)), Times.Once);
			_unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
		}

		[Test]
		public async Task SearchVacanciesAsync_WithKeyword_ReturnsFilteredResults()
		{
			var requester = new UserDTO { Role = UserRole.Worker };
			var vacancies = new List<Vacancy> {
				new Vacancy { Id = 1, Title = "Dev", Description = "C#" },
				new Vacancy { Id = 2, Title = "Tester", Description = "QA" }
			};

			_vacancyRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Vacancy, bool>>>()))
							.ReturnsAsync(vacancies);

			_mapperMock.Setup(m => m.Map<VacancyDTO>(It.IsAny<Vacancy>())).Returns<Vacancy>(v => new VacancyDTO { Id = v.Id });

			var result = await _vacancyService.SearchVacanciesAsync("dev", requester);

			ClassicAssert.AreEqual(2, result.Count());
		}

		[Test]
		public void AddVacancyAsync_NonEmployer_ThrowsUnauthorized()
		{
			var requester = new UserDTO { Role = UserRole.Worker };
			var vacancyDTO = new VacancyDTO();

			Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
				await _vacancyService.AddVacancyAsync(vacancyDTO, requester));
		}

		[Test]
		public async Task GetVacanciesByEmployerAsync_ReturnsOnlyMatching()
		{
			var vacancies = new List<Vacancy> {
				new Vacancy { Id = 1, UserId = 5 },
				new Vacancy { Id = 2, UserId = 5 }
			};

			_vacancyRepoMock
				.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Vacancy, bool>>>()))
				.ReturnsAsync(vacancies);
			_mapperMock.Setup(m => m.Map<VacancyDTO>(It.IsAny<Vacancy>())).Returns<Vacancy>(v => new VacancyDTO { Id = v.Id });

			var result = await _vacancyService.GetVacanciesByEmployerAsync(5);

			ClassicAssert.AreEqual(2, result.Count());
		}
	}
}
