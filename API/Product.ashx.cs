using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ShopProject.API
{
    /// <summary>
    /// Summary description for Product
    /// </summary>
    public class Product : IHttpHandler
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