using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Text;
using System.Xml.Serialization;
using System.Web.Script.Serialization;
using System.Data;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace ShopProject
{
    static class Utility
    {
        public static string DataTableToJSON(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }

        public static T JSONToObject<T>(string zJsonDataP)
        {
            T ObjectL;
            using (MemoryStream JsonMemoryStreamL = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(zJsonDataP)))
            {
                DataContractJsonSerializer JsonSerializerL = new DataContractJsonSerializer(typeof(T));
                ObjectL = (T)JsonSerializerL.ReadObject(JsonMemoryStreamL);
                JsonMemoryStreamL.Close();
            }
            return ObjectL;
        }

        public static T ConvertJsonToObject<T>(string zJsonDataP)
        {

            if((zJsonDataP.StartsWith("[")) && ( zJsonDataP.EndsWith("]")))
            {
                zJsonDataP = zJsonDataP.Substring(1, zJsonDataP.Length - 2);
            }

            T ObjectL = new JavaScriptSerializer().Deserialize<T>(zJsonDataP);
            return ObjectL;
        }

        public static T ConvertDataTableToObject<T>(DataTable DataTableP)
        {
            T ObjectL;
            string zJsonL = DataTableToJSON(DataTableP);
            ObjectL = ConvertJsonToObject<T>(zJsonL);
            return ObjectL;
        }

        public static Result ExtractMethodNameAndPostData(HttpRequest RequestP, out string MethodNameP, out string PostJSONP)
        {
            Result aResultL = new Result();
            MethodNameP = "";
            PostJSONP = "";

            string zPostDataL = new System.IO.StreamReader(RequestP.InputStream).ReadToEnd();
            ApiCall ApiCallL = ConvertJsonToObject<ApiCall>(zPostDataL);
            if(ApiCallL == null)
            {
                aResultL.MarkAsFailed(HttpStatusCode.BadRequest, "Failed to parse POST data.");
                return aResultL;
            }
            MethodNameP = ApiCallL.Method;
            PostJSONP = ApiCallL.PostData;
            return aResultL;
        }

        public static Dictionary<string, string> GetHeadersDictionary(HttpRequest HttpRequestP)
        {
            Dictionary<string, string> HeadersListL = new Dictionary<string, string>();

            string[] HeadersKeyL = HttpRequestP.Headers.AllKeys;

            if(HeadersKeyL.Length > 0)
            {
                foreach(string KeyL in HeadersKeyL)
                {
                    HeadersListL.Add(KeyL, HttpRequestP.Headers[KeyL]);
                }
            }
            return HeadersListL;
        }

        public static string GetTokenFromHeaderList(Dictionary<string, string> HeadersListP)
        {
            string zTokenL = "";
            if(!HeadersListP.ContainsKey("Token"))
            {
                if(!HeadersListP.ContainsKey("token"))
                {
                    return zTokenL;
                }
                else
                {
                    HeadersListP.TryGetValue("token", out zTokenL);
                    return zTokenL;
                }
            }
            HeadersListP.TryGetValue("Token", out zTokenL);
            return zTokenL;
        }

    }


    [Serializable]
    public class Result
    {
        protected int nErrorCodeM;
        protected string zShortErrorMsgM;
        protected string zLongErrorMsgM;

        public Result()
        {
            nErrorCodeM = 200;
            ShortErrorMessage = "Success";
            zLongErrorMsgM = "Success";
        }

        public static implicit operator Result(Exception aExceptionP)
        {
            Result aResultL = new Result();
            aResultL.nErrorCodeM = -100;
            aResultL.ShortErrorMessage = "Internal System error";
            aResultL.zLongErrorMsgM = aExceptionP.ToString();

            return aResultL;
        }


        public bool HasFailed()
        {
            if (nErrorCodeM == 200)
                return false;
            return true;
        }

        public void Reset()
        {
            nErrorCodeM = 200;
            zShortErrorMsgM = "Success";
            zLongErrorMsgM = "Success";
        }

        public void MarkAsFailed(HttpStatusCode StatusCodeP, string zShortErrorMessageP, string zLongErrorMsgP)
        {
            nErrorCodeM = (int)StatusCodeP;
            zShortErrorMsgM = zShortErrorMessageP;
            zLongErrorMsgM = zLongErrorMsgP;
            return;
        }

        public void MarkAsFailed(HttpStatusCode StatusCodeP, string zShortErrorMessageP)
        {
            nErrorCodeM = (int)StatusCodeP;
            zShortErrorMsgM = zShortErrorMessageP;
            zLongErrorMsgM = zShortErrorMessageP;
            return;
        }

        public override string ToString()
        {
            string zRetValL = string.Format("ErrorCode: {0}\r\nShort Description: {1}\r\nLong Description: {2}", nErrorCodeM, zShortErrorMsgM, zLongErrorMsgM);
            return zRetValL;
        }


        public int ErrorCode
        {
            get
            {
                return this.nErrorCodeM;
            }
            set
            {
                this.nErrorCodeM = value;
            }
        }

        public string ShortErrorMessage
        {
            get
            {
                return this.zShortErrorMsgM;
            }
            set
            {
                this.zShortErrorMsgM = value;
            }
        }

        public string LongErrorMessage
        {
            get
            {
                return this.zLongErrorMsgM;
            }
            set
            {
                this.zLongErrorMsgM = value;
            }
        }
    }

}