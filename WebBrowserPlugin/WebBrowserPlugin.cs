using System;
using System.Windows.Forms;
using CKAN;

namespace WebBrowserPlugin
{

    public class WebBrowserPlugin : CKAN.IGUIPlugin
    {

        private readonly string VERSION = "v1.0.0";

        public override void Initialize()
        {
            var webBrowser = new WebBrowser();
            webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser.Location = new System.Drawing.Point(3, 3);
            webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            webBrowser.Name = "webBrowser1";
            webBrowser.Size = new System.Drawing.Size(1015, 640);
            webBrowser.TabIndex = 0;
            webBrowser.Url = new System.Uri("http://kerbalstuff.com", System.UriKind.Absolute);

            var tabPage = new TabPage();
            tabPage.Controls.Add(webBrowser);
            tabPage.Location = new System.Drawing.Point(4, 22);
            tabPage.Name = "tabPage1";
            tabPage.Padding = new System.Windows.Forms.Padding(3);
            tabPage.Size = new System.Drawing.Size(1021, 646);
            tabPage.TabIndex = 5;
            tabPage.Text = "KerbalStuff browser";
            tabPage.UseVisualStyleBackColor = true;

            Main.Instance.m_TabController.m_TabPages.Add("KerbalStuffBrowser", tabPage);
            Main.Instance.m_TabController.ShowTab("KerbalStuffBrowser", 1, false);
        }

        public override void Deinitialize()
        {
            
        }

        public override string GetName()
        {
            return "Web Browser";
        }

        public override CKAN.Version GetVersion()
        {
            return new CKAN.Version(VERSION);
        }

    }

}
