using System.Collections.Generic;
using System.Windows.Forms;

using CKAN.Versioning;
using CKAN.GUI;

namespace PartManagerPlugin
{

    public class PartManagerConfig
    {
        public List<KeyValuePair<string, string>> disabledParts;
    }

    public class PartManagerPlugin : IGUIPlugin
    {
        public override void Initialize()
        {
            tabController = typeof(Main).GetField("tabController",
                                                  System.Reflection.BindingFlags.NonPublic
                                                  | System.Reflection.BindingFlags.Instance)
                                        .GetValue(Main.Instance) as TabController;

            var tabPage = new TabPage();
            tabPage.Name = "PartManager";
            tabPage.Text = "PartManager";

            m_UI = new PartManagerUI();
            m_UI.Dock = DockStyle.Fill;
            tabPage.Controls.Add(m_UI);

            tabController.m_TabPages.Add("PartManager", tabPage);
            tabController.ShowTab("PartManager", 1, false);
        }

        public override void Deinitialize()
        {
            tabController.HideTab("PartManager");
            tabController.m_TabPages.Remove("PartManager");
        }

        public override string GetName() => "PartManager by nlight";
        public override ModuleVersion GetVersion() => VERSION;

        private TabController tabController;
        private PartManagerUI m_UI = null;

        private static readonly ModuleVersion VERSION = new ModuleVersion("v1.1.0");
    }
}
