using AutoMapper.Configuration.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

namespace PlanificatorSali.Models
{
    public class Event
    {   
        [Key]
        public int EventId { get; set; }
        [NotNull]
        public string Title { get; set; }
        public string Description { get; set; }
        [Column("event_start")]
        [NotNull]
        public string Start { get; set; }
       
        [Column("event_end")]
        public string End { get; set; }

        [Column("all_day")]
        public bool AllDay { get; set; }
        
        public string color { get; set; }

        public string participanti { get; set;}

        [NotNull]
        [ForeignKey("salaID")]
        public int salaID { get; set; }
        [NotMapped]
        public Sala sala { get; set; }
        


        [ForeignKey("ApplicationUserId")]
        [NotNull]
        public string ApplicationUserId { get; set; }
        
        [NotMapped]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }

   
}
