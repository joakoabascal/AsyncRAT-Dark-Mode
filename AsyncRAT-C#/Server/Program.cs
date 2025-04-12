using Server.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                string batPath = Path.Combine(Application.StartupPath, "Fixer.bat");
                if (!File.Exists(batPath))
                    File.WriteAllText(batPath, Properties.Resources.Fixer);
            }
            catch { }
            form1 = new Form1();
            Application.Run(form1);
        }
        public static Form1 form1;
    }
}
