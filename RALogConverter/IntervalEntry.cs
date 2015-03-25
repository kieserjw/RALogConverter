using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RALogConverter
{
    class IntervalEntry
    {
        public static int numFields = 5;
        public readonly static String[] fieldHeaders = { "Set", "Reps", "Dist/Time", "Goal", "Actual", "Rep rest", "Set rest" };
        public readonly static String tab = "   ";
        public readonly static String spaceChar = " ";

        public static String headerString(int[] lengths)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(fieldHeaders[0] + tab + fieldHeaders[1] + tab);
            for (int i = 0; i < IntervalEntry.numFields; i++)
            {
                String f = fieldHeaders[i + 2];
                sb.Append(f);
                if (f.Length < lengths[i])
                {
                    for (int z = 0; z < (lengths[i] - f.Length); z++) sb.Append(IntervalEntry.spaceChar);

                }
            }
            return sb.ToString();

        }

        public IntervalEntry(int set, int rep, String[] fields)
        {
            this.set = set;
            this.rep = rep;
            this.fields = fields;
        }
        public IntervalEntry()
        {
            set = rep = 0;
            fields = new String[numFields];
        }
        private int set, rep;
        private String[] fields;

        public String ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(set + tab + rep + tab);
            for (int i = 0; i < fields.Length; i++)
            {
                s.Append(fields[i]);
            }
            return s.ToString();
        }

        public int getSet()
        {
            return set;
        }
        public void setSet(int set)
        {
            this.set = set;
        }
        public int getRep()
        {
            return rep;
        }
        public void setRep(int rep)
        {
            this.rep = rep;
        }
        public String getField(int i)
        {
            if (i >= 0 && (i < IntervalEntry.numFields))
            {
                return this.fields[i];
            }
            else
            {
                return null;
            }
        }
        public void setFields(String fields, int i)
        {
            if (i >= 0 && (i < IntervalEntry.numFields))
            {
                this.fields[i] = fields;
            }
        }

        public String[] getFields()
        {
            return fields;
        }
        public void setFields(String[] fields)
        {
            this.fields = fields;
        }
    }
}
