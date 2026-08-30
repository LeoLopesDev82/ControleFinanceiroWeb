namespace ControleFinanceiroWeb.Services.Security
{
    public interface IPinLockout
    {
        bool IsLocked();

        int RemainingLockSeconds();

        void RegisterFailure();

        void RegisterSuccess();
    }
}
