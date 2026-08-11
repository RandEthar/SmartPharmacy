using System.ComponentModel.DataAnnotations;

namespace SmartPharmacy.DAL.DTO.Request
{
    public class ChangeRoleRequest
    {
        [Required]
        public string Role { get; set; }
    }
}
