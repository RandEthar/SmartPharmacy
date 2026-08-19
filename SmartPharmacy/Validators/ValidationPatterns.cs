namespace SmartPharmacy.PL.Validators
{
    /// <summary>
    /// Patterns shared by more than one validator, so a change lands everywhere at once.
    /// </summary>
    public static class ValidationPatterns
    {
        /// <summary>
        /// Palestinian mobile numbers: Jawwal (059…) and Ooredoo (056…), written either locally
        /// as 05XXXXXXXX or internationally with the +970 / +972 (or 00970 / 00972) prefix.
        /// </summary>
        public const string PalestinianMobile = @"^(?:(?:\+|00)97[02]|0)5[69]\d{7}$";

        public const string PalestinianMobileMessage =
            "Phone number must be a valid Palestinian mobile number, e.g. 0599123456 or +970599123456.";
    }
}
