using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lawyers.Infrastructure.Models
{
    public class LawyerDiploma
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int LawyerId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string QualificationType { get; set; }

        public string Description { get; set; }

        public DateTime? IssuedDate { get; set; }

        [StringLength(255)]
        public string IssuedBy { get; set; }

        public string DocumentUrl { get; set; }

        public bool IsPublic { get; set; }

        public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey("LawyerId")]
        public virtual LawyerProfile LawyerProfile { get; set; }
    }
} 