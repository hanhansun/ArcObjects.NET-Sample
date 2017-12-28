using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TestRESTWebRequest
{
    class Program
    {
        static JavaScriptSerializer json = null;

        static void Main(string[] args)
        {
            //Replace the following 4 parameters based on your Server environment
            string serviceurl = "https://csc-hsun7d.esri.com:6443/arcgis/admin/services/SampleWorldCities.MapServer";
            string tokenurl = "http://csc-hsun7d.esri.com:6080/arcgis/admin/generateToken";
            string username = "siteadmin";  //server admin username
            string password = "siteadmin";  //server admin password


            json = new System.Web.Script.Serialization.JavaScriptSerializer();

            string token = GetToken(tokenurl, username, password);
            string serviceStat = GetServiceStatus(serviceurl,token);
            if (serviceStat == "STARTED")
                Console.WriteLine("Service is started.\nStop Operation Status: " + StartOrStopService(serviceurl, "stop", token));
            else if (serviceStat == "STOPPED")
                Console.WriteLine("Service is stopped.\nStart Operation Status: " + StartOrStopService(serviceurl, "start", token));
            Console.ReadLine();
        }

        private static string GetServiceStatus(string serviceurl, string token)
        {
            var data = Encoding.ASCII.GetBytes("token=" + token + "&f=json");
            var request = (HttpWebRequest)WebRequest.Create(serviceurl + "/status");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var rawResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();
            object servstat = "";
            if (rawResponse.Contains("error"))
            {
                MessageBox.Show("failed: " + rawResponse);
                throw new System.InvalidOperationException("fix the token generation process");
            }
            else
            {
                Dictionary<string, object> jsonResponse = json.Deserialize<Dictionary<string, object>>(rawResponse);
                if (jsonResponse.ContainsKey("realTimeState"))
                    jsonResponse.TryGetValue("realTimeState", out servstat);
            }
            response.Close();
            return servstat.ToString();
        }
        
        private static string StartOrStopService(string serviceurl, string action, string token)
        {
            var data = Encoding.ASCII.GetBytes("token=" + token + "&f=json");
            var request = (HttpWebRequest)WebRequest.Create(serviceurl + "/" + action);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var rawResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();
            object servstat = "";
            if (rawResponse.Contains("error"))
            {
                MessageBox.Show("failed: " + rawResponse);
                throw new System.InvalidOperationException("fix the token generation process");
            }
            else
            {
                Dictionary<string, object> jsonResponse = json.Deserialize<Dictionary<string, object>>(rawResponse);
                if (jsonResponse.ContainsKey("status"))
                    jsonResponse.TryGetValue("status", out servstat);
            }
            response.Close();
            return servstat.ToString();
        }

        private static string GetToken(string url, string username, string password)
        {
            object tokenstr = "";
            var postData = string.Format("username={0}&password={1}&client=requestip&expiration=60&f=json", username, password);
            var data = Encoding.ASCII.GetBytes(postData);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var rawResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (rawResponse.Contains("error"))
            {
                MessageBox.Show("failed: " + rawResponse);
                throw new System.InvalidOperationException("fix the token generation process");
            }
            else
            {
                Dictionary<string, object> jsonResponse = json.Deserialize<Dictionary<string, object>>(rawResponse);
                if (jsonResponse.ContainsKey("token"))
                    jsonResponse.TryGetValue("token",out tokenstr);
            }
            response.Close();
            return tokenstr.ToString();
        }

        private static string ToggleService_Stop_Start(string url, string Action)
        {

            return "";
        }
    }
}
