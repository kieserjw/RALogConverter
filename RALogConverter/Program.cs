//#define CATCH_AT_RUNTIME

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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RALogConverter
{
    class Program
    {
        static void Main(string[] args)
        {                        
            getDataFromSite();
            Console.ReadKey();
        }
        
        private static void getDataFromSite()
        {
            String sd = "1/1/2000";            //start date
            String ed = "12/31/2015";           //end date            
            String txtUserID = "kieserjw";  //desired account's username (use %20 for spaces)

            String txtUsername = "kieserjw2";  //Any account's username
            String txtPassword = "";   //That account's pw
            
            String txtLastName = "";
            String txtCity = "";
            String lstState = "0";
            String lstCategory = "0";

            String uk = "";

            if (txtUserID.Equals(txtUsername))
            {
                Console.WriteLine("Cannot use same account for log in as target data.  Please create dummy log in account");
                return; //MAJOR BUG
            }

            //BEGIN LOGIN ---------------

            String postUrl = "http://www.running2win.com/verifylogin.asp";
            String postData = String.Format("txtUsername={0}&txtPassword={1}&btnLogin={2}", txtUsername, txtPassword, "Sign In");

            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            CookieContainer cookieJar = new CookieContainer();
            getRequest.CookieContainer = cookieJar;            
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
            String sourceCode = null;
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                sourceCode = sr.ReadToEnd();  //this sourcecode is never read.  We only need the cookies from the log in
            }

            //END LOGIN ---------------


            //BEGIN USER ID TO UK---------------

            postUrl = "http://www.running2win.com/community/UserSearch.asp";
            postData = String.Format("txtLastName={0}&txtUserName={1}&txtCity={2}&lstState={3}&lstCategory={4}&btnSearch={5}", txtLastName, txtUserID, txtCity, lstState, lstCategory, "Submit search criteria");

            getRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            getRequest.CookieContainer = cookieJar;            
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
            if (sourceCode.Contains("Please log in"))
            {
                Console.WriteLine("Error with log in"); 
                return;
            }
            if (sourceCode.Contains(">No<"))
            {
                Console.WriteLine("User log is private"); 
                return;
            }
            //System.IO.File.WriteAllText(@"C:/Users/Jeremy/Downloads/test.html", sourceCode);  //TESTING PURPOSES.  it's easier to see the html in browser (as compared to the local variable window)

            int startIndex = sourceCode.IndexOf("<a href=\"view-member-running-log.asp?uk=") + 40;  //look for the UK key associated with this particular UserID
            int endIndex = sourceCode.IndexOf("\"", startIndex);
        
            uk = sourceCode.Substring(startIndex, endIndex - startIndex);
            if (uk.Length != 70 && Regex.Matches(uk, @"[^A-Z0-9]").Count > 0)
            {
                Console.WriteLine("bad UK or UserID");  //UK key should be 70 chars of capital alphas or numerics
                return;
            }
            //END USER ID TO UK---------------

            DateTime start = DateTime.Parse(sd, System.Globalization.CultureInfo.InvariantCulture);
            DateTime end = DateTime.Parse(ed, System.Globalization.CultureInfo.InvariantCulture);
            if (end.CompareTo(start) < 0)  //if start and end dates are out of order, flip them
            {
                DateTime tmp = end;
                end = start;
                start = tmp;
            }
            ArrayList dateList = new ArrayList();
            dateList.Add(start);
            while (end.CompareTo(((DateTime)dateList[dateList.Count - 1]).AddYears(1)) > 0) //create a list of date intervals that are at most 1 year long
            {
                dateList.Add(((DateTime)dateList[dateList.Count - 1]).AddYears(1));
            }
            dateList.Add(end.AddDays(1));  //add one day for the end to compensate for the removal of a day during the loop

            Translator translator = new Translator();  //get one translator to add all others to


            //BEGIN DATA RETRIEVAL ---------------
            for (int i = 0; i < dateList.Count - 1; i++)  //r2w only supports retrieving 1 year's worth of data at a time, so we must loop through to get the entire range if necessary
            {
                sd = ((DateTime)dateList[i]).ToShortDateString();
                ed = ((DateTime)dateList[i + 1]).AddDays(-1).ToShortDateString(); //take off 1 day for the end of the range to avoid duplicates on end dates

                String getUrl = "http://www.running2win.com/community/view-member-running-log.asp";
                String getData = String.Format("vu={0}&sd={1}&ed={2}&uk={3}", "", sd, ed, uk);

                getRequest = (HttpWebRequest)WebRequest.Create(getUrl + "?" + getData);
                getRequest.CookieContainer = cookieJar;
                getRequest.Method = WebRequestMethods.Http.Get;

                getResponse = (HttpWebResponse)getRequest.GetResponse();
                sourceCode = null;
                using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
                {
                    sourceCode = sr.ReadToEnd();  //sourceCode is actual html for the given UserID and daterange
                }

                newStream.Close();
                Translator t = new Translator();
                

#if CATCH_AT_RUNTIME //added try/catch to catch errors and retry data requests at runtime.  comment out #def CATCH_AT_RUNTIME @ line 1 to resume catching errors in the C# environment
                try
                {
#endif
                    readFromSite(t, sourceCode);  //parse the sourceCode entries and add them to t
                    if (t.getEntryList().Count > 0)
                    {
                        translator.getEntryList().AddRange(t.getEntryList());   //if there were any entries, add them to the overall set                        
                    }
                    Console.WriteLine(sd + " - " + ed + " -- " + t.getEntryList().Count);  //display the interval and its number of entries
#if CATCH_AT_RUNTIME
                }
                catch (Exception e)
                {
                    if (t.getEntryList().Count > 0)
                    {
                        LogEntry le = ((LogEntry)t.getEntryList()[t.getEntryList().Count - 1]);          //if there was an error, display the last successful entry's date/comment
                        Console.WriteLine("Error after date: " + le.getDate().ToShortDateString());
                        Console.WriteLine(le.getNotes());                                                                       
                    }
                    Console.WriteLine(e.ToString());                                                    //display the actual error and skip this interval
                }
#endif

            }

            //END DATA RETRIEVAL ---------------   


            writeFileFromSite(translator, txtUserID, ((DateTime)dateList[0]).ToShortDateString().Replace("/", "-"), ed.Replace("/", "-"));
                  

        }

        private static void readFromSite(Translator t, String sourceCode)
        {

            if (!sourceCode.Contains("Please log in to your account"))              //error with the log in
            {
                HtmlDocument doc = new HtmlDocument();

                int index = sourceCode.IndexOf("<p>&nbsp;</p>")+13;
                sourceCode = sourceCode.Substring(index, sourceCode.LastIndexOf("<p>&nbsp;</p>") - index).Replace("\t", "");  //parse out the actual entries
                if (sourceCode.Length > 5)
                {
                    doc.LoadHtml(sourceCode);
                    t.translateFromSite(doc);
                }                
            }
        }

        private static void writeFileFromSite(Translator t, String username, String sd, String ed)
        {
            if (t.getEntryList().Count != 0)
            {
                String header = "Date,Time,Activity,Workout,Distance,Distance Unit,Duration," +
                        "Course,Equipment Brand,Equipment Model,Equipment Serial,Weight,Weight Unit," +
                        "Rest HR,Average HR,Max HR,Temperature,Temperature Unit,Quality,Effort,Notes";
                String folder = @"C:\Users\Jeremy\Documents\Projects\C#\RALogConverter\out\";
                String output = folder + username + "_" + sd + "_" + ed + DateTime.Now.ToString("_HHmmss") + ".csv";  //outputs to csv file with date range and timestamp in filename
                StringBuilder sb = new StringBuilder();
                foreach (LogEntry e in t.getEntryList())  //append all entries to sb
                {
                    sb.Append(e.toString());
                    sb.Append("\n");
                }

                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(output))  //write out the entire file
                {
                    sw.WriteLine(header);
                    sw.WriteLine(sb.ToString());
                    Console.WriteLine(output);
                }
            }

        }

        private static void readFromFiles(Translator t)  //NOT USED FOR WEBSITE CRAWLING
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

        private static void writeFile(Translator t, String folder)//NOT USED FOR WEBSITE CRAWLING
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

    }
}
