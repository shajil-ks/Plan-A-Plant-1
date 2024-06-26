using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IWalletTransactionRepository:IRepository <WalletTransaction>
    {
        void Update(WalletTransaction obj);

    }
}
