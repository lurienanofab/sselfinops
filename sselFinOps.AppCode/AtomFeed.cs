using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.IO;

namespace sselFinOps.AppCode
{
    public class AtomFeed
    {
        private XmlDocument xdoc;
        private XmlNamespaceManager nsmgr;

        public Uri Uri { get; set; }

        public AtomFeed()
        {
            Uri = null;
        }

        public AtomFeed(Uri uri)
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
            string content = string.Empty;
            using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
            {
                content = reader.ReadToEnd();
                reader.Close();
            }
            xdoc = new XmlDocument();
            xdoc.LoadXml(content);
            nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("gCal", "http://schemas.google.com/gCal/2005");
            nsmgr.AddNamespace("gd", "http://schemas.google.com/g/2005");
        }

        public string FeedTitle()
        {
            XmlNode node = GetNode(xdoc, "/atom:feed/atom:title");
            return node.InnerText;
        }

        public IEnumerable<AtomEntry> Entries()
        {
            XmlNodeList nodes = GetNodes(xdoc, "/atom:feed/atom:entry");
            foreach (XmlNode node in nodes)
            {
                yield return new AtomEntry(node, this);
            }
        }

        internal XmlNode GetNode(XmlNode root, string xpath)
        {
            XmlNode node = root.SelectSingleNode(xpath, nsmgr);
            if (node == null)
                throw new Exception("Node not found for xpath: " + xpath);
            return node;
        }

        internal XmlNodeList GetNodes(XmlNode root, string xpath)
        {
            return root.SelectNodes(xpath, nsmgr);
        }

        public class AtomEntry
        {
            private XmlNode node;
            private AtomFeed parent;

            internal AtomEntry(XmlNode node, AtomFeed parent)
            {
                this.node = node;
                this.parent = parent;
            }

            //public string Title
            //{
            //    get
            //    {
            //        XmlNode n = parent.GetNode(node, "atom:title");
            //        return n.InnerText;
            //    }
            //}

            //public string Summary
            //{
            //    get
            //    {
            //        XmlNode n = parent.GetNode(node, "atom:summary");
            //        return n.InnerText;
            //    }
            //}

            //public string Content
            //{
            //    get
            //    {
            //        XmlNode n = parent.GetNode(node, "atom:content");
            //        return n.InnerText;
            //    }
            //}

            public string GetInnerText(string xpath)
            {
                try
                {
                    XmlNode n = parent.GetNode(node, xpath);
                    return n.InnerText;
                }
                catch
                {
                    return string.Empty;
                }
            }

            public string GetAttributeValue(string xpath, string attr)
            {
                try
                {
                    XmlNode n = parent.GetNode(node, xpath);
                    return n.Attributes[attr].Value;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
