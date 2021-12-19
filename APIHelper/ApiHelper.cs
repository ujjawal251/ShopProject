using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Ajax.Utilities;
using System.Net;
using ShopProject.API;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web.WebSockets;

namespace ShopProject
{
    public class ApiHelper
    {

        #region SQL Connection

        private string zConnectionStringL = ConfigurationManager.AppSettings["ConnectionString"].ToString();

        private DataTable GetData(SqlCommand SqlCommandP)
        {
            DataTable DataTableL = new DataTable();
            SqlConnection SqlConnectionL = new SqlConnection(zConnectionStringL);
            try
            {
                SqlDataAdapter sda = new SqlDataAdapter();
                SqlCommandP.CommandType = CommandType.Text;
                SqlCommandP.Connection = SqlConnectionL;
                SqlConnectionL.Open();
                sda.SelectCommand = SqlCommandP;
                sda.Fill(DataTableL);
            }
            catch (Exception e)
            {
                // 
            }
            finally
            {
                if (SqlConnectionL != null && SqlConnectionL.State != ConnectionState.Closed)
                {
                    SqlConnectionL.Close();
                }
            }
            return DataTableL;
        }

        #endregion

        #region Write Response

        public static void WriteSuccessResponse(HttpContext ContextP, Result aResultP, string zMessageP)
        {
            ContextP.Response.ContentType = "text/json";
            ContextP.Response.StatusCode = (int)aResultP.ErrorCode;
            ContextP.Response.Write(zMessageP);
            return;
        }

        public static void WriteFailedResponse(HttpContext ContextP, Result aResultP)
        {
            ContextP.Response.ContentType = "text/json";
            ContextP.Response.StatusCode = (int)aResultP.ErrorCode;
            ContextP.Response.Write(aResultP.ShortErrorMessage);
            return;
        }

        #endregion

        #region API

        #region Login

