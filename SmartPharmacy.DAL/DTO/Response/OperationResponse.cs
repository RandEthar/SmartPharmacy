namespace SmartPharmacy.DAL.DTO.Response
{
    /// <summary>
    /// Result of an action that either succeeds or fails for a reason worth showing the caller,
    /// used where a bare bool would hide *why* the operation was refused.
    /// </summary>
    public class OperationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static OperationResponse Ok(string? message = null) =>
            new OperationResponse { Success = true, Message = message };

        public static OperationResponse Fail(string message) =>
            new OperationResponse { Success = false, Message = message };
    }
}
