using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LNF.Web.Content;

namespace sselFinOps
{
    public partial class Error : LNFErrorPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override Button SendButton
        {
            get { throw new NotImplementedException(); }
        }

        protected override TextBox DescriptionTextBox
        {
            get { throw new NotImplementedException(); }
        }

        protected override Label DoneMessageLabel
        {
            get { throw new NotImplementedException(); }
        }

        protected override Literal ErrorMessageLiteral
        {
            get { throw new NotImplementedException(); }
        }

        protected override HyperLink ReloadPageHyperlink
        {
            get { throw new NotImplementedException(); }
        }
    }
}