using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Response
{
   public class PagenationResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        //computed property to calculate the total number of pages based on the total count and limit 
        public int TotalPage => (int)Math.Ceiling((double)TotalCount / Limit);  


    }
}
