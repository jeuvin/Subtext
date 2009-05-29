#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Web;
using System.Web.Routing;
using log4net;
using Subtext.BlogML;
using Subtext.Framework;
using Subtext.Framework.Configuration;
using Subtext.Framework.Exceptions;
using Subtext.Framework.Logging;
using Subtext.Framework.Providers;
using Subtext.Framework.Routing;
using Subtext.ImportExport;

namespace Subtext.Web.Admin.Pages
{
	/// <summary>
	/// Renders the page used to import and export blog data using 
	/// the BlogML format proposed in 
	/// <see href="http://markitup.com/Posts/PostsByCategory.aspx?categoryId=5751cee9-5b20-4db1-93bd-7e7c66208236">this blog</see>
	/// </summary>
    public partial class ImportExportPage : AdminOptionsPage
	{
		private readonly static ILog log = new Log();

        protected override void OnInit(EventArgs e)
        {
            this.Load += this.Page_Load;
            base.OnInit(e);
        }

		protected void btnSave_Click(object sender, EventArgs e)
		{
			Response.Redirect("Handlers/BlogMLExport.ashx?embed=" + chkEmbedAttach.Checked);
		}

		protected void btnLoad_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				try
				{
                    //Temporarily extend script timeout for large BlogML imports
                    if(Server.ScriptTimeout < 3600)
                        Server.ScriptTimeout = 3600;
                    LoadBlogML();
				}
				catch(InvalidOperationException)
				{
					Messages.ShowError("The file you specified does not appear to be a valid BlogML file.", true);
				}
			}
		}

		private void LoadBlogML() {
            ISubtextContext context = SubtextContext;

            var provider = new SubtextBlogMLProvider(Config.ConnectionString, context);

			BlogMLReader bmlReader = BlogMLReader.Create(provider);
			
			try
			{
                bmlReader.ReadBlog(importBlogMLFile.PostedFile.InputStream);
			}
			catch(BlogImportException bie)
			{
				log.Error("Import of BlogML file failed.", bie);
				Messages.ShowError(bie.Message, true);
			}
		    finally
			{
			    importBlogMLFile.PostedFile.InputStream.Close();
			}

			Messages.ShowMessage("The BlogML file was successfully imported!");
		}

        protected void btnClearContent_Click(object sender, EventArgs e)
        {
            if (chkClearContent.Checked)
            {
                chkClearContent.Checked = false;
                chkClearContent.Visible = false;
                btnClearContent.Visible = false;
                
                Blog.ClearBlogContent(Config.CurrentBlog.Id);
                msgpnlClearContent.ShowMessage("Success! The content has been obliterated!");
            }
            else
                msgpnlClearContent.ShowError(@"You need to check the ""Clear Content"" checkbox to continue.");
        }
	}
}


