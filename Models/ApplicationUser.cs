using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PlanificatorSali.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string Prenume { get; set; }

        public string Nume { get; set; }

        public virtual ICollection<Event> Events { get; set; }


        public string GetFullName()
        {
           string NumeComplet = Nume +" " + Prenume;
            return NumeComplet;
        }
    }
}
