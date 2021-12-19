using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace ShopProject.API
{
    /// <summary>
    /// Summary description for Brand
    /// </summary>
    public class Brand : IHttpHandler
    {

        public void ProcessRequest(HttpContext ContextP)
        {
            HttpResponse ResponseL = ContextP.Response;
            HttpRequest RequestL = ContextP.Request;
            //var val = RequestL.Headers["Token"].ToString();
            string zResponseL = "";
            Result aResultL = new Result();
            try
            {
                ApiHelper ApiHelperL = new ApiHelper();
                aResultL = ApiHelperL.ValidateRequest(RequestL);
                if(aResultL.HasFailed())
                {
                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                    return;
                }
                string HttpMethodL = RequestL.HttpMethod;
                
                if (HttpMethodL.ToUpper() == "GET")
                {
                    if(RequestL.QueryString.Count > 0)
                    {
                        aResultL = ApiHelperL.GetBrandInfoById(RequestL.QueryString["Id"], out zResponseL);
                        if(aResultL.HasFailed())
                        {
                            ApiHelper.WriteFailedResponse(ContextP, aResultL);
                            return;
                        }
                    }
                    else
                    {
                        aResultL = ApiHelperL.GetBrandList(out zResponseL);
                        if(aResultL.HasFailed())
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
                    if(aResultL.HasFailed())
                    {
                        ApiHelper.WriteFailedResponse(ContextP, aResultL);
                        return;
                    }
                    switch (zMethodNameL.ToUpper())
                    {
                        case "CREATEBRAND":
                            {
                                BrandClass BrandClassL = Utility.ConvertJsonToObject<BrandClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.CreateBrand(BrandClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "UPDATEBRAND":
                            {
                                BrandClass BrandClassL = Utility.ConvertJsonToObject<BrandClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.UpdateBrand(BrandClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "DELETEBRAND":
                            {
                                DeleteClass DeleteClassL = Utility.ConvertJsonToObject<DeleteClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.DeleteBrand(DeleteClassL, out zResponseL);
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
            catch(Exception ex)
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