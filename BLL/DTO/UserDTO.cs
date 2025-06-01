using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
	public class UserDTO
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public UserRole Role { get; set; }
	}
}
