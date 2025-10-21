namespace Lawyers.Infrastructure.Models.Dtos
{
    public class LawyerPracticeAreaDTO
    {
        public int LawyerId { get; set; }
        public int PracticeAreaId { get; set; }
        public PracticeAreaDTO PracticeArea { get; set; }
    }

    public class LawyerPracticeAreaCreateDTO
    {
        public int LawyerId { get; set; }
        public int PracticeAreaId { get; set; }
    }
}
