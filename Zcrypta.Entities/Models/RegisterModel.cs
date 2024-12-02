using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zcrypta.Entities.Models
{
    public class RegisterModel : LoginModel
    {
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
