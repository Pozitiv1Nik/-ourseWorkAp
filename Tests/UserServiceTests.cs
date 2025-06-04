using NUnit.Framework;
using Moq;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.DTO;
using BLL.Services;
using DAL;
using Domain.Entities;
using System;
using System.Linq.Expressions;
using NUnit.Framework.Legacy;

namespace BLL.Tests
{
	[TestFixture]
	public class UserServiceTests
	{
		private Mock<IUnitOfWork> _unitOfWorkMock;
		private Mock<IUserRepository> _userRepoMock;
		private Mock<IMapper> _mapperMock;
		private UserService _userService;

		[SetUp]
		public void Setup()
		{
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_userRepoMock = new Mock<IUserRepository>();
			_mapperMock = new Mock<IMapper>();

			_unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

			_userService = new UserService(_unitOfWorkMock.Object, _mapperMock.Object);
		}

		[Test]
		public async Task GetUserByIdAsync_ValidId_ReturnsUserDto()
		{
			var user = new User { Id = 1, UserName = "test", Role = UserRole.Worker };
			var userDto = new UserDTO { Id = 1, UserName = "test", Role = UserRole.Worker };

			_userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
			_mapperMock.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);
			var result = await _userService.GetUserByIdAsync(1);
			ClassicAssert.AreEqual("test", result.UserName);
			ClassicAssert.AreEqual(UserRole.Worker, result.Role);
		}

		[Test]
		public void GetUserByIdAsync_InvalidId_ThrowsKeyNotFound()
		{
			_userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);
			Assert.ThrowsAsync<KeyNotFoundException>(async () =>
				await _userService.GetUserByIdAsync(1));
		}

		[Test]
		public async Task GetAllUsersAsync_ReturnsAllMappedUsers()
		{
			var users = new List<User>
			{
				new User { Id = 1, UserName = "a", Role = UserRole.Admin },
				new User { Id = 2, UserName = "b", Role = UserRole.Worker }
			};

			_userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
			_mapperMock.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
				.Returns<User>(u => new UserDTO { Id = u.Id, UserName = u.UserName, Role = u.Role });

			var result = await _userService.GetAllUsersAsync();

			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.IsTrue(result.Any(r => r.UserName == "a"));
		}

		[Test]
		public async Task AddUserAsync_NewUsername_AddsUser()
		{
			var userDto = new UserDTO { UserName = "new_user", Role = UserRole.Employer };
			var user = new User { UserName = "new_user", Role = UserRole.Employer };

			_userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
				.ReturnsAsync(new List<User>());
			_mapperMock.Setup(m => m.Map<User>(userDto)).Returns(user);

			await _userService.AddUserAsync(userDto, "secret");

			_userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.UserName == "new_user" && u.Password == "secret")), Times.Once);
			_unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
		}

		[Test]
		public void AddUserAsync_UsernameExists_ThrowsInvalidOperationException()
		{
			var userDto = new UserDTO { UserName = "existing" };
			var existingUser = new User { UserName = "existing" };

			_userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
				.ReturnsAsync(new List<User> { existingUser });

			ClassicAssert.ThrowsAsync<InvalidOperationException>(async () =>
				await _userService.AddUserAsync(userDto, "pass"));
		}

		[Test]
		public async Task AuthenticateAsync_ValidCredentials_ReturnsUserDto()
		{
			var user = new User { Id = 1, UserName = "john", Password = "1234", Role = UserRole.Admin };
			var userDto = new UserDTO { Id = 1, UserName = "john", Role = UserRole.Admin };

			_userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
				.ReturnsAsync(new List<User> { user });

			_mapperMock.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

			var result = await _userService.AuthenticateAsync("john", "1234");

			ClassicAssert.AreEqual("john", result.UserName);
			ClassicAssert.AreEqual(UserRole.Admin, result.Role);
		}

		[Test]
		public void AuthenticateAsync_InvalidCredentials_ThrowsUnauthorizedAccess()
		{
			var user = new User { UserName = "john", Password = "wrongpass" };

			_userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
				.ReturnsAsync(new List<User> { user });

			ClassicAssert.ThrowsAsync<UnauthorizedAccessException>(async () =>
				await _userService.AuthenticateAsync("john", "1234"));
		}

		[Test]
		public async Task UpdateUserAsync_ValidUser_UpdatesAndSaves()
		{
			var userDto = new UserDTO { Id = 1, UserName = "updated", Role = UserRole.Worker };
			var user = new User { Id = 1, UserName = "old", Role = UserRole.Admin };

			_userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

			await _userService.UpdateUserAsync(userDto);

			_mapperMock.Verify(m => m.Map(userDto, user), Times.Once);
			_userRepoMock.Verify(r => r.Update(user), Times.Once);
			_unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
		}

		[Test]
		public void UpdateUserAsync_UserNotFound_ThrowsKeyNotFound()
		{
			_userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);

			Assert.ThrowsAsync<KeyNotFoundException>(async () =>
				await _userService.UpdateUserAsync(new UserDTO { Id = 1 }));
		}

		[Test]
		public async Task DeleteUserAsync_ValidId_RemovesAndSaves()
		{
			var user = new User { Id = 1 };

			_userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

			await _userService.DeleteUserAsync(1);

			_userRepoMock.Verify(r => r.Remove(user), Times.Once);
			_unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
		}

		[Test]
		public void DeleteUserAsync_UserNotFound_ThrowsKeyNotFound()
		{
			_userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User)null);

			Assert.ThrowsAsync<KeyNotFoundException>(async () =>
				await _userService.DeleteUserAsync(99));
		}

		[Test]
		public async Task GetByUsernameAsync_ValidUsername_ReturnsUserDto()
		{
			var user = new User { Id = 2, UserName = "login" };
			var userDto = new UserDTO { Id = 2, UserName = "login"};

			_userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
				.ReturnsAsync(new List<User> { user });

			_mapperMock.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

			var result = await _userService.GetByUsernameAsync("login");

			ClassicAssert.AreEqual("login", result.UserName);
		}

		[Test]
		public void GetByUsernameAsync_NotFound_ThrowsKeyNotFound()
		{
			_userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
				.ReturnsAsync(new List<User>());

			ClassicAssert.ThrowsAsync<KeyNotFoundException>(async () =>
				await _userService.GetByUsernameAsync("none"));
		}
	}
}
