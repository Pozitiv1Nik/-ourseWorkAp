using System;

namespace BLL.DTO
{
    public class ResumeVacancyLinkDTO
    {
        public int Id { get; set; }
        public int ResumeId { get; set; }
        public int VacancyId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string ResumeTitle { get; set; }
        public string VacancyTitle { get; set; }
        public string WorkerName { get; set; }
        public string EmployerName { get; set; }
    }
}