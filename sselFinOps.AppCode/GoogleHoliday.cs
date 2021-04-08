using Ical.Net.CalendarComponents;
using LNF;
using LNF.DataAccess;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sselFinOps.AppCode
{
    public class GoogleHoliday
    {
        private CalendarEvent _entry;

        protected ISession Session => ServiceProvider.Current.DataAccess.Session;

        public GoogleHoliday(CalendarEvent entry)
        {
            _entry = entry;
        }

        public string UID
        {
            get  {return _entry.Uid;}
        }

        public string Name
        {
            get
            {
                //HttpUtility.HtmlDecode(_entry.GetInnerText("atom:title"));
                return _entry.Description;
            }
        }

        public DateTime StartDate
        {
            get
            {
                //<gd:when endTime='2015-01-02' startTime='2015-01-01'/>
                //HttpUtility.HtmlDecode(_entry.GetAttributeValue("gd:when", "startTime"));
                return _entry.Start.AsSystemLocal;
            }
        }

        public DateTime EndDate
        {
            get
            {
                //<gd:when endTime='2015-01-02' startTime='2015-01-01'/>
                //HttpUtility.HtmlDecode(_entry.GetAttributeValue("gd:when", "endTime"));
                return _entry.End.AsSystemLocal;
            }
        }

        public string DisplayStartDate
        {
            get { return StartDate.ToString("dd MMMM yyyy"); }
        }

        public string DisplayEndDate
        {
            get { return EndDate.AddDays(-1).ToString("dd MMMM yyyy"); }
        }

        public bool Available
        {
            get
            {
                return Session.Query<Holiday>().Count(x => x.HolidayDate >= StartDate && x.HolidayDate < EndDate) == 0;
            }
        }

        public IList<Holiday> CreateHolidays()
        {
            IList<Holiday> result = new List<Holiday>();

            DateTime d = StartDate;

            while (d < EndDate)
            {
                var item = new Holiday() { Description = Name, HolidayDate = d };
                Session.Insert(item);
                result.Add(item);
            }

            return result;
        }
    }
}
