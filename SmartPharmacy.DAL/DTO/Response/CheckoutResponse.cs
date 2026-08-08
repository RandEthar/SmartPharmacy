namespace SmartPharmacy.DAL.DTO.Response
{
    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? OrderId { get; set; }
        public string? CheckoutUrl { get; set; }
        //???
        public bool RequiresPrescription { get; set; }
    }
}
