using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ShopProject.API
{
    /// <summary>
    /// Summary description for ShoppingCart
    /// </summary>
    public class ShoppingCart : IHttpHandler
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
                if(HttpMethodL.ToUpper() == "GET")
                {
                    aResultL.MarkAsFailed(HttpStatusCode.NotFound, "GET call is not allowed on this address.");
                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                    return;
                }
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
                    case "ADDPRODUCTTOCART":
                        {
                            ShoppingCartClass ShoppingCartClassL = Utility.ConvertJsonToObject<ShoppingCartClass>(zPOSTJsonL);
                            aResultL = ApiHelperL.AddProductToCart(ShoppingCartClassL, out zResponseL);
                            if (aResultL.HasFailed())
                            {
                                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                return;
                            }
                            break;
                        }
                    case "GETCARTPRODUCTSBYCUSTOMER":
                        {
                            CustomerIdClass CustomerIdClassL = Utility.ConvertJsonToObject<CustomerIdClass>(zPOSTJsonL);
                            aResultL = ApiHelperL.GetCartProductsByCustomer(CustomerIdClassL, out zResponseL);
                            if (aResultL.HasFailed())
                            {
                                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                return;
                            }
                            break;
                        }
                    case "REMOVEPRODUCTFROMCART":
                        {
                            ShoppingCartIdClass ShoppingCartIdClassL = Utility.ConvertJsonToObject<ShoppingCartIdClass>(zPOSTJsonL);
                            aResultL = ApiHelperL.RemoveProductFromCart(ShoppingCartIdClassL, out zResponseL);
                            if (aResultL.HasFailed())
                            {
                                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                return;
                            }
                            break;
                        }
                    case "CHECKOUT":
                        {
                            CustomerIdClass CustomerIdClassL = Utility.ConvertJsonToObject<CustomerIdClass>(zPOSTJsonL);
                            aResultL = ApiHelperL.CreateCheckOut(CustomerIdClassL, out zResponseL);
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