        public Result CreateLogin(LoginClass LoginClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(LoginClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Login POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(LoginClassP.Login))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to signup. Login is blank.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(LoginClassP.Password))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to signup. Password is blank.");
                    return aResultL;
                }
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_Login_By_Login N'" + LoginClassP.Login + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                if(DataTableL.Rows.Count > 0)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.OK, "Failed to create login as login '" + LoginClassP.Login + "' is already present.");
                    return aResultL;
                }

                zQueryL = "EXEC dbo.sp_Create_Login N'" + LoginClassP.Login + "', N'" + LoginClassP.Password + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = LoginClassP.Login + " login created successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result DeleteLogin(string zLoginP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(string.IsNullOrEmpty(zLoginP))
                {
                    return aResultL;
                }
                DataTable DataTableL = new DataTable();
                string zQueryL = "DELETE FROM Login where Login = N'" + zLoginP + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = "Login Deleted successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }
        public Result GetLoginInfoByLogin(string zLoginP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_Login_By_Login N'" + zLoginP + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result LoginIntoSystem(LoginClass LoginClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (LoginClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Login POST data not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(LoginClassP.Login))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to login. Login id is blank.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(LoginClassP.Password))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to login. Password is blank.");
                    return aResultL;
                }

                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_Login_By_Login N'" + LoginClassP.Login + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                if(DataTableL.Rows.Count == 0)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.Unauthorized, "Failed to login. Login is not found.");
                    return aResultL;
                }
                if(DataTableL.Rows[0]["Password"].ToString() != LoginClassP.Password)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.Unauthorized, "Failed to login. Login or password is wrong.");
                    return aResultL;
                }

                aResultL = CreateActiveLogin(LoginClassP, out zResponseP);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }

            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        private Result CreateActiveLogin(LoginClass LoginClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                string LoginL = LoginClassP.Login;
                string SessionKeyL = Guid.NewGuid().ToString();
                DateTime ExpirationDateTimeL = DateTime.Now;
                ExpirationDateTimeL = ExpirationDateTimeL.AddDays(1);

                string zExpirationDateTimeL = ExpirationDateTimeL.ToString("yyyy/MM/dd HH:mm:ss.fff");

                string zQueryL = "EXEC dbo.sp_Create_Active_Login N'" + LoginL + "', N'" + SessionKeyL + "', N'" + zExpirationDateTimeL + "'";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zQueryL = "SELECT TOP 1 Id, Login, Session_Key as Token, Expiration_Time as ExpirationTime from Active_Logins where Login = N'" + LoginL + "' order by Id DESC";
                DataTableL = new DataTable();
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result GetActiveLoginInfoByToken(string zTokenP, out ActiveLoginClass ActiveLoginClassP)
        {
            Result aResultL = new Result();
            ActiveLoginClassP = null;
            try
            {
                string zQueryL = "SELECT TOP 1 Id, Login, Session_Key as Token, Expiration_Time as ExpirationTime FROM Active_Logins where Session_Key = N'" + zTokenP + "' order by Id DESC";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                ActiveLoginClassP = Utility.ConvertDataTableToObject<ActiveLoginClass>(DataTableL);
            }
            catch (Exception ex)
            {
                ActiveLoginClassP = null;
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result ValidateRequest(HttpRequest HttpRequestP)
        {
            Result aResultL = new Result();
            try
            {
                Dictionary<string, string> HeadersListL = Utility.GetHeadersDictionary(HttpRequestP);
                if(!HeadersListL.ContainsKey("token"))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Invalid request. Token not found.");
                    return aResultL;
                }
                string zTokenL;
                HeadersListL.TryGetValue("token", out zTokenL);
                aResultL = ValidateToken(zTokenL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Invalid request. Message : " + ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        private Result ValidateToken(string zTokenP)
        {
            Result aResultL = new Result();
            try
            {
                if(string.IsNullOrEmpty(zTokenP))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Invalid request. Token not found.");
                    return aResultL;
                }

                ActiveLoginClass ActiveLoginClassL = null;
                aResultL = GetActiveLoginInfoByToken(zTokenP, out ActiveLoginClassL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }

                if(ActiveLoginClassL != null)
                {
                    if(!string.IsNullOrEmpty(ActiveLoginClassL.Id.ToString()))
                    {
                        DateTime ExpirationDateTimeL = new DateTime();
                        ExpirationDateTimeL = ExpirationDateTimeL.AddDays(1);
                        string zExpirationDateTimeL = ExpirationDateTimeL.ToString("yyyy/MM/dd HH:mm:ss.fff");

                        string zQueryL = "UPDATE Active_Logins SET Expiration_Time = N'" + zExpirationDateTimeL + "' where Id = N'" + ActiveLoginClassL.Id + "'";
                        DataTable DataTableL = new DataTable();
                        SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                        DataTableL = GetData(SqlCommandL);
                    }
                }
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Invalid request. Message :" + ex.Message);
                return aResultL;
            }
            return aResultL;
        }


        public Result LogOutFromSystem(HttpContext ContextP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                string zTokenL = Utility.GetTokenFromHeaderList(Utility.GetHeadersDictionary(ContextP.Request));
                if(string.IsNullOrEmpty(zTokenL))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Invalid request. Token not found.");
                    return aResultL;
                }

                ActiveLoginClass ActiveLoginClassL = null;
                aResultL = GetActiveLoginInfoByToken(zTokenL, out ActiveLoginClassL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }
                if(ActiveLoginClassL == null)
                {
                    zResponseP = "Logged out successfully.";
                    return aResultL;
                }

                string zQueryL = "EXEC dbo.sp_Delete_Active_Login N'" + ActiveLoginClassL.Id.ToString() + "'";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = "Logged out successfully.";
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        #endregion

        #region System Admin

        public Result GetSystemAdminList(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_System_Admin";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetSystemAdminInfoById(string zIdP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            if (string.IsNullOrEmpty(zIdP))
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "System Admin Id not found.");
                return aResultL;
            }
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_System_Admin_By_Id " + zIdP;
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result CreateSystemAdmin(SystemAdminWithLoginClass SystemAdminClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(SystemAdminClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "System Admin POST data not found.");
                    return aResultL;
                }

                LoginClass LoginClassL = new LoginClass();
                LoginClassL.Login = SystemAdminClassP.Login;
                LoginClassL.Password = SystemAdminClassP.Password;

                string zResponseL = "";
                aResultL = CreateLogin(LoginClassL, out zResponseL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }

                zResponseL = "";
                aResultL = GetLoginInfoByLogin(LoginClassL.Login, out zResponseL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }

                LoginClassL = new LoginClass();
                LoginClassL = Utility.ConvertJsonToObject<LoginClass>(zResponseL);

                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Create_System_Admin N'" + SystemAdminClassP.Name + "', N'" + LoginClassL.Login + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = SystemAdminClassP.Name + " system admin created successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result UpdateSystemAdmin(SystemAdminWithLoginClass SystemAdminClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(SystemAdminClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to update system admin. System Admin POST data not found");
                    return aResultL;
                }
                string zQueryL = "";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL;
                if (!string.IsNullOrEmpty(SystemAdminClassP.Login))
                {
                    DataTableL = new DataTable();
                    zQueryL = "SELECT * from System_Admin where Id = N'" + SystemAdminClassP.Id + "'";
                    SqlCommandL = new SqlCommand(zQueryL);
                    DataTableL = GetData(SqlCommandL);

                    if(DataTableL.Rows[0]["Login_Id"].ToString() != SystemAdminClassP.Login)
                    {
                        //zQueryL = "UPDATE "       //DO_LATER
                    }
                }
                zQueryL = "UPDATE System_Admin SET Name = N'" + SystemAdminClassP.Name + "' WHERE Id = N'" + SystemAdminClassP.Id + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = SystemAdminClassP.Name + " system admin updated successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result DeleteSystemAdmin(DeleteClass DeleteClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(DeleteClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete System Admin.");
                    return aResultL;
                }

                DataTable DataTableL = new DataTable();
                string zQueryL = "SELECT * from System_Admin where Id = N'" + DeleteClassP.Id + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                SystemAdminWithLoginIdClass SystemNameWithLoginIdClassL = Utility.ConvertDataTableToObject<SystemAdminWithLoginIdClass>(DataTableL);

                if(SystemNameWithLoginIdClassL == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to parsed System admin data.");
                    return aResultL;
                }

                string zResponseL = "";
                aResultL = DeleteLogin(SystemNameWithLoginIdClassL.Login_Id, out zResponseL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }

                zQueryL = "DELETE FROM System_Admin where Id = N'" + DeleteClassP.Id + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = new DataTable();
                DataTableL = GetData(SqlCommandL);
                zResponseP = "System admin deleted successfully.";
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetAllShopRequest(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Shop_Request";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetAllApprovedShopRequest(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_Approved_Shop_Request";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetAllNotApprovedShopRequest(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_Not_Approved_Shop_Request";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result ApproveRequest(ApproveRequestClass ApproveRequestClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ApproveRequestClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to approve shop request. Shop Request POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ApproveRequestClassP.Id.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to approve shop request. Shop Request id not found.");
                    return aResultL;
                }
                string zQueryL = "UPDATE Shop_Request SET IsRequestApproved = N'1' WHERE Id = N'" + ApproveRequestClassP.Id.ToString() + "'";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = "Shop request approved successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        #endregion

        #region Shop Owner

        public Result GetShopOwnerList(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Shop_Owner";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetShopOwnerInfoById(string zIdP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            if (string.IsNullOrEmpty(zIdP))
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Shop owner Id not found.");
                return aResultL;
            }
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Shop_Owner_By_Id " + zIdP;
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result CreateShopOwner(ShopOwnerWithLoginClass ShopOwnerWithLoginClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (ShopOwnerWithLoginClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Shop Owner POST data not found.");
                    return aResultL;
                }

                LoginClass LoginClassL = new LoginClass();
                LoginClassL.Login = ShopOwnerWithLoginClassP.Login;
                LoginClassL.Password = ShopOwnerWithLoginClassP.Password;

                string zResponseL = "";
                aResultL = CreateLogin(LoginClassL, out zResponseL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }

                zResponseL = "";
                aResultL = GetLoginInfoByLogin(LoginClassL.Login, out zResponseL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }

                LoginClassL = new LoginClass();
                LoginClassL = Utility.ConvertJsonToObject<LoginClass>(zResponseL);

                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Create_Shop_Owner N'" + ShopOwnerWithLoginClassP.Name + "', N'" + LoginClassL.Login + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = ShopOwnerWithLoginClassP.Name + " Shop owner created successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result UpdateShopOwner(ShopOwnerWithLoginClass ShopOwnerWithLoginClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (ShopOwnerWithLoginClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to update shop owner. Shop owner POST data not found");
                    return aResultL;
                }
                string zQueryL = "";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL;
                if (!string.IsNullOrEmpty(ShopOwnerWithLoginClassP.Login))
                {
                    DataTableL = new DataTable();
                    zQueryL = "SELECT * from Shop_Owner where Id = N'" + ShopOwnerWithLoginClassP.Id + "'";
                    SqlCommandL = new SqlCommand(zQueryL);
                    DataTableL = GetData(SqlCommandL);

                    if (DataTableL.Rows[0]["Login_Id"].ToString() != ShopOwnerWithLoginClassP.Login)
                    {
                        //zQueryL = "UPDATE "       //DO_LATER
                    }
                }
                zQueryL = "UPDATE Shop_Owner SET Name = N'" + ShopOwnerWithLoginClassP.Name + "' WHERE Id = N'" + ShopOwnerWithLoginClassP.Id + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = ShopOwnerWithLoginClassP.Name + " shop owner updated successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result DeleteShopOwner(DeleteClass DeleteClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (DeleteClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete shop owner.");
                    return aResultL;
                }

                DataTable DataTableL = new DataTable();
                string zQueryL = "SELECT * from Shop_Owner where Id = N'" + DeleteClassP.Id + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                ShopOwnerWithLoginIdClass ShopOwnerWithLoginIdClassL = Utility.ConvertDataTableToObject<ShopOwnerWithLoginIdClass>(DataTableL);

                if (ShopOwnerWithLoginIdClassL == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to parsed shop owner data.");
                    return aResultL;
                }

                string zResponseL = "";
                aResultL = DeleteLogin(ShopOwnerWithLoginIdClassL.Login_Id, out zResponseL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }

                zQueryL = "DELETE FROM Shop_Owner where Id = N'" + DeleteClassP.Id + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = new DataTable();
                DataTableL = GetData(SqlCommandL);
                zResponseP = "Shop owner deleted successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }


        #endregion


        #region Customer

        public Result GetCustomerList(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Customer";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetCustomerInfoById(string zIdP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            if (string.IsNullOrEmpty(zIdP))
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Customer Id not found.");
                return aResultL;
            }
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Customer_By_Id " + zIdP;
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result CreateCustomer(CustomerWithLoginClass CustomerWithLoginClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (CustomerWithLoginClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Customer POST data not found.");
                    return aResultL;
                }

                LoginClass LoginClassL = new LoginClass();
                LoginClassL.Login = CustomerWithLoginClassP.Login;
                LoginClassL.Password = CustomerWithLoginClassP.Password;

                string zResponseL = "";
                aResultL = CreateLogin(LoginClassL, out zResponseL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }

                zResponseL = "";
                aResultL = GetLoginInfoByLogin(LoginClassL.Login, out zResponseL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }

                LoginClassL = new LoginClass();
                LoginClassL = Utility.ConvertJsonToObject<LoginClass>(zResponseL);

                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Create_Customer N'" + CustomerWithLoginClassP.Name + "', N'" + LoginClassL.Login + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = CustomerWithLoginClassP.Name + " Customer created successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result UpdateCustomer(CustomerWithLoginClass CustomerWithLoginClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (CustomerWithLoginClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to update Customer. Customer POST data not found");
                    return aResultL;
                }
                string zQueryL = "";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL;
                if (!string.IsNullOrEmpty(CustomerWithLoginClassP.Login))
                {
                    DataTableL = new DataTable();
                    zQueryL = "SELECT * from Customer where Id = N'" + CustomerWithLoginClassP.Id + "'";
                    SqlCommandL = new SqlCommand(zQueryL);
                    DataTableL = GetData(SqlCommandL);

                    if (DataTableL.Rows[0]["Login_Id"].ToString() != CustomerWithLoginClassP.Login)
                    {
                        //zQueryL = "UPDATE "       //DO_LATER
                    }
                }
                zQueryL = "UPDATE Customer SET Name = N'" + CustomerWithLoginClassP.Name + "' WHERE Id = N'" + CustomerWithLoginClassP.Id + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = CustomerWithLoginClassP.Name + " customer updated successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result DeleteCustomer(DeleteClass DeleteClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (DeleteClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete customer.");
                    return aResultL;
                }

                DataTable DataTableL = new DataTable();
                string zQueryL = "SELECT * from Customer where Id = N'" + DeleteClassP.Id + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                CustomerWithLoginIdClass CustomerWithLoginIdClassL = Utility.ConvertDataTableToObject<CustomerWithLoginIdClass>(DataTableL);

                if (CustomerWithLoginIdClassL == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to parsed customer data.");
                    return aResultL;
                }

                string zResponseL = "";
                aResultL = DeleteLogin(CustomerWithLoginIdClassL.Login_Id, out zResponseL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }

                zQueryL = "DELETE FROM Customer where Id = N'" + DeleteClassP.Id + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = new DataTable();
                DataTableL = GetData(SqlCommandL);
                zResponseP = "Customer deleted successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }


        #endregion

        #region Shop

        public Result GetShopList(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Shop";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetShopInfoById(string zIdP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            if (string.IsNullOrEmpty(zIdP))
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Shop Id not found.");
                return aResultL;
            }
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Shop_By_Id " + zIdP;
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result CreateShop(ShopClass ShopClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ShopClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop. Shop POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ShopClassP.Name))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop. Shop name not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ShopClassP.ShopOwnerId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop. Shop owner id not found.");
                    return aResultL;
                }

                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Create_Shop N'" + ShopClassP.Name + "', N'" + ShopClassP.ShopOwnerId.ToString() + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = ShopClassP.Name + " shop created successfully.";
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result UpdateShop(ShopClass ShopClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (ShopClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop. Shop POST data not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ShopClassP.Name))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop. Shop name not found.");
                    return aResultL;
                }

                string zQueryL = "UPDATE Shop SET Name = N'" + ShopClassP.Name + "' WHERE Id = N'" + ShopClassP.Id.ToString() + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTable DataTableL = new DataTable();
                DataTableL = GetData(SqlCommandL);
                zResponseP = ShopClassP.Name + " shop updated successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result DeleteShop(DeleteClass DeleteClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(DeleteClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete Shop. Shop POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(DeleteClassP.Id.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete Shop. Shop Id not found.");
                    return aResultL;
                }

                DataTable ShopRequestDTL;
                aResultL = ReadShopRequestForShop(DeleteClassP.Id.ToString(), out ShopRequestDTL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }

                if(ShopRequestDTL.Rows.Count > 0)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete shop as this shop is using by " + ShopRequestDTL.Rows.Count.ToString() + " 'Shop Request' object.");
                    return aResultL;
                }

                DataTable ProductDtL;
                aResultL = GetProductListByShopId(DeleteClassP.Id.ToString(), out ProductDtL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }

                if (ProductDtL.Rows.Count > 0)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete shop as this shop is using by " + ProductDtL.Rows.Count.ToString() + " 'Product' object.");
                    return aResultL;
                }

                string zQueryL = "DELETE FROM Shop WHERE Id = N'" + DeleteClassP.Id.ToString() + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTable DataTableL = new DataTable();
                DataTableL = GetData(SqlCommandL);
                zResponseP = "Shop deleted successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result ReadShopRequestForShop(string zShopIdP, out DataTable ShopRequestDatatableP)
        {
            Result aResultL = new Result();
            ShopRequestDatatableP = new DataTable();
            try
            {
                if(string.IsNullOrEmpty(zShopIdP))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Shop id is blank.");
                    return aResultL;
                }

                string zQueryL = "EXEC dbo.sp_Get_Shop_Request_By_Shop_Id N'" + zShopIdP + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                ShopRequestDatatableP = GetData(SqlCommandL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetProductListByShop(ShopIdClass ShopIdClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ShopIdClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get product list by shop.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ShopIdClassP.ShopId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get product list by shop. Shop id is blank.");
                    return aResultL;
                }
                DataTable ProductListDtL = new DataTable();
                aResultL = GetProductListByShopId(ShopIdClassP.ShopId.ToString(), out ProductListDtL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }

                zResponseP = Utility.DataTableToJSON(ProductListDtL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }
        public Result GetProductListByShopId(string zShopIdP, out DataTable ProductListP)
        {
            Result aResultL = new Result();
            ProductListP = new DataTable();
            try
            {
                if (string.IsNullOrEmpty(zShopIdP))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Shop id is blank.");
                    return aResultL;
                }
                string zQueryL = "EXEC dbo.sp_Get_Products_By_Shop_Id N'" + zShopIdP + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                ProductListP = GetData(SqlCommandL);

            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result CreateShopRequest(ShopRequestClass ShopRequestClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ShopRequestClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop request. Shop Request POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ShopRequestClassP.ShopId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop request. Shop Id not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ShopRequestClassP.ShopOwnerId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create shop request. Shop Owner Id not found.");
                    return aResultL;
                }

                string zQueryL = "EXEC dbo.sp_Create_Shop_Request N'" + ShopRequestClassP.ShopOwnerId + "', N'" + ShopRequestClassP.ShopId + "', N'0'";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = "Shop request created successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result CreateProduct(ProductClass ProductClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ProductClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create product. Product POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ProductClassP.Name))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create product. Product name not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ProductClassP.Price.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create product. Product price not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ProductClassP.ShopId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to create product. Shop Id not found.");
                    return aResultL;
                }
                string zQueryL = "INSERT INTO Product ";
                string zNameL = "(Name, Price";
                string zValueL = "(N'" + ProductClassP.Name + "', N'" + ProductClassP.Price.ToString() + "'";
                if (!string.IsNullOrEmpty(ProductClassP.UserRating.ToString()))
                {
                    zNameL += ", User_Rating";
                    zValueL += ", N'" + ProductClassP.UserRating.ToString() + "'";
                }
                if(!string.IsNullOrEmpty(ProductClassP.BrandId.ToString()))
                {
                    zNameL += ", Brand_Id";
                    zValueL += ", N'" + ProductClassP.BrandId.ToString() + "'";
                }
                if(!string.IsNullOrEmpty(ProductClassP.DeliveryTime))
                {
                    zNameL += ", Delivery_Time";
                    zValueL += ", N'" + ProductClassP.DeliveryTime + "'";
                }
                if (!string.IsNullOrEmpty(ProductClassP.ShopId))
                {
                    zNameL += ", Shop_Id";
                    zValueL += ", N'" + ProductClassP.ShopId + "'";
                }
                zNameL += ")";
                zValueL += ")";

                zQueryL += zNameL;
                zQueryL += " VALUES ";
                zQueryL += zValueL;

                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = "Product created successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result UpdateProduct(ProductClass ProductClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (ProductClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to update product. Product POST data not found.");
                    return aResultL;
                }
                string zQueryL = "UPDATE Product SET ";
                if(!string.IsNullOrEmpty(ProductClassP.Name))
                {
                    zQueryL += "Name = N'" + ProductClassP.Name + "'";
                }
                if(!string.IsNullOrEmpty(ProductClassP.Price.ToString()))
                {
                    zQueryL += ", Price = N'" + ProductClassP.Price.ToString() + "'";
                }
                if(!string.IsNullOrEmpty(ProductClassP.UserRating.ToString()))
                {
                    zQueryL += ", User_Rating = N'" + ProductClassP.UserRating.ToString() + "'";
                }
                if(!string.IsNullOrEmpty(ProductClassP.BrandId.ToString()))
                {
                    zQueryL += ", Brand_Id = N'" + ProductClassP.BrandId.ToString() + "'";
                }
                if(!string.IsNullOrEmpty(ProductClassP.DeliveryTime))
                {
                    zQueryL += ", Delivery_Time = N'" + ProductClassP.DeliveryTime + "'";
                }
                if(!string.IsNullOrEmpty(ProductClassP.ShopId.ToString()))
                {
                    zQueryL += ", Shop_Id = N'" + ProductClassP.ShopId.ToString() + "'";
                }
                zQueryL += " WHERE Id = N'" + ProductClassP.Id + "'";

                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = "Product updated successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result DeleteProduct(DeleteClass DeleteClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(DeleteClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete Product. Product POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(DeleteClassP.Id.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete Product. Product id not found.");
                    return aResultL;
                }
                DataTable DataTableL = new DataTable();
                string zQueryL = "SELECT Shopping_Cart.Id FROM Shopping_Cart WHERE Shopping_Cart.Product_Id = N'" + DeleteClassP.Id + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                if(DataTableL.Rows.Count > 0)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete Product as this product is using by " + DataTableL.Rows.Count.ToString() + " Shopping cart object.");
                    return aResultL;
                }

                DataTableL = new DataTable();
                zQueryL = "SELECT Check_Out_Products.Id FROM Check_Out_Products WHERE Check_Out_Products.Product_Id = N'" + DeleteClassP.Id + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                if (DataTableL.Rows.Count > 0)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete Product as this product is using by " + DataTableL.Rows.Count.ToString() + " Check Out Products object.");
                    return aResultL;
                }

                DataTableL = new DataTable();
                zQueryL = "DELETE FROM Product WHERE Id = N'" + DeleteClassP.Id.ToString() + "'";
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result AddProductToCart(ShoppingCartClass ShoppingCartP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ShoppingCartP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to add item to cart. Shopping Cart POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ShoppingCartP.ProductId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to add item to cart. Product Id not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ShoppingCartP.CustomerId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to add item to cart. Customer Id not found.");
                    return aResultL;
                }

                string zQueryL = "INSERT INTO Shopping_Cart (Product_Id, Customer_Id) VALUES (N'" + ShoppingCartP.ProductId.ToString() + "', N'" + ShoppingCartP.CustomerId.ToString() + "')";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = "Product added to cart successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        //public Result RemoveProductFromCart(CustomerIdClass CustomerIdClassP, out string zResponseP)
        //{
        //    Result aResultL = new Result();
        //    zResponseP = "";
        //    try
        //    {
        //        if(CustomerIdClassP == null)
        //        {
        //            aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to Get cart products. Customer Id POST data not found.");
        //            return aResultL;
        //        }
        //        if(string.IsNullOrEmpty(CustomerIdClassP.CustomerId.ToString()))
        //        {
        //            aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get cart products. Customer Id is blank.");
        //            return aResultL;
        //        }

        //        string zQueryL = "EXEC dbo.sp_Get_Cart_Products_By_Customer_Id N'" + CustomerIdClassP.CustomerId + "'";
        //        DataTable DataTableL = new DataTable();
        //        SqlCommand SqlCommandL = new SqlCommand(zQueryL);
        //        DataTableL = GetData(SqlCommandL);

        //        zResponseP = Utility.DataTableToJSON(DataTableL);
        //    }
        //    catch (Exception ex)
        //    {
        //        aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
        //        return aResultL;
        //    }
        //    return aResultL;
        //}

        public Result GetCartProductsByCustomer(CustomerIdClass CustomerIdClassP, out DataTable ProductListP)
        {
            Result aResultL = new Result();
            ProductListP = new DataTable();
            try
            {
                if (CustomerIdClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to Get cart products. Customer Id POST data not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(CustomerIdClassP.CustomerId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get cart products. Customer Id is blank.");
                    return aResultL;
                }

                string zQueryL = "EXEC dbo.sp_Get_Cart_Products_By_Customer_Id N'" + CustomerIdClassP.CustomerId + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                ProductListP = GetData(SqlCommandL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetCartProductsByCustomer(CustomerIdClass CustomerIdClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                aResultL = GetCartProductsByCustomer(CustomerIdClassP, out DataTableL);
                if(aResultL.HasFailed())
                {
                    return aResultL;
                }
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }
        public Result RemoveProductFromCart(ShoppingCartIdClass ShoppingCartIdClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ShoppingCartIdClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to remove product from shopping cart.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ShoppingCartIdClassP.ShoppingCartId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to remove product from shopping cart. shopping cart item id not found.");
                    return aResultL;
                }

                string zQueryL = "DELETE FROM Shopping_Cart WHERE Id = N'" + ShoppingCartIdClassP.ShoppingCartId.ToString() + "'";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = "Product removed from cart successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetProductListByBrands(ProductByBrandClass ProductByBrandClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ProductByBrandClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Product List By Brand POST data not found.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ProductByBrandClassP.ShopId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Shop id is blank.");
                    return aResultL;
                }
                if(string.IsNullOrEmpty(ProductByBrandClassP.BrandIdCsv.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Brands Id not found.");
                    return aResultL;
                }
                string zBrandIdCsvL = ProductByBrandClassP.BrandIdCsv.ToString();
                string[] BrandIdArrayL = zBrandIdCsvL.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string zBrandIdInOperatorQueryL = "";
                if(BrandIdArrayL.Length > 0)
                {
                    zBrandIdInOperatorQueryL += "(";
                    foreach(string zBrandIdL in BrandIdArrayL)
                    {
                        if(zBrandIdInOperatorQueryL.Length > 1)
                        {
                            zBrandIdInOperatorQueryL += ", N'" + zBrandIdL + "'";
                        }
                        else
                        {
                            zBrandIdInOperatorQueryL += "N'" + zBrandIdL + "'";
                        }
                    }
                    zBrandIdInOperatorQueryL += ")";
                }

                string zQueryL = "select Product.Id as ProductId, Product.Name as ProductName, Product.Price, Product.User_Rating, Product.Delivery_Time, Brand.Id as BrandId, Brand.Name as BrandName from Product left join Brand on Product.Brand_Id = Brand.Id  where ((Product.Shop_Id = N'" + ProductByBrandClassP.ShopId.ToString() + "') AND (Product.Brand_Id IN " + zBrandIdInOperatorQueryL + "))";
                if(!string.IsNullOrEmpty(ProductByBrandClassP.OrderByClause))
                {
                    zQueryL += " ORDER BY " + ProductByBrandClassP.OrderByClause;
                }
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetProductsByPriceGreaterThanQueryValue(ProductByPriceClass ProductByPriceClassP, out string zResponseP)
        {
            return GetProductsByPriceInternal(ProductByPriceClassP, true, out zResponseP);
        }

        public Result GetProductsByPriceLessThanQueryValue(ProductByPriceClass ProductByPriceClassP, out string zResponseP)
        {
            return GetProductsByPriceInternal(ProductByPriceClassP, false, out zResponseP);
        }

        private Result GetProductsByPriceInternal(ProductByPriceClass ProductByPriceClassP, bool bIsGreaterThanP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if(ProductByPriceClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get product list. Product List By Price POST data not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ProductByPriceClassP.ShopId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Shop id is blank.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ProductByPriceClassP.Price.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Price not found.");
                    return aResultL;
                }
                string IsGreaterThanL = ">";
                if(!bIsGreaterThanP)
                {
                    IsGreaterThanL = "<";
                }

                string zQueryL = "select Product.Id as ProductId, Product.Name as ProductName, Product.Price, Product.User_Rating, Product.Delivery_Time, Brand.Id as BrandId, Brand.Name as BrandName from Product left join Brand on Product.Brand_Id = Brand.Id  where ((Product.Shop_Id = N'" + ProductByPriceClassP.ShopId.ToString() + "') AND (Product.Price " + IsGreaterThanL + " N'" + ProductByPriceClassP.Price.ToString() + "'))";
                if (!string.IsNullOrEmpty(ProductByPriceClassP.OrderByClause))
                {
                    zQueryL += " ORDER BY " + ProductByPriceClassP.OrderByClause;
                }

                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);

            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetProductListByUserRating(ProductByRatingClass ProductByRatingClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (ProductByRatingClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Product List By rating POST data not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ProductByRatingClassP.ShopId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Shop id is blank.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(ProductByRatingClassP.Rating.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Rating not found.");
                    return aResultL;
                }

                string zQueryL = "select Product.Id as ProductId, Product.Name as ProductName, Product.Price, Product.User_Rating, Product.Delivery_Time, Brand.Id as BrandId, Brand.Name as BrandName from Product left join Brand on Product.Brand_Id = Brand.Id where ((Product.Shop_Id = N'" + ProductByRatingClassP.ShopId.ToString() + "') AND (Product.User_Rating >=  N'" + ProductByRatingClassP.Rating.ToString() + "'))";
                if (!string.IsNullOrEmpty(ProductByRatingClassP.OrderByClause))
                {
                    zQueryL += " ORDER BY " + ProductByRatingClassP.OrderByClause;
                }

                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetProductsByBrandPriceAndRating(ProductByBrandPriceAndRatingClass ProductByBrandPriceAndRatingClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (ProductByBrandPriceAndRatingClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Product List filter POST data not found.");
                    return aResultL;
                }

                if(string.IsNullOrEmpty(ProductByBrandPriceAndRatingClassP.ShopId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to get Product list. Shop id not found.");
                    return aResultL;
                }

                string zSelectClauseL = "Product.Id as ProductId, Product.Name as ProductName, Product.Price, Product.User_Rating, Product.Delivery_Time, Brand.Id as BrandId, Brand.Name as BrandName ";
                string zOrderByClauseL = "";
                if(!string.IsNullOrEmpty(ProductByBrandPriceAndRatingClassP.OrderByClause))
                {
                    zOrderByClauseL = ProductByBrandPriceAndRatingClassP.OrderByClause;
                }

                string zSearchConditionClauseL = "";
                string zBrandSearconditionL = "";
                if(!string.IsNullOrEmpty(ProductByBrandPriceAndRatingClassP.BrandIdCsv.ToString()))
                {
                    string zBrandIdCsvL = ProductByBrandPriceAndRatingClassP.BrandIdCsv.ToString();
                    string[] BrandIdArrayL = zBrandIdCsvL.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string zBrandIdInOperatorQueryL = "";
                    if (BrandIdArrayL.Length > 0)
                    {
                        zBrandIdInOperatorQueryL += "(";
                        foreach (string zBrandIdL in BrandIdArrayL)
                        {
                            if (zBrandIdInOperatorQueryL.Length > 1)
                            {
                                zBrandIdInOperatorQueryL += ", N'" + zBrandIdL + "'";
                            }
                            else
                            {
                                zBrandIdInOperatorQueryL += "N'" + zBrandIdL + "'";
                            }
                        }
                        zBrandIdInOperatorQueryL += ")";
                    }

                    if (zBrandIdInOperatorQueryL.Length > 0)
                    {
                        zBrandSearconditionL += "( Product.Brand_Id IN " + zBrandIdInOperatorQueryL + ")";
                    }
                }
                

                string zPriceSearchConditionL = "";
                if(!string.IsNullOrEmpty(ProductByBrandPriceAndRatingClassP.Price.ToString()))
                {
                    zPriceSearchConditionL = "(Product.Price < N'" + ProductByBrandPriceAndRatingClassP.Price.ToString() + "')";
                }

                string zRatingSearchConditionL = "";
                if(!string.IsNullOrEmpty(ProductByBrandPriceAndRatingClassP.Rating.ToString()))
                {
                    zRatingSearchConditionL = "(Product.User_Rating > N'" + ProductByBrandPriceAndRatingClassP.Rating.ToString() + "')";
                }

                string zShopIdSearchConditionL = "( Product.Shop_Id = N'" + ProductByBrandPriceAndRatingClassP.ShopId.ToString() + "')";

                zSearchConditionClauseL += zShopIdSearchConditionL;

                if(!string.IsNullOrEmpty(zBrandSearconditionL))
                {
                    zSearchConditionClauseL += " AND " + zBrandSearconditionL;
                }
                if(!string.IsNullOrEmpty(zPriceSearchConditionL))
                {
                    zSearchConditionClauseL += " AND " + zPriceSearchConditionL;
                }

                if(!string.IsNullOrEmpty(zRatingSearchConditionL))
                {
                    zSearchConditionClauseL += " AND " + zRatingSearchConditionL;
                }

                string zQueryL = "SELECT " + zSelectClauseL + " FROM Product left join Brand on Product.Brand_Id = Brand.Id WHERE (" + zSearchConditionClauseL + ")";
                if(!string.IsNullOrEmpty(zOrderByClauseL))
                {
                    zQueryL += " ORDER BY " + zOrderByClauseL;
                }

                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }



        #endregion


        #region Check Out

        public Result CreateCheckOut(CustomerIdClass CustomerIdClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (CustomerIdClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to check out. Customer Id POST data not found.");
                    return aResultL;
                }
                if (string.IsNullOrEmpty(CustomerIdClassP.CustomerId.ToString()))
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to check out. Customer Id is blank.");
                    return aResultL;
                }

                DataTable ProductListL = new DataTable();
                aResultL = GetCartProductsByCustomer(CustomerIdClassP, out ProductListL);
                if (aResultL.HasFailed())
                {
                    return aResultL;
                }
                if(ProductListL.Rows.Count == 0)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to check out. Cart is empty.");
                    return aResultL;
                }

                double TotalCheckOutPriceL = 0;
                for(int i = 0; i < ProductListL.Rows.Count; i++)
                {
                    double nPriceL;
                    double.TryParse(ProductListL.Rows[i]["Price"].ToString(), out nPriceL);
                    TotalCheckOutPriceL += nPriceL;
                }

                if(TotalCheckOutPriceL < 500)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.OK, "Failed to check out as total cart amount is less than 500.");
                    return aResultL;
                }

                string zCurrentDateTimeL = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

                string zQueryL = "INSERT INTO Check_Out (Customer_Id, Check_Out_Amount, Check_Out_DateTime) values (N'" + CustomerIdClassP.CustomerId.ToString() + "', N'" + TotalCheckOutPriceL.ToString() + "', N'" + zCurrentDateTimeL + "')";
                DataTable DataTableL = new DataTable();
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                zQueryL = "SELECT TOP 1 Id FROM Check_Out WHERE Check_Out.Customer_Id = N'" + CustomerIdClassP.CustomerId.ToString() + "' order by Id DESC";
                DataTableL = new DataTable();
                SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);

                string zCheckOutIdL = "";
                if(DataTableL.Rows.Count > 0)
                {
                    zCheckOutIdL = DataTableL.Rows[0]["Id"].ToString();
                }

                if(!string.IsNullOrEmpty(zCheckOutIdL))
                {
                    for (int i = 0; i < ProductListL.Rows.Count; i++)
                    {
                        string zProductIdL = ProductListL.Rows[i]["ProductId"].ToString();
                        if(string.IsNullOrEmpty(zProductIdL))
                        {
                            continue;
                        }
                        zQueryL = "INSERT INTO Check_Out_Products (Product_Id, Check_Out_Id) VALUES (N'" + zProductIdL + "', N'" + zCheckOutIdL + "')";
                        DataTableL = new DataTable();
                        SqlCommandL = new SqlCommand(zQueryL);
                        DataTableL = GetData(SqlCommandL);
                    }
                }

                zResponseP = "Check out successfully completed.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }


        #endregion





        #region Brand

        public Result GetBrandList(out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Brand";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result GetBrandInfoById(string zIdP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            if (string.IsNullOrEmpty(zIdP))
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Brand Id not found.");
                return aResultL;
            }
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Get_All_Brand_By_Id " + zIdP;
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = Utility.DataTableToJSON(DataTableL);
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }

        public Result CreateBrand(BrandClass BrandClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Create_Brand N'" + BrandClassP.Name + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = BrandClassP.Name + " brand created successfully.";
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result UpdateBrand(BrandClass BrandClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                DataTable DataTableL = new DataTable();
                string zQueryL = "EXEC dbo.sp_Update_Brand N'" + BrandClassP.Name + "', " + BrandClassP.Id;

                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTableL = GetData(SqlCommandL);
                zResponseP = BrandClassP.Name + " brand updated successfully.";
            }
            catch (Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
            }
            return aResultL;
        }

        public Result DeleteBrand(DeleteClass DeleteClassP, out string zResponseP)
        {
            Result aResultL = new Result();
            zResponseP = "";
            try
            {
                if (DeleteClassP == null)
                {
                    aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to delete Brand.");
                    return aResultL;
                }
                string zQueryL = "DELETE FROM System_Admin where Id = N'" + DeleteClassP.Id + "'";
                SqlCommand SqlCommandL = new SqlCommand(zQueryL);
                DataTable DataTableL = new DataTable();
                DataTableL = GetData(SqlCommandL);
                zResponseP = "Brand deleted successfully.";
            }
            catch(Exception ex)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, ex.Message);
                return aResultL;
            }
            return aResultL;
        }


        #endregion





        #endregion

    }
}