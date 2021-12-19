using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ShopProject.API
{
    /// <summary>
    /// Summary description for ShopOwner
    /// </summary>
    public class ShopOwner : IHttpHandler
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
                string HttpMethodL = RequestL.HttpMethod;

                if (HttpMethodL.ToUpper() == "GET")
                {
                    aResultL = ApiHelperL.ValidateRequest(RequestL);
                    if (aResultL.HasFailed())
                    {
                        ApiHelper.WriteFailedResponse(ContextP, aResultL);
                        return;
                    }
                    if (RequestL.QueryString.Count > 0)
                    {
                        aResultL = ApiHelperL.GetShopOwnerInfoById(RequestL.QueryString["Id"], out zResponseL);
                        if (aResultL.HasFailed())
                        {
                            ApiHelper.WriteFailedResponse(ContextP, aResultL);
                            return;
                        }
                    }
                    else
                    {
                        aResultL = ApiHelperL.GetShopOwnerList(out zResponseL);
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
                    switch (zMethodNameL.ToUpper())
                    {
                        case "CREATESHOPOWNER":
                            {
                                ShopOwnerWithLoginClass ShopOwnerWithLoginClassL = Utility.ConvertJsonToObject<ShopOwnerWithLoginClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.CreateShopOwner(ShopOwnerWithLoginClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "UPDATESHOPOWNER":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                ShopOwnerWithLoginClass ShopOwnerWithLoginClassL = Utility.ConvertJsonToObject<ShopOwnerWithLoginClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.UpdateShopOwner(ShopOwnerWithLoginClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "DELETESHOPOWNER":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                DeleteClass DeleteClassL = Utility.ConvertJsonToObject<DeleteClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.DeleteShopOwner(DeleteClassL, out zResponseL);
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