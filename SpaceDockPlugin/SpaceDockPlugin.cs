using System;
using System.Collections.Generic;
using System.Windows.Forms;

using mshtml;

using CKAN;
using CKAN.Versioning;
using CKAN.GUI;

namespace SpaceDockPlugin
{

    public class SpaceDockPlugin : IGUIPlugin
    {

        private readonly ModuleVersion VERSION = new ModuleVersion("v1.0.0");

        private Dictionary<int, CkanModule> SpaceDockToCkanMap = new Dictionary<int, CkanModule>();

        public override void Initialize()
        {
            tabController = typeof(Main).GetField("tabController",
                                                  System.Reflection.BindingFlags.NonPublic
                                                  | System.Reflection.BindingFlags.Instance)
                                        .GetValue(Main.Instance) as TabController;

            using (var regMgr = RegistryManager.Instance(Main.Instance.CurrentInstance,
                                                         new RepositoryDataManager()))
            {
                var registry = regMgr.registry;

                var kspVersion = Main.Instance.CurrentInstance.VersionCriteria();

                foreach (var module in registry.CompatibleModules(Main.Instance.CurrentInstance.StabilityToleranceConfig,
                                                                  kspVersion))
                {
                    var latest = registry.LatestAvailable(module.identifier,
                                                          Main.Instance.CurrentInstance.StabilityToleranceConfig,
                                                          kspVersion);
                    if (latest.resources != null)
                    {
                        if (latest.resources.spacedock != null)
                        {
                            int ks_id = int.Parse(latest.resources.spacedock.ToString().Split('/')[4]);
                            SpaceDockToCkanMap[ks_id] = latest;
                        }
                    }
                }
            }

            var webBrowser = new WebBrowser();
            webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser.Url = new System.Uri("https://SpaceDock.info", System.UriKind.Absolute);

            webBrowser.DocumentCompleted += (sender, args) =>
            {
                var thumbnails = GetElementsByClass(webBrowser.Document, "thumbnail");
                foreach (var thumbnail in thumbnails)
                {
                    var url = thumbnail.Children[1].GetAttribute("href");
                    var ksmod_id = int.Parse(url.Split('/')[4]);

                    var module = CkanModuleForSpaceDockID(ksmod_id);
                    if (module != null)
                    {
                        thumbnail.Children[0].InnerHtml = "<img src=\"https://raw.githubusercontent.com/KSP-CKAN/CKAN-cmdline/master/assets/ckan-64.png\"/>";
                        if (IsModuleInstalled(module.identifier))
                        {
                            thumbnail.Children[0].InnerHtml += "<div style=\"margin-top: 32px;\" class=\"ksp-update\">Installed</div>";
                        }
                    }
                }

                HtmlElement downloadLink = webBrowser.Document.GetElementById("download-link-primary");
                if (downloadLink == null)
                {
                    return;
                }

                int mod_id = -1;

                var downloadUrl = downloadLink.GetAttribute("href");
                if (downloadUrl.StartsWith("#"))
                {
                    mod_id = int.Parse(downloadUrl.Substring(1));
                }
                else if (!int.TryParse(downloadUrl.Split('/')[4], out mod_id))
                {
                    mod_id = -1;
                }

                var ckanModule = CkanModuleForSpaceDockID(mod_id);
                if (ckanModule != null)
                {
                    downloadLink.SetAttribute("href", "#" + mod_id.ToString());

                    if (IsModuleInstalled(ckanModule.identifier))
                    {
                        downloadLink.InnerHtml = "Installed";
                    }
                    else if (IsModuleSelectedForInstall(ckanModule.identifier))
                    {
                        downloadLink.InnerHtml = "Selected for install";
                    }
                    else
                    {
                        downloadLink.InnerHtml = "Add to CKAN install";

                        webBrowser.Document.Body.MouseDown += (o, e) =>
                        {
                            switch (e.MouseButtonsPressed)
                            {
                                case MouseButtons.Left:
                                    HtmlElement element = webBrowser.Document.GetElementFromPoint(e.ClientMousePosition);
                                    if (element != null && element.Id == "download-link-primary")
                                    {
                                        SelectModuleForInstall(ckanModule.identifier);
                                    }

                                    break;
                            }
                        };
                    }
                }
            };

            var tabPage = new TabPage();
            tabPage.Name = "SpaceDockBrowserTabPage";
            tabPage.Text = "SpaceDock";

            tabPage.Controls.Add(webBrowser);

            tabController.m_TabPages.Add("SpaceDockBrowser", tabPage);
            tabController.ShowTab("SpaceDockBrowser", 1, false);
        }
        static IEnumerable<HtmlElement> GetElementsByClass(HtmlDocument doc, string className)
        {
            foreach (HtmlElement e in doc.All)
                if (e.GetAttribute("className") == className)
                    yield return e;
        }

        public override void Deinitialize()
        {
            tabController.HideTab("SpaceDockBrowser");
            tabController.m_TabPages.Remove("SpaceDockBrowser");
        }

        public override string GetName() => "SpaceDockPlugin by nlight";

        public override ModuleVersion GetVersion() => VERSION;

        private bool IsModuleInstalled(string identifier)
        {
            using (var regMgr = RegistryManager.Instance(Main.Instance.CurrentInstance,
                                                         new RepositoryDataManager()))
            {
                var registry = regMgr.registry;

                return registry.InstalledModule(identifier) != null;
            }
        }

        private bool IsModuleSelectedForInstall(string identifier)
            => !IsModuleInstalled(identifier)
               && !Main.Instance.ManageMods.mainModList
                                           .full_list_of_mod_rows
                                           .TryGetValue(identifier, out DataGridViewRow row)
               && row.Tag is GUIMod gmod
               && gmod.SelectedMod != gmod.InstalledMod.Module;

        private void SelectModuleForInstall(string identifier)
        {
            foreach (DataGridViewRow row in Main.Instance.ManageMods.ModGrid.Rows)
            {
                var mod = ((GUIMod) row.Tag);
                if (mod.Identifier == identifier)
                {
                    (row.Cells[0] as DataGridViewCheckBoxCell).Value = true;
                    mod.SelectedMod = mod.LatestCompatibleMod;
                }
            }
        }

        private CkanModule CkanModuleForSpaceDockID(int id)
        {
            if (!SpaceDockToCkanMap.ContainsKey(id))
            {
                return null;
            }

            return SpaceDockToCkanMap[id];
        }

        private TabController tabController;
    }
}
