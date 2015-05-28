using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editor
{
    static class Program
    {
        static void Main()
        {
            Application.EnableVisualStyles();

            var window = new EditorWindow();
            window.Run();
        }
    }
}
