using System.Windows.Forms;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

using CKAN.Versioning;
using CKAN.GUI;

namespace LogViewPlugin
{
    public class LogViewPlugin : IGUIPlugin
    {
        public override void Initialize()
        {
            tabController = typeof(Main).GetField("tabController",
                                                  System.Reflection.BindingFlags.NonPublic
                                                  | System.Reflection.BindingFlags.Instance)
                                        .GetValue(Main.Instance) as TabController;

            var tabPage = new TabPage();
            tabPage.Name = "LogViewTabPage";
            tabPage.Text = "LogView";

            var textbox = new TextBox();
            textbox.Dock = System.Windows.Forms.DockStyle.Fill;
            textbox.Multiline = true;
            textbox.Name = "LogViewTextBox";
            textbox.ReadOnly = true;
            textbox.ScrollBars = ScrollBars.Both;

            tabPage.Controls.Add(textbox);

            tabController.m_TabPages.Add("LogViewTabPage", tabPage);
            tabController.ShowTab("LogViewTabPage", 1, false);

            Hierarchy h = (Hierarchy)LogManager.GetRepository();
            h.Root.Level = Level.All;

            IAppender appender = new LogAppender(textbox);
            h.Root.AddAppender(appender);

            LogManager.GetRepository().Threshold = Level.Info;
        }

        public override void Deinitialize()
        {
            tabController.HideTab("LogViewTabPage");
            tabController.m_TabPages.Remove("LogViewTabPage");
        }

        public override string GetName() => "LogView by nlight";
        public override ModuleVersion GetVersion() => VERSION;

        private TabController tabController;

        private static readonly ModuleVersion VERSION = new ModuleVersion("v1.0.0");
    }
}
