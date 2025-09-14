using System.Drawing;
using System.Windows.Forms;

using CKAN.Versioning;
using CKAN.GUI;

namespace WebBrowserPlugin
{
    public class WebBrowserPlugin : IGUIPlugin
    {
        public override void Initialize()
        {
            Util.Invoke(Main.Instance, () =>
            {
                tabController = typeof(Main).GetField("tabController",
                                                      System.Reflection.BindingFlags.NonPublic
                                                      | System.Reflection.BindingFlags.Instance)
                                            .GetValue(Main.Instance) as TabController;

                var webBrowser = new WebBrowser()
                {
                    Name                   = "webBrowser1",
                    Dock                   = DockStyle.Fill,
                    Location               = new Point(3, 3),
                    MinimumSize            = new Size(20, 20),
                    Size                   = new Size(1015, 640),
                    TabIndex               = 0,
                    Url                    = new System.Uri("https://spacedock.info",
                                                            System.UriKind.Absolute),
                    ScriptErrorsSuppressed = true,
                };

                var tabPage = new TabPage()
                {
                    Name                    = "SpaceDockBrowserTabPage",
                    Location                = new Point(4, 22),
                    Padding                 = new Padding(3),
                    Size                    = new Size(1021, 646),
                    TabIndex                = 5,
                    Text                    = "SpaceDock",
                    UseVisualStyleBackColor = true,
                };
                tabPage.Controls.Add(webBrowser);
                tabController.m_TabPages.Add("SpaceDockBrowser", tabPage);
                tabController.ShowTab("SpaceDockBrowser", 1, false);
            });
        }

        public override void Deinitialize()
        {
            Util.Invoke(Main.Instance, () =>
            {
                tabController.HideTab("SpaceDockBrowser");
                tabController.m_TabPages.Remove("SpaceDockBrowser");
            });
        }

        public override string GetName() => "Web Browser";

        public override ModuleVersion GetVersion() => new ModuleVersion(VERSION);

        private TabController tabController;

        private static readonly string VERSION = "v1.0.0";
    }
}
