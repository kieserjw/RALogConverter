using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RALogConverter
{
    class Translator
    {
        private ArrayList entryList;
        private static String[] brands = { "brooks", "asics", "nike", "hoka", "adidas", "saucony", "newton", "puma", "reebok" };

        public Translator()
        {
            this.entryList = new ArrayList();
        }

        public ArrayList getEntryList()
        {
            return this.entryList;
        }

        public void translateFromSite(HtmlDocument doc)
        {
            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table[contains(@class,'encapsule')]");

            for (int i = 0; i < tables.Count; i++)
            {

                HtmlNode entryNode = tables[i].ChildNodes[1].ChildNodes[1];
                HtmlNode genInfo = entryNode.ChildNodes[1].ChildNodes[1];
                HtmlNode otherInfo = entryNode.ChildNodes[3];
                LogEntry le = new LogEntry();

                setDateActivityInfoFromSite(le, genInfo.InnerText.Split('\n'));
                if (le.getDate().Date.Equals(DateTime.Parse("5/23/2011", System.Globalization.CultureInfo.InvariantCulture)))
                {
                    int ps = 0;//testing purposes
                }
                setGenInfoFromSite(le, otherInfo.InnerText.Replace("\r", "").Split('\n'));
                if (entryNode.ChildNodes[5].InnerText.StartsWith("\nRace Info"))
                {
                    HtmlNode raceInfo = entryNode.ChildNodes[5];
                    setRaceInfoFromSite(le, raceInfo.InnerText.Replace("\r", "").Split('\n'));
                }

                else if (entryNode.ChildNodes[5].InnerText.StartsWith("\nInterval Info"))
                {
                    HtmlNode intervalInfo = entryNode.ChildNodes[5];
                    setIntervalInfoFromSite(le, intervalInfo.InnerText.Replace("\r", "").Split('\n'));

                }
                else if (entryNode.ChildNodes[5].InnerText.Contains("Cross Training Comments"))
                {
                    HtmlNode xTrainInfo = entryNode.ChildNodes[5];
                    xTrainInfo = xTrainInfo.LastChild.PreviousSibling;
                    setXTrainFromSite(le, xTrainInfo.InnerText.Replace("\r", "").Split('\n'));

                }

                entryList.Add(le);
                Console.WriteLine(le.getDate().ToString(System.Globalization.CultureInfo.InvariantCulture));

            }
        }

        private void setGenInfoFromSite(LogEntry le, string[] genInfo)
        {
            int i = 0;
            while (!genInfo[i].StartsWith("Comments")) i++;
            String notes = genInfo[++i];
            while (!genInfo[i].StartsWith("Total distance")) i++;
            String distanceStr = genInfo[i = i + 2];
            while (!genInfo[i].StartsWith("Course")) i++;
            String course = genInfo[++i];
            while (!genInfo[i].StartsWith("Difficulty")) i++;
            String effort = genInfo[++i];
            while (!genInfo[i].StartsWith("Morning HR")) i++;
            String morHR = genInfo[i = i + 2];
            while (!genInfo[i].StartsWith("Average HR")) i++;
            String avgHR = genInfo[i = i + 2];
            while (!genInfo[i].StartsWith("MAX HR")) i++;
            String maxHR = genInfo[i = i + 2];
            while (!genInfo[i].StartsWith("Weight")) i++;
            String wt = genInfo[++i];
            while (!genInfo[i].StartsWith("Shoe")) i++;
            String shoe = genInfo[++i];

            le.setNotes(notes);

            String[] distanceInfo = distanceStr.Split(' ');
            double distance = 0;
            String distanceUnit = "miles";
            if (distanceInfo.Length > 1 && !distanceInfo[0].StartsWith("--") && !distanceInfo[0].Equals(""))
            {
                distance = Double.Parse(distanceInfo[0]);
                distanceUnit = distanceInfo[1].ToLower();
                if (!distanceUnit.EndsWith("s")) distanceUnit += "s";
                if (distanceInfo.Length > 2)
                {
                    double duration = parseDuration(distanceInfo[3]);
                    le.setDuration(duration);
                }
            }
            le.setDistance(distance);
            le.setDistanceUnit(distanceUnit);

            int rating = Int32.Parse(effort.Substring(effort.IndexOf("Rating: ") + 8, effort.IndexOf("]") - effort.IndexOf("Rating: ") - 8));
            effort = effort.Substring(0, effort.IndexOf("&"));
            if (rating != 1)
            {
                le.setEffort(rating);
            }
            else if (effort.Equals("Somewhat Difficult"))
            {
                le.setEffort(4);
            }
            else if (effort.Equals("Difficult"))
            {
                le.setEffort(6);
            }
            else if (effort.Equals("Hard"))
            {
                le.setEffort(8);
            }

            if (shoe.Length > 0)
            {
                String equipmentBrand = null, equipmentModel = null;
                foreach (String b in brands)
                {
                    if (shoe.ToLower().Contains(b))
                    {
                        equipmentBrand = shoe.Substring(0, b.Length);
                        equipmentModel = shoe.Substring(b.Length).Trim();
                        le.setEquipmentBrand(equipmentBrand);
                        le.setEquipmentModel(equipmentModel);
                        break;
                    }
                }
                if (equipmentBrand == null)
                {
                    le.setEquipmentModel(shoe);
                }
            }


            if (morHR.Length > 0 && !morHR.Equals("--"))
                le.setRestHR(Int32.Parse((morHR.Split(' '))[0]));
            if (avgHR.Length > 0 && !avgHR.Equals("--"))
                le.setAverageHR(Int32.Parse((avgHR.Split(' '))[0]));
            if (maxHR.Length > 0 && !maxHR.Equals("--"))
                le.setMaxHR(Int32.Parse((maxHR.Split(' '))[0]));

            wt = wt.Substring(0, wt.IndexOf("&"));
            double weight = Double.Parse(wt);
            le.setWeight(weight);

        }

        private void setDateActivityInfoFromSite(LogEntry le, string[] info)
        {
            String dateStr = (info[2].Split(' '))[1];
            String time = info[5];
            String activity = info[8];
            dateActivityHelper(le, dateStr, time, activity);
        }

        private void setRaceInfoFromSite(LogEntry le, String[] raceInfo)
        {
            LogEntry le2 = new LogEntry();
            le2.setDate(le.getDate());
            le2.setActivity(le.getActivity());
            le2.setWorkout(le.getWorkout());
            le.setWorkout("Normal");

            le2.setNotes("***RACE***");
            int i = 0;
            foreach (String s in raceInfo)
            {
                if (!s.Equals(""))
                {
                    if ((s.Contains("Race Name") || s.Contains("Terrain")
                       || s.Contains("Warm Up") || s.Contains("Cool Down")
                       || s.Contains("Overall Place") || s.Contains("Age Place")
                       || s.Contains("Race Splits")) && raceInfo[i + 1].Length > 0)
                    {
                        le2.setNotes(le2.getNotes() + "\n" + s + ": " + raceInfo[i + 1]);
                    }
                    else if (s.Contains("Distance") && raceInfo[i + 1].Length > 0)
                    {
                        String[] dist = raceInfo[i + 1].Split(new[] { "&nbsp;" }, StringSplitOptions.None);
                        double distance = Double.Parse(dist[0]);
                        le2.setDistance(distance);
                        String distanceUnit = dist[1].ToLower();
                        if (!distanceUnit.EndsWith("s")) distanceUnit += "s";
                        le2.setDistanceUnit(distanceUnit);


                        if (le.getDistanceUnit().Equals("miles") && distanceUnit.Equals("kilometers"))
                        {//convert to WU/CD units (only support for km, mi, and races in meters )
                            distance /= 1.609;
                        }
                        else if (le.getDistanceUnit().Equals("kilometers") && distanceUnit.Equals("miles"))
                        {
                            distance *= 1.609;

                        }
                        else if (distanceUnit.Equals("meters") && le.getDistanceUnit().Equals("miles"))
                        {
                            distance /= 1609.0;
                        }
                        else if (distanceUnit.Equals("meters") && le.getDistanceUnit().Equals("kilometers"))
                        {
                            distance /= 1000.0;
                        }
                        if (le.getDistance() >= distance)
                        {
                            le.setDistance(le.getDistance() - distance); //compensate for distance run in race
                        }


                    }
                    else if (s.Contains("Time") && raceInfo[i + 2].Length > 0)
                    {
                        double duration = parseDuration(raceInfo[i + 2]);
                        le2.setDuration(duration);
                        if (le.getDuration() >= duration)
                        {
                            le.setDuration(le.getDuration() - duration); //compensate for time run in race
                        }
                    }
                    else if (s.Contains("Comments") && raceInfo[i + 1].Length > 0)
                    {
                        le2.setNotes(le2.getNotes() + "\n\n" + raceInfo[i + 1]);
                    }

                }
                i++;

            }
            this.entryList.Add(le2);
        }

        private void setIntervalInfoFromSite(LogEntry le, String[] intervalInfo)
        {
            //interval -- tried to print it out with spacing, but RA doesn't support it.  It just trims it
            //Could try to make this look nicer, but without a monospacing font, it isn't worth it

            StringBuilder sb = new StringBuilder();
            sb.Append("***INTERVAL***\nWarm-up: ");
            int i = 0;
            while (!intervalInfo[i].StartsWith(" Warm-up: ")) i++;
            i++;
            sb.Append(intervalInfo[i]);
            while (!intervalInfo[i].StartsWith("Set rest ")) i++;
            i = i + 3;
            sb.Append("\nSet...Reps...Distance/Time...Goal...Actual...Rep rest...Set rest\n");
            while (i < intervalInfo.Length - 4)
            {

                sb.Append(intervalInfo[i] + "........." + // set
                        intervalInfo[i + 1] + "....." + // reps
                          intervalInfo[i + 2].Replace("&nbsp;", "") + " " + intervalInfo[i + 3] + "...." + // dist/time
                        (intervalInfo[i + 4].Equals("") ? "____" : intervalInfo[i + 4]) + "...." + // goals
                        (intervalInfo[i + 5].Equals("") ? "____" : intervalInfo[i + 5]) + "...." + // Actual
                        (intervalInfo[i + 6].Equals("") ? "____" : intervalInfo[i + 6]) + "...." + // rep rest
                        (intervalInfo[i + 7].Equals("") ? "____" : intervalInfo[i + 7]) + "\n"); // set rest
                i = i + 10;
            }
            i++;
            sb.Append("Cool-down: " + intervalInfo[i]);

            sb.Append("\n\n" + le.getNotes());
            le.setNotes(sb.ToString());


        }

        private void setXTrainFromSite(LogEntry le, String[] xTrainInfo)
        {
            String xtComments = xTrainInfo[2].Trim();
            if (xtComments.Length > 0)
            {
                le.setNotes(le.getNotes() + "\n\nCross Training Comments:\n" + xtComments);
            }
        }

        public void translate(HtmlDocument doc)
        {
            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
            for (int i = 0; i < tables.Count; i++)
            {
                HtmlNode dateInfo = tables[i];
                if (dateInfo.Id.Equals("Table1"))
                {
                    HtmlNode genInfo = dateInfo.NextSibling.NextSibling;
                    HtmlNode raceIntervalInfo = null;
                    HtmlNode crossTrainingComments = null;
                    HtmlNode otherInfo = genInfo.NextSibling;
                    LogEntry le = new LogEntry();
                    if (otherInfo.InnerHtml.Contains(">Cross Training Info:"))
                    {
                        crossTrainingComments = otherInfo;
                        otherInfo = otherInfo.NextSibling;
                    }
                    if (otherInfo.InnerHtml.Contains(">Interval Information:") || otherInfo.InnerHtml.Contains(">Race Information:"))
                    {
                        raceIntervalInfo = otherInfo;
                        otherInfo = otherInfo.NextSibling;
                    }


                    setDateActivityInfo(le, dateInfo);
                    setGenInfo(le, genInfo);
                    if (raceIntervalInfo != null) setRaceIntervalInfo(le, raceIntervalInfo);
                    if (crossTrainingComments != null && !crossTrainingComments.InnerHtml.Contains("display:none")) setXTrainComments(le, crossTrainingComments);
                    setOtherInfo(le, otherInfo);  //dummy method -- no RA data to parse here

                    this.entryList.Add(le);
                    Console.WriteLine(le.getDate().ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        private void setDateActivityInfo(LogEntry le, HtmlNode dateInfo)
        {
            //"MM/dd/yyyy HH:mm:ss"
            String dateStr = dateInfo.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerHtml;
            dateStr = dateStr.Substring(dateStr.IndexOf(",") + 2);
            String time = dateInfo.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerHtml;
            String activity = dateInfo.ChildNodes[0].ChildNodes[2].ChildNodes[0].InnerHtml;
            dateActivityHelper(le, dateStr, time, activity);
        }

        private void dateActivityHelper(LogEntry le, String dateStr, String time, String activity)
        {
            if (time.Equals("Morning"))
            {
                time = " 10:00:00";
            }
            else if (time.Equals("Afternoon"))
            {
                time = " 14:00:00";
            }
            else if (time.Equals("Evening"))
            {
                time = " 18:00:00";
            }
            else if (time.Equals("Night"))
            {
                time = " 20:00:00";
            }
            else
            {
                time = " 12:00:00";
            }
            DateTime date;

            date = DateTime.Parse(dateStr + time,
                          System.Globalization.CultureInfo.InvariantCulture);
            le.setDate(date);            
            parseAndSetActivity(le, activity);
        }

        private void parseAndSetActivity(LogEntry le, String activity)
        {
            String workout = "Normal";
            if (activity.ToLower().Contains("bike") || activity.ToLower().Contains("bicyle"))
            {
                activity = "Bike";
            }
            else if (activity.ToLower().Contains("swim"))
            {
                activity = "Swim";
            }
            else if (activity.ToLower().Contains("ski"))
            {
                activity = "Ski";
            }
            else if (activity.ToLower().Contains("off"))
            {
                activity = "Rest";
            }
            else if (activity.ToLower().Contains("cross") || activity.ToLower().Contains("xtrain") ||
                   activity.ToLower().Contains("x train") || activity.ToLower().Contains("x-train") ||
                   activity.ToLower().Contains("other"))
            {
                activity = "Cross Training";
            }
            else if (activity.ToLower().Contains("elliptical"))
            {
                activity = "Elliptical";
            }
            else
            {
                workout = activity.Replace("run", "").Replace("Run", "").Replace("Training", "").Replace("training", "").Replace("workout", "").Replace("Workout", "").Trim();
                activity = "Run";

            }
            le.setWorkout(workout);
            le.setActivity(activity);


        }

        private void setGenInfo(LogEntry le, HtmlNode genInfo)
        {
            HtmlNodeCollection eList = genInfo.ChildNodes;
            foreach (HtmlNode e in eList)
            {
                //Console.WriteLine(e.InnerHtml);
                if (e.InnerHtml.Contains(">Comments<"))
                {
                    String notes = e.LastChild.InnerHtml;
                    le.setNotes(notes);
                }
                else if (e.InnerHtml.Contains(">Total distance<"))
                {
                    String[] distanceInfo = e.LastChild.InnerText.Split(' ');
                    double distance = 0;
                    String distanceUnit = "miles";
                    if (distanceInfo.Length > 1 && !distanceInfo[0].StartsWith("--") && !distanceInfo[0].Equals(""))
                    {
                        distance = Double.Parse(distanceInfo[0]);
                        distanceUnit = distanceInfo[1].ToLower();
                        if (distanceInfo.Length > 2)
                        {
                            double duration = parseDuration(distanceInfo[3]);
                            le.setDuration(duration);
                        }
                    }
                    le.setDistance(distance);
                    le.setDistanceUnit(distanceUnit);
                }
                else if (e.InnerHtml.Contains(">Difficulty<"))
                {
                    String effort = e.LastChild.InnerText;
                    int rating = Int32.Parse(effort.Substring(effort.IndexOf("Rating: ") + 8, effort.IndexOf("]") - effort.IndexOf("Rating: ") - 8));
                    effort = effort.Substring(0, effort.IndexOf("&"));
                    if (rating != 1)
                    {
                        le.setEffort(rating);
                    }
                    else if (effort.Equals("Somewhat Difficult"))
                    {
                        le.setEffort(4);
                    }
                    else if (effort.Equals("Difficult"))
                    {
                        le.setEffort(6);
                    }
                    else if (effort.Equals("Hard"))
                    {
                        le.setEffort(8);
                    }
                }
                else if (e.InnerHtml.Contains(">Shoe<"))
                {
                    String shoe = e.LastChild.InnerText;
                    if (shoe.Length > 0)
                    {
                        String equipmentBrand = null, equipmentModel = null;
                        foreach (String b in brands)
                        {
                            if (shoe.ToLower().Contains(b))
                            {
                                equipmentBrand = shoe.Substring(0, b.Length);
                                equipmentModel = shoe.Substring(b.Length).Trim();
                                le.setEquipmentBrand(equipmentBrand);
                                le.setEquipmentModel(equipmentModel);
                                break;
                            }
                        }
                        if (equipmentBrand == null)
                        {
                            le.setEquipmentModel(shoe);
                        }
                    }

                }
                else if (e.InnerHtml.Contains(">Morning HR"))
                {
                    String[] ahr = e.LastChild.InnerText.Split(' ');
                    int restHR = Int32.Parse(ahr[0]);
                    le.setRestHR(restHR);
                }
                else if (e.InnerHtml.Contains(">Average HR"))
                {
                    String[] ahr = e.LastChild.InnerText.Split(' ');
                    int averageHR = Int32.Parse(ahr[0]);
                    le.setAverageHR(averageHR);
                }
                else if (e.InnerHtml.Contains(">MAX HR"))
                {
                    String[] ahr = e.LastChild.InnerText.Split(' ');
                    int maxHR = Int32.Parse(ahr[0]);
                    le.setMaxHR(maxHR);
                }
                else if (e.InnerHtml.Contains(">Temperature<"))
                {
                    String temp = e.LastChild.InnerHtml;
                    int temperature = Int32.Parse(temp.Substring(0, temp.Length - 1));
                    le.setTemperature(temperature);
                }
                else if (e.InnerHtml.Contains("lbs/kilo"))
                {
                    String wt = e.InnerHtml;
                    wt = wt.Substring(0, wt.IndexOf("&"));
                    double weight = Double.Parse(wt);
                    le.setWeight(weight);
                }
            }

        }

        private double parseDuration(String dur)
        {
            double duration = 0;
            if (dur.Length > 0)
            {
                String[] durationInfo = (dur.Split(' '))[0].Split(':');//assumes time is first part of text field. removes any other text options
                for (int i = 0; i < durationInfo.Length; i++)
                {
                    duration += Double.Parse(durationInfo[durationInfo.Length - i - 1]) * Math.Pow(60, i);
                }
            }
            return duration;
        }

        private void setRaceIntervalInfo(LogEntry le, HtmlNode raceIntervalInfo)
        {
            if (raceIntervalInfo.InnerHtml.Contains("Interval Information:"))
            {//interval -- tried to print it out with spacing, but RA doesn't support it.  It just trims it
                //Could try to make this look nicer, but without a monospacing font, it isn't worth it
                HtmlNodeCollection eList = raceIntervalInfo.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes;
                eList.Remove(0);
                ArrayList ill = new ArrayList();
                int[] lengths = new int[IntervalEntry.numFields];
                for (int i = 0; i < IntervalEntry.numFields; i++)
                {
                    lengths[i] = IntervalEntry.fieldHeaders[i + 2].Length + 2;
                }
                foreach (HtmlNode e in eList)
                {
                    int set = Int32.Parse(e.ChildNodes[0].InnerHtml);
                    int rep = Int32.Parse(e.ChildNodes[1].InnerHtml);
                    String[] fields = new String[IntervalEntry.numFields];
                    for (int i = 0; i < fields.Length; i++)
                    {
                        fields[i] = e.ChildNodes[2 + i].InnerHtml.Replace("&nbsp;", " ");
                        if (lengths[i] < fields[i].Length) lengths[i] = fields[i].Length + 2;
                    }
                    ill.Add(new IntervalEntry(set, rep, fields));

                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < IntervalEntry.numFields; i++)
                {
                    for (int j = 0; j < ill.Count; j++)
                    {
                        String f = ((IntervalEntry)ill[j]).getField(i);
                        if (f.Length < lengths[i])
                        {
                            sb.Clear();//reset sb
                            for (int z = 0; z < (lengths[i] - f.Length); z++) sb.Append(IntervalEntry.spaceChar);
                            ((IntervalEntry)ill[j]).setFields(f + sb.ToString(), i);
                        }

                    }
                }
                sb.Clear();
                sb.Append("***INTERVAL***\n" + IntervalEntry.headerString(lengths));
                for (int i = 0; i < ill.Count; i++)
                {
                    sb.Append("\n" + ((IntervalEntry)ill[i]).ToString());
                }
                sb.Append("\n" + le.getNotes());
                le.setNotes(sb.ToString());
            }
            else
            { //race
                LogEntry le2 = new LogEntry();
                le2.setDate(le.getDate());
                le2.setActivity(le.getActivity());
                le2.setWorkout(le.getWorkout());
                le.setWorkout("Normal");

                HtmlNodeCollection eList = raceIntervalInfo.ChildNodes;
                le2.setNotes("***RACE***");
                foreach (HtmlNode e in eList)
                {
                    if (e.InnerHtml.Contains(">Race Name<") || e.InnerHtml.Contains(">Terrain<")
                            || e.InnerHtml.Contains(">Warm Up<") || e.InnerHtml.Contains(">Cool Down<")
                            || e.InnerHtml.Contains(">Overall Place<") || e.InnerHtml.Contains(">Age Place<"))
                    {
                        if (e.ChildNodes[2].InnerHtml.Length > 0) le2.setNotes(le2.getNotes() + "\n" + e.ChildNodes[1].InnerHtml + ": " + e.ChildNodes[2].InnerHtml);
                    }
                    else if (e.InnerHtml.Contains(">Distance<"))
                    {
                        String[] dist = e.LastChild.InnerText.Split(new[] { "&nbsp;" }, StringSplitOptions.None);
                        double distance = Double.Parse(dist[0]);
                        le2.setDistance(distance);
                        String distanceUnit = dist[1].ToLower();
                        if (!distanceUnit.EndsWith("s")) distanceUnit += "s";
                        le2.setDistanceUnit(distanceUnit);

                        if (le.getDistanceUnit().Equals("miles") && distanceUnit.Equals("kilometers"))
                        {//convert to WU/CD units (only support for km, mi)
                            distance /= 1.609;
                        }
                        else if (le.getDistanceUnit().Equals("kilometers") && distanceUnit.Equals("miles"))
                        {
                            distance *= 1.609;

                        }
                        if (le.getDistance() >= distance)
                        {
                            le.setDistance(le.getDistance() - distance); //compensate for distance run in race
                        }


                    }
                    else if (e.InnerHtml.Contains(">Time<"))
                    {
                        double duration = parseDuration(e.LastChild.InnerText);
                        le2.setDuration(duration);
                        if (le.getDuration() >= duration)
                        {
                            le.setDuration(le.getDuration() - duration); //compensate for time run in race
                        }
                    }
                    else if (e.InnerHtml.Contains(">Comments<"))
                    {
                        String notes = e.LastChild.InnerText;
                        le2.setNotes(le2.getNotes() + "\n\n" + notes);
                    }
                }
                this.entryList.Add(le2);

            }


        }

        private void setOtherInfo(LogEntry le, HtmlNode otherInfo)
        {
            //no data to get for RA here
        }

        private void setXTrainComments(LogEntry le, HtmlNode crossTrainingComments)
        {
            String xtComments = crossTrainingComments.LastChild.LastChild.InnerText;
            if (xtComments.Length > 0)
            {
                le.setNotes(le.getNotes() + "\nCross Training Comments:\n" + xtComments);
            }
        }





    }
}
