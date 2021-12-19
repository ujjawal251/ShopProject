using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Net;
using System.Web;

namespace ShopProject.API
{
    /// <summary>
    /// Summary description for Login
    /// </summary>
    public class Login : IHttpHandler
    {

        public void ProcessRequest(HttpContext ContextP)
        {
            HttpResponse ResponseL = ContextP.Response;
            HttpRequest RequestL = ContextP.Request;

            string zResponseL = "";
            Result aResultL = new Result();
            try
            {
                string MethodL = RequestL.HttpMethod;
                if(MethodL.ToUpper() == "GET")
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "GET request is not allowed on login.ashx.");
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
                LoginClass LoginClassL = Utility.ConvertJsonToObject<LoginClass>(zPOSTJsonL);
                ApiHelper ApiHelperL = new ApiHelper();
                switch (zMethodNameL.ToUpper())
                {
                    case "LOGIN":
                        {
                            aResultL = ApiHelperL.LoginIntoSystem(LoginClassL, out zResponseL);
                            if(aResultL.HasFailed())
                            {
                                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                return;
                            }
                            break;
                        }
                    case "SIGNUP":
                        {
                            aResultL = ApiHelperL.CreateLogin(LoginClassL, out zResponseL);
                            if (aResultL.HasFailed())
                            {
                                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                return;
                            }
                            break;
                        }
                    case "LOGOUT":
                        {
                            aResultL = ApiHelperL.LogOutFromSystem(ContextP, out zResponseL);
                            if (aResultL.HasFailed())
                            {
                                ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                return;
                            }
                            break;
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