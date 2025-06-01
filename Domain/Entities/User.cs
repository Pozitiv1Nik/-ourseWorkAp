using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class AppUser
	{
		public int Id { get; set; }
		public string AppUserName { get; set; }
		public string Password { get; set; }
		public AppUserRole Role { get; set; }
	}
}
