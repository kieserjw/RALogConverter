using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RALogConverter
{
    class LogEntry
    {
        public LogEntry(DateTime date, String activity, String workout,
            String distanceUnit, String course, String equipmentBrand,
            String equipmentModel, String equipmentSerial, String weightUnit,
            String temperatureUnit, String notes, double distance,
            double weight, double duration, int temperature, int restHR,
            int averageHR, int maxHR, int quality, int effort)
        {
            this.date = date;
            this.activity = activity;
            this.workout = workout;
            this.distanceUnit = distanceUnit;
            this.course = course;
            this.equipmentBrand = equipmentBrand;
            this.equipmentModel = equipmentModel;
            this.equipmentSerial = equipmentSerial;
            this.weightUnit = weightUnit;
            this.temperatureUnit = temperatureUnit;
            this.notes = notes;
            this.distance = distance;
            this.weight = weight;
            this.duration = duration;
            this.temperature = temperature;
            this.restHR = restHR;
            this.averageHR = averageHR;
            this.maxHR = maxHR;
            this.quality = quality;
            this.effort = effort;
        }
        private DateTime date;
        private String activity, workout, distanceUnit, course, equipmentBrand, equipmentModel, equipmentSerial, weightUnit, temperatureUnit, notes;
        private double distance, weight, duration;
        private int temperature, restHR, averageHR, maxHR, quality, effort;

        public LogEntry()
        {
            date = new DateTime();
            activity = workout = course = equipmentBrand = equipmentModel = equipmentSerial = notes = "";
            distanceUnit = "miles";
            weightUnit = "pound";
            temperatureUnit = "F";
            distance = weight = duration = restHR = averageHR = maxHR = 0;
            quality = effort = 0;
            temperature = -100;
        }

        public LogEntry clone()
        {
            return new LogEntry(date, activity, workout,
                    distanceUnit, course, equipmentBrand,
                    equipmentModel, equipmentSerial, weightUnit,
                    temperatureUnit, notes, distance,
                    weight, duration, temperature, restHR,
                    averageHR, maxHR, quality, effort);
        }

        public String toString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(date.ToString("MM/dd/yyyy") + "," +
                date.ToString("h:mm tt") + "," +
                activity + "," +
                workout + "," +
                distance.ToString("F2") + "," +
                distanceUnit + "," +
                duration + "," +
                course + "," +
                equipmentBrand + "," +
                equipmentModel + "," +
                equipmentSerial + ",");
            if (weight != 0) s.Append(weight);
            s.Append(",");
            s.Append(weightUnit + ",");
            if (restHR != 0) s.Append(restHR);
            s.Append(",");
            if (averageHR != 0) s.Append(averageHR);
            s.Append(",");
            if (maxHR != 0) s.Append(maxHR);
            s.Append(",");
            if (temperature != -100) s.Append(temperature);
            s.Append(",");
            s.Append(temperatureUnit + ",");
            if (quality != 0) s.Append(quality);
            s.Append(",");
            if (effort != 0) s.Append(effort);
            s.Append(",");
            s.Append("\"" + notes + "\"");
            return s.ToString();
        }

        public DateTime getDate()
        {
            return date;
        }
        public void setDate(DateTime date)
        {
            this.date = date;
        }
        public double getDuration()
        {
            return duration;
        }
        public void setDuration(double duration)
        {
            this.duration = duration;
        }
        public String getActivity()
        {
            return activity;
        }
        public void setActivity(String activity)
        {
            this.activity = activity;
        }
        public String getWorkout()
        {
            return workout;
        }
        public void setWorkout(String workout)
        {
            this.workout = workout;
        }
        public String getDistanceUnit()
        {
            return distanceUnit;
        }
        public void setDistanceUnit(String distanceUnit)
        {
            this.distanceUnit = distanceUnit;
        }
        public String getCourse()
        {
            return course;
        }
        public void setCourse(String course)
        {
            this.course = course;
        }
        public String getEquipmentBrand()
        {
            return equipmentBrand;
        }
        public void setEquipmentBrand(String equipmentBrand)
        {
            this.equipmentBrand = equipmentBrand;
        }
        public String getEquipmentModel()
        {
            return equipmentModel;
        }
        public void setEquipmentModel(String equipmentModel)
        {
            this.equipmentModel = equipmentModel;
        }
        public String getEquipmentSerial()
        {
            return equipmentSerial;
        }
        public void setEquipmentSerial(String equipmentSerial)
        {
            this.equipmentSerial = equipmentSerial;
        }
        public String getWeightUnit()
        {
            return weightUnit;
        }
        public void setWeightUnit(String weightUnit)
        {
            this.weightUnit = weightUnit;
        }
        public String getTemperatureUnit()
        {
            return temperatureUnit;
        }
        public void setTemperatureUnit(String temperatureUnit)
        {
            this.temperatureUnit = temperatureUnit;
        }
        public String getNotes()
        {
            return notes;
        }
        public void setNotes(String notes)
        {
            this.notes = notes.Replace("\"", "'").Replace("<br>", "\n");
        }
        public double getDistance()
        {
            return distance;
        }
        public void setDistance(double distance)
        {
            this.distance = distance;
        }
        public double getWeight()
        {
            return weight;
        }
        public void setWeight(double weight)
        {
            this.weight = weight;
        }
        public int getRestHR()
        {
            return restHR;
        }
        public void setRestHR(int restHR)
        {
            this.restHR = restHR;
        }
        public int getAverageHR()
        {
            return averageHR;
        }
        public void setAverageHR(int averageHR)
        {
            this.averageHR = averageHR;
        }
        public int getMaxHR()
        {
            return maxHR;
        }
        public void setMaxHR(int maxHR)
        {
            this.maxHR = maxHR;
        }
        public int getTemperature()
        {
            return temperature;
        }
        public void setTemperature(int temperature)
        {
            this.temperature = temperature;
        }
        public int getQuality()
        {
            return quality;
        }
        public void setQuality(int quality)
        {
            this.quality = quality;
        }
        public int getEffort()
        {
            return effort;
        }
        public void setEffort(int effort)
        {
            this.effort = effort;
        }
    }
}
