using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class RegisterResponse
    {
        public String Message { get; set; }
        public bool Success { get; set; }
        public List<String>? Errors { get; set; }
    }
}
