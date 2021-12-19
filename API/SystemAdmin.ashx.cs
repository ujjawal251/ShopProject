using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ShopProject.API
{
    /// <summary>
    /// Summary description for SystemAdmin
    /// </summary>
    public class SystemAdmin : IHttpHandler
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
                        aResultL = ApiHelperL.GetSystemAdminInfoById(RequestL.QueryString["Id"], out zResponseL);
                        if (aResultL.HasFailed())
                        {
                            ApiHelper.WriteFailedResponse(ContextP, aResultL);
                            return;
                        }
                    }
                    else
                    {
                        aResultL = ApiHelperL.GetSystemAdminList(out zResponseL);
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
                        case "CREATESYSTEMADMIN":
                            {
                                SystemAdminWithLoginClass SystemAdminWithLoginClassL = Utility.ConvertJsonToObject<SystemAdminWithLoginClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.CreateSystemAdmin(SystemAdminWithLoginClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "UPDATESYSTEMADMIN":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                SystemAdminWithLoginClass SystemAdminWithLoginClassL = Utility.ConvertJsonToObject<SystemAdminWithLoginClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.UpdateSystemAdmin(SystemAdminWithLoginClassL, out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "DELETESYSTEMADMIN":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                DeleteClass DeleteClassL = Utility.ConvertJsonToObject<DeleteClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.DeleteSystemAdmin(DeleteClassL, out zResponseL);
                                if(aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETALLSHOPREQUEST":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                aResultL = ApiHelperL.GetAllShopRequest(out zResponseL);
                                if(aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETALLAPPROVEDSHOPREQUEST":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                aResultL = ApiHelperL.GetAllApprovedShopRequest(out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "GETALLNOTAPPROVEDSHOPREQUEST":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                aResultL = ApiHelperL.GetAllNotApprovedShopRequest(out zResponseL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                break;
                            }
                        case "APPROVEREQUEST":
                            {
                                aResultL = ApiHelperL.ValidateRequest(RequestL);
                                if (aResultL.HasFailed())
                                {
                                    ApiHelper.WriteFailedResponse(ContextP, aResultL);
                                    return;
                                }
                                ApproveRequestClass ApproveRequestClassL = Utility.ConvertJsonToObject<ApproveRequestClass>(zPOSTJsonL);
                                aResultL = ApiHelperL.ApproveRequest(ApproveRequestClassL, out zResponseL);
                                if(aResultL.HasFailed())
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