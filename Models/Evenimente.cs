using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace PlanificatorSali.Models
{
    public class Evenimente
    {
        public int ID { get; set; }

        public string start_data { get; set; }

        public string sfarsit_data { get; set; }
      

        public string Titlu { get; set; }

        public string Descriere { get; set; }
        public bool AllDay { get; set; }
        public string culoare { get; set; }
        

        public int salaID { get; set; }
        public Sala sala { get; set; }

        
        public string participanti { get; set; }

        [ForeignKey("ApplicationUser")]
        [NotNull]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
