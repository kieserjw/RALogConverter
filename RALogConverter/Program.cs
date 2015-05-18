using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RALogConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Translator t = new Translator();
            //readFromFiles(t,"");
            RunAsync().Wait();
            Console.ReadKey();
        }

        static async Task RunAsync()
        {
            string sd = "1/1/2000";            //start date
            string ed = "12/1/2015";           //end date
            String txtUsername = "kieserjw2";  //Any account's username
            String txtPassword = "";           //That account's pw
            String txtUserID = "dgreeno";      //desired account's username
            
            String txtLastName = "";
            String txtCity = "";
            String lstState = "0";
            String lstCategory = "0";

            String uk = "";

            //BEGIN LOGIN ---------------

            String postUrl = "http://www.running2win.com/verifylogin.asp";
            String postData = String.Format("txtUsername={0}&txtPassword={1}&btnLogin={2}", txtUsername, txtPassword, "Sign In");


            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            CookieContainer cookieJar = new CookieContainer();
            getRequest.CookieContainer = cookieJar;
            //getRequest.CookieContainer.Add(cookies); //recover cookies First request
            getRequest.Method = WebRequestMethods.Http.Post;
            getRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            getRequest.AllowWriteStreamBuffering = true;
            getRequest.ProtocolVersion = HttpVersion.Version11;
            getRequest.AllowAutoRedirect = true;
            getRequest.ContentType = "application/x-www-form-urlencoded";
            getRequest.KeepAlive = true;

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            getRequest.ContentLength = byteArray.Length;
            Stream newStream = getRequest.GetRequestStream(); //open connection
            newStream.Write(byteArray, 0, byteArray.Length); // Send the data.            

            HttpWebResponse getResponse = (HttpWebResponse)getRequest.GetResponse();
            string sourceCode = null;
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                sourceCode = sr.ReadToEnd();
            }

            //END LOGIN ---------------


            //BEGIN USER ID ---------------

            postUrl = "http://www.running2win.com/community/UserSearch.asp";
            postData = String.Format("txtLastName={0}&txtUserName={1}&txtCity={2}&lstState={3}&lstCategory={4}&btnSearch={5}", txtLastName, txtUserID, txtCity, lstState, lstCategory, "Submit search criteria");

            getRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            getRequest.CookieContainer = cookieJar;
            //getRequest.CookieContainer.Add(cookies); //recover cookies First request
            getRequest.Method = WebRequestMethods.Http.Post;
            getRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            getRequest.AllowWriteStreamBuffering = true;
            getRequest.ProtocolVersion = HttpVersion.Version11;
            getRequest.AllowAutoRedirect = true;
            getRequest.ContentType = "application/x-www-form-urlencoded";
            getRequest.KeepAlive = true;

            byteArray = Encoding.UTF8.GetBytes(postData);
            getRequest.ContentLength = byteArray.Length;
            newStream = getRequest.GetRequestStream(); //open connection
            newStream.Write(byteArray, 0, byteArray.Length); // Send the data.            

            getResponse = (HttpWebResponse)getRequest.GetResponse();
            sourceCode = null;
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                sourceCode = sr.ReadToEnd();
            }
            


            int startIndex = sourceCode.IndexOf("<a href=\"view-member-running-log.asp?uk=") + 40;
            int endIndex = sourceCode.IndexOf("\"", startIndex);

            uk = sourceCode.Substring(startIndex, endIndex - startIndex);
            //uk = "FGCIBRRPPFPXGMTGY42OQKSACSIGGOBFEZBXVBPQF29564UGWTKAQEOMHEJCTJKFWHGCTI";

            //END USER ID ---------------

            DateTime start = DateTime.Parse(sd, System.Globalization.CultureInfo.InvariantCulture);
            DateTime end = DateTime.Parse(ed, System.Globalization.CultureInfo.InvariantCulture);
            if (ed.CompareTo(sd) < 0)
            {
                DateTime tmp = end;
                end = start;
                start = tmp;
            }
            ArrayList dateList = new ArrayList();
            dateList.Add(start);
            while (end.CompareTo(((DateTime)dateList[dateList.Count - 1]).AddYears(1)) > 0)
            {
                dateList.Add(((DateTime)dateList[dateList.Count - 1]).AddYears(1));
            }
            dateList.Add(end.AddDays(1));

            Translator t = new Translator();


            //BEGIN DATA RETRIEVAL ---------------
            for (int i = 0; i < dateList.Count - 1; i++)
            {
                sd = ((DateTime)dateList[i]).ToShortDateString();
                ed = ((DateTime)dateList[i + 1]).AddDays(-1).ToShortDateString();

                string getUrl = "http://www.running2win.com/community/view-member-running-log.asp";
                string getData = String.Format("vu={0}&sd={1}&ed={2}&uk={3}", "", sd, ed, uk);

                getRequest = (HttpWebRequest)WebRequest.Create(getUrl + "?" + getData);

                getRequest.CookieContainer = cookieJar;

                getRequest.Method = WebRequestMethods.Http.Get;

                getResponse = (HttpWebResponse)getRequest.GetResponse();
                sourceCode = null;
                using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
                {
                    sourceCode = sr.ReadToEnd();
                }

                newStream.Close();
                readFromSite(t, sourceCode);

            }
            writeFileFromSite(t, txtUserID, ((DateTime)dateList[0]).ToShortDateString().Replace("/", "-"), ed.Replace("/", "-"));
            //END DATA RETRIEVAL ---------------         

        }

        private static void readFromSite(Translator t, string sourceCode)
        {

            if (!sourceCode.Contains("Please log in to your account"))
            {
                HtmlDocument doc = new HtmlDocument();

                int index = sourceCode.IndexOf("<p>&nbsp;</p>")+13;
                sourceCode = sourceCode.Substring(index, sourceCode.LastIndexOf("<p>&nbsp;</p>") - index).Replace("\t", "");
                if (sourceCode.Length > 5)//&& !sourceCode.Contains("Create a workout for"))
                {
                    doc.LoadHtml(sourceCode);
                    t.translateFromSite(doc);
                }
                else
                {
                    Console.WriteLine("No entries");
                }
            }
        }

        private static void readFromFiles(Translator t)
        {

            String targetFolder = null;
            bool test = true;
            if (test)
            {
                targetFolder = @"C:\Users\Jeremy\Downloads\IndyRunner";
            }
            else
            {
                Console.WriteLine("Please select any r2w file with \"_Export_\" in its name\nAll files in selected directory will be parsed and outputted to out.csv");
                targetFolder = Console.ReadLine();
            }


            ArrayList fileList = new ArrayList();

            if (targetFolder != null)
            {
                String[] allFiles = Directory.GetFiles(targetFolder);
                foreach (String file in allFiles)
                {
                    if (file.Contains("_Export_"))
                    {
                        fileList.Add(file);
                    }
                }
            }
            if (fileList.Count > 0)
            {
                foreach (String file in fileList)
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(file);
                    t.translate(doc);
                }
                writeFile(t, targetFolder);
            }
            else
            {
                Console.WriteLine("Selected file:\n    " + targetFolder + "\ndoes not contain files with r2w's naming tag \"_Export_\" in it.\n\nPlease try again");
            }

        }

        private static void writeFile(Translator t, String folder)
        {
            String header = "Date,Time,Activity,Workout,Distance,Distance Unit,Duration," +
                    "Course,Equipment Brand,Equipment Model,Equipment Serial,Weight,Weight Unit," +
                    "Rest HR,Average HR,Max HR,Temperature,Temperature Unit,Quality,Effort,Notes\n";
            String output = "";
            if (folder.Equals(""))
            {
                output = @"C:\Users\Jeremy\Downloads\c#_out-" + DateTime.Now.ToString("HH-mm-ss") + ".csv";
            }
            else
            {
                output = folder + Path.DirectorySeparatorChar + folder.Substring(folder.LastIndexOf(Path.DirectorySeparatorChar) + 1) + "_c#_out.csv";
            }
            StringBuilder sb = new StringBuilder();
            foreach (LogEntry e in t.getEntryList())
            {
                sb.Append(e.toString());
                sb.Append("\n");
            }

            // Create a file to write to. 
            using (StreamWriter sw = File.CreateText(output))
            {
                sw.WriteLine(header);
                sw.WriteLine(sb.ToString());
                Console.WriteLine(output);
            }

        }


        private static void writeFileFromSite(Translator t, String username, String sd, String ed)
        {
            if (t.getEntryList().Count != 0)
            {
                String header = "Date,Time,Activity,Workout,Distance,Distance Unit,Duration," +
                        "Course,Equipment Brand,Equipment Model,Equipment Serial,Weight,Weight Unit," +
                        "Rest HR,Average HR,Max HR,Temperature,Temperature Unit,Quality,Effort,Notes";
                String output = @"C:\Users\Jeremy\Downloads\" + username + "_" + sd + "_" + ed + DateTime.Now.ToString("_HHmmss") + ".csv";
                StringBuilder sb = new StringBuilder();
                foreach (LogEntry e in t.getEntryList())
                {
                    sb.Append(e.toString());
                    sb.Append("\n");
                }

                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(output))
                {
                    sw.WriteLine(header);
                    sw.WriteLine(sb.ToString());
                    Console.WriteLine(output);
                }
            }

        }
    }
}
