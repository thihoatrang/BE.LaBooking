namespace Lawyers.Infrastructure.Models.Dtos
{
    public class PracticeAreaDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class PracticeAreaCreateDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class PracticeAreaUpdateDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
