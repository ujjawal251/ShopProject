using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;

namespace ShopProject.API
{
    /// <summary>
    /// Summary description for Shop
    /// </summary>
    public class Shop : IHttpHandler
    {

        public void ProcessRequest(HttpContext ContextP)
        {
            HttpResponse ResponseL = ContextP.Response;
            HttpRequest RequestL = ContextP.Request;
            Result aResultL = new Result();
            string zResponseL = "";
            
            try
            {
                ApiHelper ApiHelperL = new ApiHelper();
                aResultL = ApiHelperL.ValidateRequest(RequestL);
                if (aResultL.HasFailed())
                {
                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                    return;
                }
                string HttpMethodL = RequestL.HttpMethod;
                if (HttpMethodL.ToUpper() == "GET")
                {
                    if (RequestL.QueryString.Count > 0)
                    {
                        aResultL = ApiHelperL.GetShopInfoById(RequestL.QueryString["Id"], out zResponseL);
                        if (aResultL.HasFailed())
                        {
                            ApiHelper.WriteFailedResponse(ContextP, aResultL);
                            return;
                        }
                    }
                    else
                    {
                        aResultL = ApiHelperL.GetShopList(out zResponseL);
                        if (aResultL.HasFailed())
                        {
                            ApiHelper.WriteFailedResponse(ContextP, aResultL);
                            return;
                        }
                    }
                }
                else
                {
                    string zMethodNameL = "";
                    string zPOSTJsonL = "";
                    aResultL = Utility.ExtractMethodNameAndPostData(RequestL, out zMethodNameL, out zPOSTJsonL);
                    if (aResultL.HasFailed())
                    {
                        ApiHelper.WriteFailedResponse(ContextP, aResultL);
                        return;
                    }
                    switch(zMethodNameL.ToUpper())
                    {
                        case "CREATESHOP":
                            {
                                ShopClass ShopClassL = Utility.ConvertJsonToObject<ShopClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.CreateShop(ShopClassL, out zResponseL);
                                if(aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "UPDATESHOP":
                            {
                                ShopClass ShopClassL = Utility.ConvertJsonToObject<ShopClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.UpdateShop(ShopClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "DELETESHOP":
                            {
                                DeleteClass DeleteClassL = Utility.ConvertJsonToObject<DeleteClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.DeleteShop(DeleteClassL, out zResponseL);
                                if(aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "CREATESHOPREQUEST":
                            {
                                ShopRequestClass ShopRequestClassL = Utility.ConvertJsonToObject<ShopRequestClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.CreateShopRequest(ShopRequestClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "CREATEPRODUCT":
                            {
                                ProductClass ProductClassL = Utility.ConvertJsonToObject<ProductClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.CreateProduct(ProductClassL, out zResponseL);
                                if(aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "UPDATEPRODUCT":
                            {
                                ProductClass ProductClassL = Utility.ConvertJsonToObject<ProductClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.UpdateProduct(ProductClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "DELETEPRODUCT":
                            {
                                DeleteClass DeleteClassL = Utility.ConvertJsonToObject<DeleteClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.DeleteProduct(DeleteClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETPRODUCTBYBRAND":
                            {
                                ProductByBrandClass ProductByBrandClassL = Utility.ConvertJsonToObject<ProductByBrandClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.GetProductListByBrands(ProductByBrandClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETPRODUCTBYPRICEGREATERTHAN":
                            {
                                ProductByPriceClass ProductByPriceClassL = Utility.ConvertJsonToObject<ProductByPriceClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.GetProductsByPriceGreaterThanQueryValue(ProductByPriceClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETPRODUCTBYPRICELESSTHAN":
                            {
                                ProductByPriceClass ProductByPriceClassL = Utility.ConvertJsonToObject<ProductByPriceClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.GetProductsByPriceLessThanQueryValue(ProductByPriceClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETPRODUCTBYRATING":
                            {
                                ProductByRatingClass ProductByRatingClassL = Utility.ConvertJsonToObject<ProductByRatingClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.GetProductListByUserRating(ProductByRatingClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETPRODUCTSBYBRANDSPICERATING":
                            {
                                ProductByBrandPriceAndRatingClass ProductByBrandPriceAndRatingClassL = Utility.ConvertJsonToObject<ProductByBrandPriceAndRatingClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.GetProductsByBrandPriceAndRating(ProductByBrandPriceAndRatingClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;

                            }
                        default:
                            {
                                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Requested Method not found.");
                                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                return;
            }
            ApiHelper.WriteSuccessResponse(ContextP, aResultL, zResponseL);
            return;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}