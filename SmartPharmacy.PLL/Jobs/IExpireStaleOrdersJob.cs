namespace SmartPharmacy.PLL.Jobs
{
    public interface IExpireStaleOrdersJob
    {
        Task Run();
    }
}
