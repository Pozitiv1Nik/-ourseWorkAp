using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using BLL.Services;
using BLL.DTO;
using BLL.Interfaces;
using DAL;
using Domain.Entities;
using AutoMapper;
using System.Collections.Generic;
using System;
using System.Linq;
using NUnit.Framework.Legacy;
using System.Linq.Expressions;

namespace Tests
{
	[TestFixture]
	public class ResumeVacancyLinkServiceTests
	{
		private Mock<IUnitOfWork> _unitOfWorkMock;
		private Mock<IMapper> _mapperMock;
		private ResumeVacancyLinkService _service;

		[SetUp]
		public void SetUp()
		{
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_mapperMock = new Mock<IMapper>();
			_service = new ResumeVacancyLinkService(_unitOfWorkMock.Object, _mapperMock.Object);
		}

		[Test]
		public async Task ApplyResumeToVacancyAsync_ShouldAddLink_WhenValid()
		{
			var requester = new UserDTO { Id = 1, Role = UserRole.Worker };
			var resume = new Resume { Id = 10, UserId = 1 };
			var vacancy = new Vacancy { Id = 20 };

			_unitOfWorkMock.Setup(u => u.Resumes.GetByIdAsync(10)).ReturnsAsync(resume);
			_unitOfWorkMock.Setup(u => u.Vacancies.GetByIdAsync(20)).ReturnsAsync(vacancy);
			_unitOfWorkMock.Setup(u => u.ResumeVacancyLinks.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ResumeVacancyLink, bool>>>()))
									   .ReturnsAsync(new List<ResumeVacancyLink>());

			await _service.ApplyResumeToVacancyAsync(10, 20, requester);

			_unitOfWorkMock.Verify(u => u.ResumeVacancyLinks.AddAsync(It.IsAny<ResumeVacancyLink>()), Times.Once);
			_unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
		}

		[Test]
		public void ApplyResumeToVacancyAsync_ShouldThrow_WhenNotWorker()
		{
			var requester = new UserDTO { Id = 1, Role = UserRole.Admin };

			Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
				_service.ApplyResumeToVacancyAsync(1, 2, requester));
		}

		[Test]
		public async Task GetLinkByIdAsync_ShouldReturnLink_WhenAdmin()
		{
			var requester = new UserDTO { Id = 99, Role = UserRole.Admin };
			var link = new ResumeVacancyLink { ResumeId = 1, VacancyId = 2 };
			var resume = new Resume { Id = 1, UserId = 11 };
			var vacancy = new Vacancy { Id = 2, UserId = 22 };

			_unitOfWorkMock.Setup(u => u.ResumeVacancyLinks.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(link);
			_unitOfWorkMock.Setup(u => u.Resumes.GetByIdAsync(1)).ReturnsAsync(resume);
			_unitOfWorkMock.Setup(u => u.Vacancies.GetByIdAsync(2)).ReturnsAsync(vacancy);
			_unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new User());

			_mapperMock.Setup(m => m.Map<ResumeVacancyLinkDTO>(It.IsAny<ResumeVacancyLink>()))
					   .Returns(new ResumeVacancyLinkDTO());

			var result = await _service.GetLinkByIdAsync(1, requester);

			ClassicAssert.IsInstanceOf<ResumeVacancyLinkDTO>(result);
		}

		[Test]
		public void GetLinkByIdAsync_ShouldThrow_WhenNotAuthorized()
		{
			var requester = new UserDTO { Id = 5, Role = UserRole.Worker };
			var link = new ResumeVacancyLink { ResumeId = 1, VacancyId = 2 };
			var resume = new Resume { Id = 1, UserId = 999 };
			var vacancy = new Vacancy { Id = 2, UserId = 888 };

			_unitOfWorkMock.Setup(u => u.ResumeVacancyLinks.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(link);
			_unitOfWorkMock.Setup(u => u.Resumes.GetByIdAsync(1)).ReturnsAsync(resume);
			_unitOfWorkMock.Setup(u => u.Vacancies.GetByIdAsync(2)).ReturnsAsync(vacancy);

			Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
				_service.GetLinkByIdAsync(1, requester));
		}

		[Test]
		public async Task LinkExistsAsync_ShouldReturnTrue_IfLinkExists()
		{
			_unitOfWorkMock.Setup(u => u.ResumeVacancyLinks.FindAsync(It.IsAny<Expression<Func<ResumeVacancyLink, bool>>>()))
						   .ReturnsAsync(new List<ResumeVacancyLink> { new ResumeVacancyLink() });


			var result = await _service.LinkExistsAsync(1, 2);

			ClassicAssert.IsTrue(result);
		}

		[Test]
		public async Task LinkExistsAsync_ShouldReturnFalse_IfLinkNotExists()
		{
			_unitOfWorkMock.Setup(u => u.ResumeVacancyLinks.FindAsync(It.IsAny<Expression<Func<ResumeVacancyLink, bool>>>()))
						   .ReturnsAsync(new List<ResumeVacancyLink>());

			var result = await _service.LinkExistsAsync(1, 2);

			ClassicAssert.IsFalse(result);
		}
	}
}
