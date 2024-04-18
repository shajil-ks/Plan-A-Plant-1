﻿namespace Plan_A_Plant.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category{  get; }
        IProductRepository Product { get; }
        IProductImageRepository ProductImage { get; }

        IShoppingCartRepository ShoppingCart { get; }   

        IApplicationUserRepository ApplicationUser { get; } 

        IOrderDetailRepository OrderDetail { get; } 

        IOrderHeaderRepository OrderHeader { get; } 
       
        void Save();
    }
}
