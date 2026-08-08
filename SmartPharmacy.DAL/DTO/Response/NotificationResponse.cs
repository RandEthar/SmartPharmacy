using SmartPharmacy.DAL.Models;
using System;
using System.Collections.Generic;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class NotificationResponse
    {
        public int Id { get; set; }
        public String Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public List<NotificationProductResponse> Products { get; set; }
    }
}
