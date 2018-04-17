using Ical.Net;
using Ical.Net.CalendarComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace sselFinOps.AppCode
{
    public class iCalFeed
    {
        public Uri Uri { get; private set; }

        private Calendar _calendar;

        public iCalFeed(Uri uri)
        {
            Refresh(uri);
        }

        public void Refresh(Uri uri)
        {
            Uri = uri;
            Refresh();
        }

        public void Refresh()
        {
            if (Uri == null)
                throw new ArgumentNullException("Uri");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Uri);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream response = resp.GetResponseStream();
            _calendar = Calendar.Load(response);
        }

        public string FeedTitle
        {
            get { return _calendar.Name; }
        }

        public IEnumerable<CalendarEvent> GetEvents()
        {
            return _calendar.Events.AsEnumerable();
        }
    }
}
