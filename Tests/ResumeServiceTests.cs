using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Services;
using DAL;
using Domain.Entities;
using BLL.DTO;
using AutoMapper;
using System.Linq;
using BLL;
using NUnit.Framework.Legacy;

namespace Tests
{
	public class ResumeServiceTests
	{
		private Mock<IUnitOfWork> _unitOfWorkMock;
		private IMapper _mapper;
		private ResumeService _resumeService;

		[SetUp]
		public void Setup()
		{
			_unitOfWorkMock = new Mock<IUnitOfWork>();

			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = mapperConfig.CreateMapper();

			_resumeService = new ResumeService(_unitOfWorkMock.Object, _mapper);
		}

		[Test]
		public async Task GetAllResumesAsync_AsAdmin_ReturnsAllResumes()
		{
			var resumes = new List<Resume>
			{
				new Resume { Id = 1, Title = "Dev", UserId = 1 },
				new Resume { Id = 2, Title = "Tester", UserId = 2 }
			};
			var requester = new UserDTO { Id = 99, Role = UserRole.Admin };

			var resumeRepoMock = new Mock<IResumeRepository>();
			resumeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(resumes);
			_unitOfWorkMock.Setup(u => u.Resumes).Returns(resumeRepoMock.Object);
			var result = await _resumeService.GetAllResumesAsync(requester);
			ClassicAssert.AreEqual(2, result.Count());
		}

		[Test]
		public async Task GetAllResumesAsync_AsWorker_ReturnsOwnResumesOnly()
		{
			var resumes = new List<Resume>
			{
				new Resume { Id = 1, Title = "Dev", UserId = 1 },
				new Resume { Id = 2, Title = "Tester", UserId = 2 }
			};
			var requester = new UserDTO { Id = 1, Role = UserRole.Worker };

			var resumeRepoMock = new Mock<IResumeRepository>();
			resumeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(resumes);
			_unitOfWorkMock.Setup(u => u.Resumes).Returns(resumeRepoMock.Object);
			var result = await _resumeService.GetAllResumesAsync(requester);
			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.IsTrue(result.All(r => r.UserId == 1));
		}

		[Test]
		public async Task AddResumeAsync_ValidResume_AddsSuccessfully()
		{
			var newResume = new ResumeDTO
			{
				Title = "New Dev",
				Description = "C# Developer",
				Experience = "2 years",
				ExpectedSalary = 1000
			};
			var requester = new UserDTO { Id = 10, Role = UserRole.Worker };

			var resumeRepoMock = new Mock<IResumeRepository>();
			_unitOfWorkMock.Setup(u => u.Resumes).Returns(resumeRepoMock.Object);
			_unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
			await _resumeService.AddResumeAsync(newResume, requester);
			resumeRepoMock.Verify(r => r.AddAsync(It.Is<Resume>(res =>
				res.Title == "New Dev" &&
				res.UserId == 10
			)), Times.Once);
		}

		[Test]
		public async Task DeleteResumeAsync_WorkerDeletingOwnResume_Success()
		{
			var resume = new Resume { Id = 5, Title = "Test", UserId = 3 };
			var requester = new UserDTO { Id = 3, Role = UserRole.Worker };

			var resumeRepoMock = new Mock<IResumeRepository>();
			resumeRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(resume);
			_unitOfWorkMock.Setup(u => u.Resumes).Returns(resumeRepoMock.Object);
			_unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
			await _resumeService.DeleteResumeAsync(5, requester);
			resumeRepoMock.Verify(r => r.Remove(resume), Times.Once);
		}

		[Test]
		public void DeleteResumeAsync_WorkerDeletingOtherUsersResume_Throws()
		{
			var resume = new Resume { Id = 7, Title = "Test", UserId = 2 };
			var requester = new UserDTO { Id = 1, Role = UserRole.Worker };

			var resumeRepoMock = new Mock<IResumeRepository>();
			resumeRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(resume);
			_unitOfWorkMock.Setup(u => u.Resumes).Returns(resumeRepoMock.Object);
			ClassicAssert.ThrowsAsync<UnauthorizedAccessException>(async () =>
			{
				await _resumeService.DeleteResumeAsync(7, requester);
			});
		}
	}
}
