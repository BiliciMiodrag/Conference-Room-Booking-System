using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace PlanificatorSali.Models
{
    public class Participants
    {
        public Event Event { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        
        [NotNull]
        [ForeignKey("EvendId")]
        public int EventId { get; set; }

        [ForeignKey("ApplicationUserId")]
        [NotNull]
        public  string UserId { get; set; }

    }
}
