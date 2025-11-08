using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalAPI.Domain.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = default!;

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = default!;
        
        [StringLength(50)]
        [Required]
        public string Password { get; set; } = default!;

        [StringLength(10)]
        [Required]
        public string Profile { get; set; } = default!;
    }
}
