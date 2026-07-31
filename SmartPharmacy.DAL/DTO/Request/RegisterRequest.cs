using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Request
{
   public class RegisterRequest
    {
        public String FullName { get; set; }
        public String UserName { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
        public String PhoneNumber { get; set; }
    }
}
