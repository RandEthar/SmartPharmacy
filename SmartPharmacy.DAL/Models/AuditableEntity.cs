using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Models
{ public enum Entitystate
    {
        Active=1,
        InActive = 2,

    }
    public class AuditableEntity
    {

      public ApplicationUser  CreatedBy { set; get; }
        public ApplicationUser ?UpdatedBy { set; get; }
        public String CreatedById { set; get; }
        public String ?UpdatedById { set; get; }
        public DateTime CreatedDate { set; get; }
        public DateTime? UpdatedDate { set; get; }
        public Entitystate entitystate { set; get; }= Entitystate.Active;


    }
}
