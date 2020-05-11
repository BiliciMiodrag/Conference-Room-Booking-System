using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanificatorSali.Models
{
    public class Sala
    {
        public int salaID { get; set; }

        public string Denumire { get; set; }

        public int Etaj { get; set; }

        public int capacitate { get; set; }

        public string infosala { get; set; }

        public bool disponibila { get; set; }

        public List<Evenimente> Evenimente { get; set; }
    }
}
