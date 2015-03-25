using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
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
            //readFromFiles(t);
            RunAsync().Wait();
            Console.ReadKey();
        }

        static async Task RunAsync()
        {

            string postUrl = "http://www.running2win.com/pleaselogin.asp";
            string postData = String.Format("txtUsername={0}&txtPassword={1}&btnLogin={2}", "kieserjw2", "testTEST", "Sign In");
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

            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            getRequest.ContentLength = byteArray.Length;
            Stream newStream = getRequest.GetRequestStream(); //open connection
            newStream.Write(byteArray, 0, byteArray.Length); // Send the data.            

            HttpWebResponse getResponse = (HttpWebResponse)getRequest.GetResponse();
            string sourceCode = null;
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                sourceCode = sr.ReadToEnd();
            }
          
            string uk, sd, ed = null;
            uk = "SEUYMJESBE45HFUHYAYBGCUBKSBPHIWHNHFAKCQYSOFO17338TSVJXPVKWSXTBXENJKYEO";
            sd = "2/1/2015";
            ed = "2/28/2015";


            String data = null;
            string getUrl = "http://www.running2win.com/community/view-member-running-log.asp";
            string getData = String.Format("vu={0}&sd={1}&ed={2}&uk={3}", "", sd, ed, uk);

            getRequest = (HttpWebRequest)WebRequest.Create(getUrl);
            
            getRequest.CookieContainer = cookieJar;
            
            getRequest.Method = WebRequestMethods.Http.Get;

            getResponse = (HttpWebResponse)getRequest.GetResponse();
            sourceCode = null;
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                sourceCode = sr.ReadToEnd();
            }

            if (sourceCode.Contains("Please log in to your account"))
            {
                Console.WriteLine("Failed");
            }


            /*          Request Headers:
                        Accept:text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,* / *;q=0.8
                        Accept-Encoding:gzip, deflate, sdch
                        Accept-Language:en-US,en;q=0.8,de;q=0.6
                        Connection:keep-alive
                        Cookie:ASPSESSIONIDQACRRQSR=EMLAHILDNDPFPEBPGLKIJNAM; __utma=159512126.287629504.1426050659.1426050659.1426050659.1; __utmb=159512126; __utmc=159512126; __utmz=159512126.1426050659.1.1.utmccn=(direct)|utmcsr=(direct)|utmcmd=(none)
                        Host:www.running2win.com
                        User-Agent:Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.115 Safari/537.36
            
                        Query String Parameters:
                        vu:
                        sd:2/1/2015
                        ed:2/28/2015
                        uk:JWAMGSSJNH38LGHNVYGDSWQJGBJBSJYLLAVCQ23959MMGBZQJFELRFOFHCHHLWSKFHBCTU
            */
            Translator t = new Translator();
            
        }
     
        private static void readFromFiles(Translator t)
        {

            String targetFolder = null;
            bool test = true;
            if (test)
            {
                targetFolder = @"C:\Users\Jeremy\Downloads\";
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
            String output = folder + Path.DirectorySeparatorChar + folder.Substring(folder.LastIndexOf(Path.DirectorySeparatorChar) + 1) + "_c#_out.csv";

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
