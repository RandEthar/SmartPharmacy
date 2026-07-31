using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Models
{
  public class AuditLog
    {
        public int Id { get; set; }
        public String UserName { get; set; }
        public String Action { get; set; }
        public String OldValue { get; set; }
        public String NewValue { get; set; }
        public String TableName { get; set; }
     
        public DateTime TimeStamp { get; set; }
    }
}
