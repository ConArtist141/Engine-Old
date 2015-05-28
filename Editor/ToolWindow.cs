using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editor
{
    public partial class ToolWindow : Form
    {
        private EditorWindow renderWindow;

        public ResourceView ResourceView { get; set; }

        public ToolWindow(EditorWindow parent)
        {
            InitializeComponent();

            renderWindow = parent;

            CreateControls();
        }

        public void CreateControls()
        {
            var tabView1 = new TabPage("Resource View");
            ResourceView = new ResourceView(renderWindow);
            tabView1.Controls.Add(ResourceView);
            ResourceView.Dock = DockStyle.Fill;

            tabControl.TabPages.Add(tabView1);
        }
    }
}
