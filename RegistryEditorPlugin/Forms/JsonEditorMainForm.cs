using System.Drawing;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZTn.Json.Editor.Linq;

using CKAN.GUI;
using CKAN;

namespace ZTn.Json.Editor.Forms
{
    public sealed partial class JsonEditorMainForm : Form
    {
        private const string DefaultFileFilters = @"json files (*.json)|*.json";

        #region >> Delegates

        private delegate void SetActionStatusDelegate(string text, bool isError);

        private delegate void SetJsonStatusDelegate(string text, bool isError);

        #endregion

        #region >> Fields

        private JTokenRoot jsonEditorItem;

        private System.Timers.Timer jsonValidationTimer;

        #endregion

        #region >> Properties

        #endregion

        #region >> Constructor

        public JsonEditorMainForm()
        {
            InitializeComponent();

            jsonTypeComboBox.DataSource = Enum.GetValues(typeof(JTokenType));

            jsonTreeView.AfterCollapse += jsonTreeView_AfterCollapse;
            jsonTreeView.AfterExpand += jsonTreeView_AfterExpand;

            SetActionStatus(@"Empty document.", true);
            SetJsonStatus(@"", false);
        }

        #endregion

        #region >> Form

        /// <inheritdoc />
        /// <remarks>
        /// Optimization aiming to reduce flickering on large documents (successfully).
        /// Source: http://stackoverflow.com/a/89125/1774251
        /// </remarks>
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;    // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        #endregion

        /// <summary>
        /// For the clicked node to be selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jsonTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            jsonTreeView.SelectedNode = e.Node;
        }

        private void jsonTreeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            var node = e.Node as IJsonTreeNode;
            if (node != null)
            {
                node.AfterCollapse();
            }
        }

        private void jsonTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            var node = e.Node as IJsonTreeNode;
            if (node != null)
            {
                node.AfterExpand();
            }
        }

        private void jsonValueTextBox_TextChanged(object sender, EventArgs e)
        {
            var node = jsonTreeView.SelectedNode as IJsonTreeNode;
            if (node == null)
            {
                return;
            }

            StartValidationTimer(node);
        }

        private void jsonValueTextBox_Leave(object sender, EventArgs e)
        {
            jsonValueTextBox.TextChanged -= jsonValueTextBox_TextChanged;
        }

        private void jsonValueTextBox_Enter(object sender, EventArgs e)
        {
            jsonValueTextBox.TextChanged += jsonValueTextBox_TextChanged;
        }

        #region >> Methods jsonTreeView_AfterSelect

        /// <summary>
        /// Main event handler dynamically dispatching the handling to specialized methods.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jsonTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            JsonTreeView_AfterSelectImplementation((dynamic)e.Node, e);
        }

        /// <summary>
        /// Default catcher in case of a node of unattended type.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="e"></param>
        // ReSharper disable once UnusedParameter.Local
        private void JsonTreeView_AfterSelectImplementation(TreeNode node, TreeViewEventArgs e)
        {
            newtonsoftJsonTypeTextBox.Text = "";

            jsonTypeComboBox.Text = String.Format("{0}: {1}", JTokenType.Undefined, node.GetType().FullName);

            jsonValueTextBox.ReadOnly = true;
        }

        // ReSharper disable once UnusedParameter.Local
        private void JsonTreeView_AfterSelectImplementation(JTokenTreeNode node, TreeViewEventArgs e)
        {
            newtonsoftJsonTypeTextBox.Text = node.Tag.GetType().Name;

            jsonTypeComboBox.Text = node.JTokenTag.Type.ToString();

            // If jsonValueTextBox is focused then it triggers this event in the update process, so don't update it again ! (risk: infinite loop between events).
            if (!jsonValueTextBox.Focused)
            {
                jsonValueTextBox.Text = node.JTokenTag.ToString();
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void JsonTreeView_AfterSelectImplementation(JValueTreeNode node, TreeViewEventArgs e)
        {
            newtonsoftJsonTypeTextBox.Text = node.Tag.GetType().Name;

            jsonTypeComboBox.Text = node.JValueTag.Type.ToString();

            switch (node.JValueTag.Type)
            {
                case JTokenType.String:
                    jsonValueTextBox.Text = @"""" + node.JValueTag + @"""";
                    break;
                default:
                    jsonValueTextBox.Text = node.JValueTag.ToString();
                    break;
            }
        }

        #endregion

        public void SetJsonSourceStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }


            try
            {
                jsonEditorItem = new JTokenRoot(stream);
            }
            catch
            {
                MessageBox.Show(this, string.Format("An error occured when reading JSON"), @"Open...");

                SetActionStatus(@"Document NOT loaded.", true);

                return;
            }

            SetActionStatus(@"Document successfully loaded.", false);

            jsonTreeView.Nodes.Clear();
            jsonTreeView.Nodes.Add(JsonTreeNodeFactory.Create(jsonEditorItem.JTokenValue));
            jsonTreeView.Nodes
                .Cast<TreeNode>()
                .ForEach(n => n.Expand());
        }

        private void SetActionStatus(string text, bool isError)
        {
            if (InvokeRequired)
            {
                Invoke(new SetActionStatusDelegate(SetActionStatus), new object[] { text, isError });
                return;
            }

            actionStatusLabel.Text = text;
            actionStatusLabel.ForeColor = isError ? Color.OrangeRed : Color.Black;
        }

        private void SetJsonStatus(string text, bool isError)
        {
            if (InvokeRequired)
            {
                Invoke(new SetJsonStatusDelegate(SetActionStatus), new object[] { text, isError });
                return;
            }

            jsonStatusLabel.Text = text;
            jsonStatusLabel.ForeColor = isError ? Color.OrangeRed : Color.Black;
        }

        private void StartValidationTimer(IJsonTreeNode node)
        {
            if (jsonValidationTimer != null)
            {
                jsonValidationTimer.Stop();
            }

            jsonValidationTimer = new System.Timers.Timer(250);

            jsonValidationTimer.Elapsed += (o, args) =>
            {
                jsonValidationTimer.Stop();

                jsonTreeView.Invoke(new Action<IJsonTreeNode>(JsonValidationTimerHandler), new object[] { node });
            };

            jsonValidationTimer.Start();
        }

        private void JsonValidationTimerHandler(IJsonTreeNode node)
        {
            jsonTreeView.BeginUpdate();

            try
            {
                jsonTreeView.SelectedNode = node.AfterJsonTextChange(jsonValueTextBox.Text);

                SetJsonStatus("Json format validated.", false);
            }
            catch (JsonReaderException exception)
            {
                SetJsonStatus(
                    String.Format("INVALID Json format at (line {0}, position {1})", exception.LineNumber, exception.LinePosition),
                    true);
            }
            catch
            {
                SetJsonStatus("INVALID Json format", true);
            }

            jsonTreeView.EndUpdate();
        }

        private void SaveChangesButton_Click(object sender, EventArgs e)
        {
            var json = jsonEditorItem.GetJsonString();

            var settings = new JsonSerializerSettings
            {
                Context = new System.Runtime.Serialization.StreamingContext(
                    System.Runtime.Serialization.StreamingContextStates.Other,
                    Main.Instance.CurrentInstance
                )
            };

            using (var regMgr = RegistryManager.Instance(Main.Instance.CurrentInstance,
                                                         new RepositoryDataManager()))
            {
                regMgr.registry = JsonConvert.DeserializeObject<CKAN.Registry>(json, settings);
            }
        }
    }
}
