using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace ShopProject
{
    public class ApiCall
    {
        public string Method { get; set; }
        public string PostData { get; set; }
    }

    public class LoginClass
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class ActiveLoginClass
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

    public class DeleteClass
    {
        public long Id { get; set; }
    }

    public class SystemAdminWithLoginClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class SystemAdminWithLoginIdClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Login_Id { get; set; }
    }


    public class ShopOwnerWithLoginClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class ShopOwnerWithLoginIdClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Login_Id { get; set; }
    }


    public class CustomerWithLoginClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class CustomerWithLoginIdClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Login_Id { get; set; }
    }



    public class ShopClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long ShopOwnerId { get; set; }
    }

    public class ShopRequestClass
    {
        public long ShopId { get; set; }
        public long ShopOwnerId { get; set; }
    }

    public class ApproveRequestClass
    {
        public long Id { get; set; }
    }

    public class ProductClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int UserRating { get; set; }
        public long BrandId { get; set; }
        public string DeliveryTime { get; set; }
        public string ShopId { get; set; }
    }

    public class BrandClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class ShoppingCartClass
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long CustomerId { get; set; }
    }

    public class CustomerIdClass
    {
        public long CustomerId { get; set; }
    }

    public class ShoppingCartIdClass
    {
        public long ShoppingCartId { get; set; }
    }

    public class ShopIdClass
    {
        public long ShopId { get; set; }
    }

    public class ProductByBrandClass
    {
        public long ShopId { get; set; }
        public string BrandIdCsv { get; set; }
        public string OrderByClause { get; set; }
    }

    public class ProductByPriceClass
    {
        public long ShopId { get; set; }
        public double Price { get; set; }
        public string OrderByClause { get; set; }
    }

    public class ProductByRatingClass
    {
        public long ShopId { get; set; }
        public long Rating { get; set; }
        public string OrderByClause { get; set; }
    }

    public class ProductByBrandPriceAndRatingClass
    {
        public long ShopId { get; set; }
        public string BrandIdCsv { get; set; }
        public double Price { get; set; }
        public long Rating { get; set; }
        public string OrderByClause { get; set; }
    }


}