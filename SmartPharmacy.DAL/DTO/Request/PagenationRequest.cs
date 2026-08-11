using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Request
{
    public   class PagenationRequest
    {
        public int Page { get; set; } = 1;
        public int Limit{ get; set; } = 10; //بكل صفحه اكم منتج بدي اعرض
        public String? Search{ get; set; }
    }
}
