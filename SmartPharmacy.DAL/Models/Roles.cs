namespace SmartPharmacy.DAL.Models
{
    /// <summary>
    /// Role names used by both the seeder and the [Authorize(Roles = ...)] attributes,
    /// so a typo can never silently lock everyone out of an endpoint.
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Pharmacist = "Pharmacist";
        public const string Patient = "Patient";

        public const string AdminOrPharmacist = Admin + "," + Pharmacist;

        public static readonly string[] All = { Patient, Admin, Pharmacist };
    }
}
