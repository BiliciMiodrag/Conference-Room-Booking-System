using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PlanificatorSali.Models
{
    public class UserLoginModel
    {
        [Required(ErrorMessage="Câmpul email este obligatoriu")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Câmpul parola este obligatoriu")]
        [DataType(DataType.Password)]
        [DisplayName("Parola")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
