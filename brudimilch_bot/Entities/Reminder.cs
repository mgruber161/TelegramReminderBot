using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brudimilch_bot.Entities
{
    [Table("Reminders")]
    public class Reminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public bool ReminderSent { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public long ChatId { get; set; }
    }
}
