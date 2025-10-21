namespace Lawyers.Infrastructure.Models.Dtos
{
    public class ServiceDTO
    {
        public int Id { get; set; }
        public int PracticeAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public PracticeAreaDTO PracticeArea { get; set; }
    }

    public class ServiceCreateDTO
    {
        public int PracticeAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class ServiceUpdateDTO
    {
        public int PracticeAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
