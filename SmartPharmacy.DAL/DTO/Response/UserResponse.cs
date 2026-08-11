using System;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class UserResponse
    {
        public String Id { get; set; }
        public String FullName { get; set; }
        public String Email { get; set; }
        public String PhoneNumber { get; set; }
        public String City { get; set; }
        public String Street { get; set; }
        public bool IsBlocked { get; set; }
    }
}
