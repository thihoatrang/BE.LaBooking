using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lawyers.Infrastructure.Models
{
    public class LawyerPracticeArea
    {
        [Key]
        [Column(Order = 0)]
        public int LawyerId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int PracticeAreaId { get; set; }

        // Navigation properties
        [ForeignKey("LawyerId")]
        public virtual LawyerProfile Lawyer { get; set; }

        [ForeignKey("PracticeAreaId")]
        public virtual PracticeArea PracticeArea { get; set; }
    }
}
