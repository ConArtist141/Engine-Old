using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Editor
{
    public partial class ResourceView : UserControl
    {
        public string ResourceRoot { get; set; }
        private string[] SupportedFileExtensions = new[] { ".obj", ".dae" };
        private EditorWindow renderWindow;

        public TreeNode SelectedNode
        {
            get
            {
                return folderView.SelectedNode;
            }
        }

        public ResourceView(EditorWindow window)
        {
            InitializeComponent();

            renderWindow = window;
            ResourceRoot = "Content";

            PopulateResourceView();
        }

        public void PopulateResourceView()
        {
            folderView.Nodes.Clear();

            var root = new TreeNode("Resource Root");
            folderView.Nodes.Add(root);

            PopulateResourceView(ResourceRoot, root);
        }

        public void NofityMouseUp()
        {
            Debug.WriteLine("Mouse Up!");
        }

        public void PopulateResourceView(string directory, TreeNode root)
        {
            if (Directory.Exists(directory))
            {
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    var dirRoot = new TreeNode("[Directory] " + Path.GetFileName(dir));
                    root.Nodes.Add(dirRoot);
                    PopulateResourceView(dir, dirRoot);
                }

                foreach (var file in Directory.GetFiles(directory))
                {
                    if (SupportedFileExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        var fileNode = new TreeNode(Path.GetFileName(file));
                        root.Nodes.Add(fileNode);
                    }
                }
            }
        }
    }
}
