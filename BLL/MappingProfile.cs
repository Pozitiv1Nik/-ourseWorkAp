using AutoMapper;
using BLL.DTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BLL
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<User, UserDTO>().ReverseMap();
			CreateMap<Resume, ResumeDTO>().ReverseMap();
			CreateMap<Vacancy, VacancyDTO>().ReverseMap();
			CreateMap<ResumeVacancyLink, ResumeVacancyLinkDTO>().ReverseMap();
		}
	}
}
