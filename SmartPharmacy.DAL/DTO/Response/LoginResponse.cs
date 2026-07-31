using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Response
{
  public class LoginResponse
    {
        public String Message { get; set; }
        public bool Success { get; set; }
        public string? AccessToken { get; set; }

    }
}
