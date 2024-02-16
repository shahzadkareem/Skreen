using Rocket_Remote;
using StreamLibrary.UnsafeCodecs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using UdtSharp;
using System.IO;
using p2p.StunServer;
using p2pcopy;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Threading.Tasks;
using System.Timers;
using Firebase.Auth;
using FireSharp.Response;
using FireSharp;
using FireSharp.Interfaces;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using ToastNotifications.Messages.Error;
using System.Windows.Forms.DataVisualization.Charting;
using System.Reflection.Emit;

namespace Rocket_Remote
{
    public partial class Form1 : Form
    {
        private string _authToken;
        private System.Windows.Forms.Timer timer;
        IFirebaseConfig config = new FireSharp.Config.FirebaseConfig()
        {
            AuthSecret = "KxuFtJgR3gg3I9byaokqEG1DA1m7vkvbE6RQywqA",
            BasePath = "https://remotive-e5933-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient clientreq;
        string UserId;
        System.Timers.Timer t;
        int h, m, s;
        private static int count = 0;
        public Form1(string id)
        {
            UserId = id;
            _authToken = Program.GetAuthToken();
            // Set the LicenseContext to NonCommercial to resolve the LicenseException
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            GlobalVariables.Root = this;
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Text = string.Empty;
            //  this.ControlBox = false;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            this.Anchor = AnchorStyles.None;
            this.Resize += new EventHandler(Form1_Resize);
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000; // 2 seconds
            timer.Tick += Timer_Tick;
        }
        public Form1()
        {
            // Set the LicenseContext to NonCommercial to resolve the LicenseException
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            GlobalVariables.Root = this;
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Text = string.Empty;
            //this.ControlBox = false;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            this.Anchor = AnchorStyles.None;
            this.Resize += new EventHandler(Form1_Resize);
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000; // 2 seconds
            timer.Tick += Timer_Tick;

        }

        RunningApp runningApps = new RunningApp();
        static Random r = new Random();
        public string myname = "Me";
        public string peername = "Peer";
        bool bConnected = false;

        Thread thread;

        Socket socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

        UdtSocket connection;

        //private readonly string StunServersJson = Directory.GetCurrentDirectory() + "\\StunServers.json";


        private void Form1_Load(object sender, EventArgs e)
        {
            roundControl1.Anchor = AnchorStyles.Left;
            roundControl4.Anchor = AnchorStyles.Left;
            panel2.Visible = true;
            panel1.Visible = true;
            button2.Enabled = false;
            try
            {
                clientreq = new FireSharp.FirebaseClient(config);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Check your connection");

            }
            myname = Environment.UserName;
            //  dspeed.SelectedIndex = 2;
            //CheckDataGridView();
            GetEndPoint();
            btndisconnect.Enabled = false;
            btndisconnect.BackColor = Color.Gray;
            // Adjust vertical centering through padding
            int verticalPadding = (txtnsg.Height - txtnsg.Font.Height) / 2;
            txtnsg.Padding = new Padding(txtnsg.Padding.Left, verticalPadding, txtnsg.Padding.Right, verticalPadding);
            txtRemoteIP.Focus();
            panelchat.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            setting_panel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        }

        private void CheckDataGridView()
        {
            //if (File.Exists(StunServersJson))
            //{
            //    // Read data from StunServersJson
            //    Array StunServers = StunServer.GetStunServersFromFile(StunServersJson);
            //    if (StunServers.Length > 0)
            //    {
            //        int i = 0;
            //        foreach (var _stun in StunServer.GetStunServersFromFile(StunServersJson))
            //        {
            //            // this.dataGridView1.Rows.Insert(i, _stun.Server, _stun.Port);
            //            i++;
            //        }
            //    } else
            //    {
            //        //this.dataGridView1.Rows.Insert(0, "stun.l.google.com", 19302);
            //    }
            //}
            //else
            //{
            //    // Save data from dataGridView to StunServersJson
            //    //this.dataGridView1.Rows.Insert(0, "stun.l.google.com", 19302);
            //    //SaveDataToJsoin();
            //}
        }

        private void GetEndPoint()
        {
            int newPort = r.Next(49152, 65535);
            socket.Bind(new IPEndPoint(IPAddress.Any, newPort));

            P2pEndPoint p2pEndPoint = GetExternalEndPoint(socket);

            if (p2pEndPoint == null)
                return;

            // txtmyHost.Text = Functions.Base64Encode(p2pEndPoint.External.ToString());
            //  txtmyHost.Text = p2pEndPoint.External.ToString();
            Clipboard.SetText(p2pEndPoint.External.ToString());
            string localendpoint = p2pEndPoint.Internal.ToString();
            string[] words = localendpoint.Split(':');
            // txtLocalHost.Text = Functions.Base64Encode(GetPhysicalIPAdress() + ":" + words[1]);
            txtLocalHost.Text = GetPhysicalIPAdress() + ":" + words[1];
        }

        private string GetPhysicalIPAdress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
                if (addr != null && !addr.Address.ToString().Equals("0.0.0.0"))
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return String.Empty;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            thread = new Thread(() => connect());
            thread.Start();

            button2.Enabled = false;
            btndisconnect.Enabled = true;
        }

        private void connect()
        {
            try
            {
                string remoteIp;
                int remotePort;

                // string peer = Functions.Base64Decode(txtRemoteIP.Text);
                string peer = txtRemoteIP.Text;
                if (string.IsNullOrEmpty(peer))
                {
                    MessageBox.Show("Invalid ip:port entered");
                    button2.Enabled = true;
                    return;
                }
                // try again to connect to external to "reopen" port
                GetExternalEndPoint(socket);
                ParseRemoteAddr(peer, out remoteIp, out remotePort);
                if (remoteIp!=null)
                {
                    connection = PeerConnect(socket, remoteIp, remotePort);
                }
                else
                {
                    // Show message box on the UI thread
                    this.Invoke(new MethodInvoker(delegate
                    {
                        DialogResult result = MessageBox.Show("Check your IP address and Try Again", "Error", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            return;
                        }
                    }));

                }


                if (connection == null)
                {
                    label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.Red));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Failed to establish Remote connection"));
                    string errorMessage = label4.Text;
                    // Use Invoke to update UI from a non-UI thread
                    label4.Invoke((MethodInvoker)(() =>
                    {
                        label4.ForeColor = Color.Red;
                        label4.Text = "Failed to establish Remote connection ";
                    }));

                    // Display the message in a MessageBox
                    // Show message box on the UI thread
                    this.Invoke(new MethodInvoker(delegate
                    {
                        label4.ForeColor = Color.Red;
                        label4.Text = "Failed to establish Remote connection";
                        MessageBox.Show("Failed to establish Remote connection", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return;
                }
                try
                {
                    Thread t = new Thread(new ParameterizedThreadStart(SenderReceiver.Run));
                    t.Start(connection);
                }
                catch (System.IO.IOException e1)
                {
                    r_chat.Invoke((MethodInvoker)(() => r_chat.ForeColor = Color.Red));
                    r_chat.Invoke((MethodInvoker)(() => r_chat.Text = "Connection Error: " + e1.Message));
                }
            }
            catch (System.IO.IOException e2)
            {
                r_chat.Invoke((MethodInvoker)(() => r_chat.ForeColor = Color.Red));
                r_chat.Invoke((MethodInvoker)(() => r_chat.Text = "Connection Error: " + e2.Message));
            }
        }

        static void ParseRemoteAddr(string addr, out string remoteIp, out int port)
        {
            if(addr.Contains(":")) {
                string[] split = addr.Split(':');

                remoteIp = split[0];
                port = int.Parse(split[1]);

            }
            else
            {
                remoteIp = null;
                port = 0;
            }

        }

        class P2pEndPoint
        {
            internal IPEndPoint External;
            internal IPEndPoint Internal;
        }

        private P2pEndPoint GetExternalEndPoint(Socket socket)
        {
            List<Tuple<string, int>> stunServers = new List<Tuple<string, int>>();

            // Read Stun Servers from dataGridView
            for (int rows = 0; rows < 1; rows++)
            {
                string Server = Convert.ToString("stun.l.google.com");
                int Port = Convert.ToInt32(19302);

                stunServers.Add(new Tuple<string, int>(Server, Port));

            }

            // https://gist.github.com/zziuni/3741933

            // stunServers.Add(new Tuple<string, int>("stun.l.google.com", 19302));

            Console.WriteLine("Contacting STUN servers to obtain your IP");

            foreach (Tuple<string, int> server in stunServers)
            {
                string host = server.Item1;
                int port = server.Item2;

                StunResult externalEndPoint = StunClient.Query(host, port, socket);

                if (externalEndPoint.NetType == StunNetType.UdpBlocked)
                {
                    continue;
                }

                Console.WriteLine("Your firewall is {0}", externalEndPoint.NetType.ToString());

                return new P2pEndPoint()
                {
                    External = externalEndPoint.PublicEndPoint,
                    Internal = (socket.LocalEndPoint as IPEndPoint)
                };
            }

            MessageBox.Show("Your external IP can't be obtained. Could not find a working STUN server :-( ");
            return null;
        }

        public UdtSocket PeerConnect(Socket socket, string remoteAddr, int remotePort)
        {
            bConnected = false;
            SenderReceiver.isConnected = false;
            int retry = 0;

            UdtSocket client = null;

            while (!bConnected)
            {
                try
                {
                    int sleepTimeToSync = 1;

                    label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.Black));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Waiting " + sleepTimeToSync + "  sec to sync with other peer"));
                    Thread.Sleep(sleepTimeToSync * 1000);

                    GetExternalEndPoint(socket);

                    if (client != null)
                        client.Close();

                    client = new UdtSocket(socket.AddressFamily, socket.SocketType);
                    client.Bind(socket);

                    retry++;
                    label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.Black));
                    label4.Invoke((MethodInvoker)(() => label4.Text = retry + " Trying to connect to " + remoteAddr + ":" + remotePort));

                    if (txtLocalHost.Text.Equals(txtRemoteIP.Text))
                    {
                        // Show message box on the UI thread
                        this.Invoke(new MethodInvoker(delegate
                        {
                            DialogResult result = MessageBox.Show("You Can not Connect your own PC", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (result == DialogResult.OK)
                            {
                                return;
                            }
                        }));
                        break;
                    }
                    else
                    {
                        client.Connect(new IPEndPoint(IPAddress.Parse(remoteAddr), remotePort));
                    }

                    label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.DarkGreen));
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Connected successfully to " + remoteAddr + ":" + remotePort));
                    string successMessage = label4.Text;

                    // Use Invoke to update UI from a non-UI thread
                    label4.Invoke((MethodInvoker)(() =>
                    {
                        label4.ForeColor = Color.DarkGreen;
                        label4.Text = "Connected successfully to " + remoteAddr + ":" + remotePort;
                    }));

                    // Display the success message in a MessageBox
                    // MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    SenderReceiver.isConnected = true;
                    bConnected = true;

                    if (SenderReceiver.isConnected == true)
                    {
                        button2.Enabled = false;
                        button2.BackColor = Color.Gray;
                        btndisconnect.Enabled = true;
                        btndisconnect.BackColor = Color.FromArgb(254, 26, 26);
                        runningApps.IP = remoteAddr;
                        runningApps.StartTime = DateTime.Now;
                        SaveLog();
                        btnRdp.Enabled = true;
                        btnchat.Enabled = true;
                        var logs = new ConnectionLog
                        {

                            StartTime = DateTime.Now.ToString("dd/MMM/yyyy HH:mm:ss"),
                            EndTime = "",
                            LocalIP = GetLocalIPAddress(),
                            RemoteIP = remoteAddr,
                            LocalMac = GetMacAddress()
                        };

                        FirebaseResponse logResp = clientreq.Set("connection_logs/" + UserId + "/" + DateTime.Now.ToString("dd-MMM-yyyy") + "/" + Guid.NewGuid().ToString(), logs);
                        // User login was successful
                        MessageBox.Show("Connected successfully to " + remoteAddr + ":" + remotePort, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Initialize the timer
                        t = new System.Timers.Timer();
                        t.Interval = 30000; // 1 minutes in milliseconds
                        t.Elapsed += OnTimeEvent;
                        t.Start();
                        Task.Run(() => timerMethod());

                    }
                }
                catch (Exception e)
                {
                    label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.Red));
                    label4.Invoke((MethodInvoker)(() => label4.Text = e.Message.Replace(Environment.NewLine, ". ")));
                }
            }
            return client;
        }

        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                s++;
                if (s == 60) {
                    s = 0;
                    m += 1;

                }
                if (m == 60)
                {
                    m = 0;
                    h += 1;
                }
            }));

            label10.Text = string.Format("{0}:{1}:{2}", h.ToString().PadLeft(2, '0'), m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
            if (label10.Text == "00:30:00")
            {
                button2.Enabled = true;
                button2.BackColor = Color.FromArgb(33, 206, 60);
                btndisconnect.Enabled = false;

                SenderReceiver.isConnected = false;
                bConnected = false;
                label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.Black));
                label4.Invoke((MethodInvoker)(() => label4.Text = "Disconnected"));
                if (!runningApps.EndTime.HasValue)
                {
                    runningApps.EndTime = DateTime.Now;
                    SaveLog();
                    txtRemoteIP.Text = "";
                    txtRemoteIP.Focus();

                }
                t.Stop();
                label10.Text = "00:00:00";
                MessageBox.Show("Your session has reached the limit");

            }
        }
        private string GetMacAddress()
        {
            string macAddress = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    continue;

                if (nic.GetIPProperties().GetIPv4Properties().IsDhcpEnabled)
                {
                    macAddress = nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddress;
        }

        private string GetLocalIPAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
                if (addr != null && !addr.Address.ToString().Equals("0.0.0.0"))
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return String.Empty;
        }


        private void timerMethod()
        {
            if (label10.ToString() == "1000*30*60")
            {
                t.Stop();
            }

        }

        //private void ReloadTimer_Tick(object sender, EventArgs e)
        //{

        //    // Calculate the elapsed time
        //    TimeSpan elapsedTime = DateTime.Now - startTime;

        //    // Update the label with the elapsed time
        //    label10.Text = $"Elapsed Time: {elapsedTime.TotalMinutes:F0} minutes";




        //}
        private void SaveLog()
        {
            string folderName = "RemoteLogs";
            string fileName = "Logfiledata677.xlsx";
            count = count + 1;
            // Get all available drives on the system
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // Find the first drive that is not the C drive
            DriveInfo selectedDrive = allDrives.FirstOrDefault(drive => !drive.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase));

            // If no other drive is found, default to C drive
            if (selectedDrive == null)
            {
                selectedDrive = allDrives.FirstOrDefault(drive => drive.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase));
            }

            string folderPath = Path.Combine(selectedDrive.Name, folderName);
            string logFilePath = Path.Combine(folderPath, fileName);


            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            FileInfo fileInfo = new FileInfo(logFilePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet;
                if (fileInfo.Exists)
                {
                    worksheet = package.Workbook.Worksheets[0];
                    // Clear data without clearing headings
                    worksheet.Cells["A2:D2" + worksheet.Dimension.Rows].Clear();
                }
                else
                {
                    worksheet = package.Workbook.Worksheets.Add("LogSheet");
                    // Add headings

                    worksheet.Cells["A1"].Value = "Session Count No";
                    worksheet.Cells["B1"].Value = "IP Address";
                    worksheet.Cells["C1"].Value = "Start Time";
                    worksheet.Cells["D1"].Value = "End Time";

                    worksheet.Cells["A1:D1"].Style.Font.Bold = true; // Bold the header row

                }

                int row = worksheet.Dimension.Rows + 1;

                Debug.WriteLine($"Saving: {runningApps.IP}, {runningApps.StartTime}, {runningApps.EndTime}");
                worksheet.Cells[row, 1].Value = count;
                worksheet.Cells[row, 2].Value = runningApps.IP;
                worksheet.Cells[row, 3].Value = runningApps.StartTime.ToString("yyyy-MM-dd HH:mm:ss"); // Format start time
                worksheet.Cells[row, 4].Value = runningApps.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL"; // Format end time or use "N/A"

                row++;


                package.Save();
            }
        }

        private void CloseAll()
        {
            if (bConnected == true)
            {
                SenderReceiver.SendMessage("end|");
                thread.Abort();
                SenderReceiver.netStream.Close();
                SenderReceiver.isConnected = false;
                SenderReceiver.client.Close();
                connection.Close();
                socket.Close();
            }
            Process.GetCurrentProcess().Kill();
        }

        public void EnableStreach(bool sino)
        {
            // checkBox1.Enabled = sino;
        }

        public void EnableDSpeed(bool sino)
        {
            // dspeed.Enabled = sino;
        }

        private delegate void PlaceString(string item);
        public void WriteFPS(string item)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new PlaceString(this.WriteFPS), new object[] { item });
                }
                else
                {
                    // this.lblFPS.Text = item;
                }
            }
            catch
            {
            }
        }
        public void WriteKB(string item)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new PlaceString(this.WriteKB), new object[] { item });
                }
                else
                {
                    // this.lblkb.Text = item;
                }
            }
            catch
            {
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //if (txtnsg.Text != "")
            //{
            //    if(bConnected == true)
            //    {
            //        Writetxtchatrom("Blue", txtnsg.Text);
            //        SenderReceiver.SendMessage("c|" + txtnsg.Text);
            //        txtnsg.Text = "";
            //    }
            //    else
            //    {
            //        MessageBox.Show("You are not connected to peer endpoint");
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Nothing write to send!");
            //}
        }

        private void btn_paste_Click(object sender, EventArgs e)
        {
            //txtRemoteIP.Text = Clipboard.GetText();
            //Clipboard.SetText(txtmyHost.Text);
            //if(txtmyHost.Text == txtRemoteIP.Text || txtLocalHost.Text == txtRemoteIP.Text)
            //{
            //    txtRemoteIP.Text = "";
            //    MessageBox.Show("Please paste peer remote host:port not your!");
            //}
        }

        public void EnableButtonRdp(bool truefalse)
        {
            this.btnRdp.Enabled = truefalse;
        }
        public void Writetxtchatrom(string color, string msg)
        {
            try
            {
                string time = DateTime.Now.ToString("hh:mm");
                this.r_chat.Select(r_chat.TextLength, 0);

                if (color == "Blue")
                {
                    //Set the formatting and color text
                    this.r_chat.SelectionFont = new Font(r_chat.Font, FontStyle.Bold);
                    this.r_chat.SelectionColor = Color.Blue;
                    this.r_chat.AppendText(myname + " [" + time + "]: ");

                    // Revert the formatting back 
                    this.r_chat.SelectionFont = r_chat.Font;
                    this.r_chat.SelectionColor = r_chat.ForeColor;
                    this.r_chat.AppendText(msg + Environment.NewLine);
                }
                else if (color == "Green")
                {
                    //Set the formatting and color text
                    this.r_chat.SelectionFont = new Font(r_chat.Font, FontStyle.Bold);
                    this.r_chat.SelectionColor = Color.Green;
                    this.r_chat.AppendText(peername + " [" + time + "]: ");

                    // Revert the formatting back 
                    this.r_chat.SelectionFont = r_chat.Font;
                    this.r_chat.SelectionColor = r_chat.ForeColor;
                    this.r_chat.AppendText(msg + Environment.NewLine);
                    //red
                }
                else if (color == "Red")
                {
                    //Set the formatting and color text
                    this.r_chat.SelectionFont = new Font(r_chat.Font, FontStyle.Bold);
                    this.r_chat.SelectionColor = Color.Red;
                    this.r_chat.AppendText(peername + " [" + time + "]: " + msg + Environment.NewLine);
                }
            }
            catch
            {
                //}
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                switch (e.CloseReason)
                {
                    case CloseReason.ApplicationExitCall:
                        CloseAll();
                        break;
                    case CloseReason.FormOwnerClosing:
                        CloseAll();
                        break;
                    case CloseReason.MdiFormClosing:
                        CloseAll();
                        break;
                    case CloseReason.None:
                        CloseAll();
                        break;
                    case CloseReason.TaskManagerClosing:
                        CloseAll();
                        break;
                    case CloseReason.UserClosing:
                        CloseAll();
                        break;
                    case CloseReason.WindowsShutDown:
                        CloseAll();
                        break;
                    default:
                        CloseAll();
                        break;
                }
            }
            catch
            {
            }
        }

        private void r_chat_TextChanged(object sender, EventArgs e)
        {
            //r_chat.SelectionStart = r_chat.TextLength;
            //r_chat.ScrollToCaret();
        }

        private void r_chat_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (txtmyHost.Text != "")
            //{
            //    Clipboard.SetText(txtmyHost.Text);
            //}
        }

        private void btnRdp_Click(object sender, EventArgs e)
        {
            //if (bConnected == true)
            //{
            //    var myForm = new pDesktop();
            //    myForm.Show();

            //    RemoteDesktop.UnsafeMotionCodec = new UnsafeStreamCodec(RemoteDesktop.DesktopQuality, true);
            //    SenderReceiver.SendMessage("openp2pDesktop|");
            //    GlobalVariables.Root.EnableButtonRdp(false);
            //}
            //else
            //{
            //    MessageBox.Show("You are not connected to peer endpoint");
            //}
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://www.pocketsolution.net/");
            }
            catch
            {
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //if(RemoteDesktop.DesktopRunning == true)
            //{
            //    if (checkBox1.Checked == true)
            //    {
            //        GlobalVariables.p2pDesktop.ControlStreach(true);
            //    }
            //    else
            //    {
            //        GlobalVariables.p2pDesktop.ControlStreach(false);
            //    }
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (txtLocalHost.Text != "")
            {
                Clipboard.SetText(txtLocalHost.Text);
            }
        }

        private void dspeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (bConnected == true)
            //{
            //    SenderReceiver.SendMessage("ds|" + dspeed.GetItemText(dspeed.SelectedItem));
            //}
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //if (this.dataGridView1.SelectedRows.Count > 0)
            //{
            //    foreach (DataGridViewRow row in this.dataGridView1.SelectedRows)
            //    {
            //        if (row.IsNewRow) continue;
            //        this.dataGridView1.Rows.RemoveAt(row.Index);
            //        CheckJsonFile();                }
            //}
        }

        #region SaveDataToJson()
        //private void SaveDataToJsoin()
        //{
        //    //if (this.dataGridView1.Rows.Count == 1 && File.Exists(StunServersJson))
        //    //{
        //    //    File.Delete(StunServersJson);
        //    //    this.dataGridView1.Rows.Insert(0, "stun.l.google.com", 19302);
        //    //}

        //    var ListJsonStunServers = new List<StunServer>();

        //    for (int rows = 0; rows < 1; rows++)
        //    {
        //        string Server = Convert.ToString("stun.l.google.com");
        //        int Port = Convert.ToInt32(19302);

        //        var _stun = new StunServer
        //        {
        //            Server = Server,
        //            Port = Port,
        //        };
        //        ListJsonStunServers.Add(_stun);

        //    }

        //    StunServer.WriteStunServersToFile(ListJsonStunServers, StunServersJson);

        //    //if (this.dataGridView1.Rows.Count > 1)
        //    //{
        //    //    MessageBox.Show("Stun Servers List saved to: " + StunServersJson);
        //    //}
        //}

        #endregion
        private void btndisconnect_Click(object sender, EventArgs e)
        {
            txtRemoteIP.ReadOnly = false;
            txtRemoteIP.Clear();
            button2.Enabled = true;
            btndisconnect.Enabled = false;
            // SenderReceiver.isConnected = false;
            bConnected = false;
            label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.Black));
            label4.Invoke((MethodInvoker)(() => label4.Text = "Disconnected"));
            string disconnectMessage = label4.Text;

            // Use Invoke to update UI from a non-UI thread
            label4.Invoke((MethodInvoker)(() =>
            {
                label4.ForeColor = Color.Black;
                label4.Text = "Disconnected";
            }));

            // Display the disconnect message in a MessageBox
            MessageBox.Show(disconnectMessage, "Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (!runningApps.EndTime.HasValue)
            {
                runningApps.EndTime = DateTime.Now;
                SaveLog();

            }
        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            thread = new Thread(() => connect());
            thread.Start();

        }

        private void btndisconnect_Click_1(object sender, EventArgs e)
        {
            txtRemoteIP.ReadOnly = false;
            txtRemoteIP.Clear();
            button2.Enabled = true;
            btndisconnect.Enabled = false;
            button2.BackColor = Color.FromArgb(33, 206, 60);
            btndisconnect.Enabled = false;
            btnRdp.Enabled = false;
            btnchat.Enabled = false;
            SenderReceiver.isConnected = false;
            bConnected = false;
            label4.Invoke((MethodInvoker)(() => label4.ForeColor = Color.Black));
            label4.Invoke((MethodInvoker)(() => label4.Text = "Disconnected"));
            if (!runningApps.EndTime.HasValue)
            {
                runningApps.EndTime = DateTime.Now;
                SaveLog();
                txtRemoteIP.Text = "";
                txtRemoteIP.Focus();
            }

        }

        private void btnRdp_Click_1(object sender, EventArgs e)
        {
            if (bConnected == true)
            {
                var myForm = new pDesktop();
                myForm.Show();

                RemoteDesktop.UnsafeMotionCodec = new UnsafeStreamCodec(RemoteDesktop.DesktopQuality, true);
                SenderReceiver.SendMessage("openp2pDesktop|");
                GlobalVariables.Root.EnableButtonRdp(false);
            }
            else
            {
                MessageBox.Show("You are not connected to peer endpoint");
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtRemoteIP_TextChanged(object sender, EventArgs e)
        {
            if (txtRemoteIP.Text != "")
            {
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        private void roundControl4_Load(object sender, EventArgs e)
        {

        }

        private void txtRemoteIP_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow numbers (0-9), dot (.), colon (:), and backspace (\b)
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ':' && e.KeyChar != '\b')
            {
                e.Handled = true; // Suppress the key press
            }
        }
        private bool IsSpecialCharacter(char c)
        {
            // Define the special characters you want to allow
            char[] allowedSpecialChars = { '.', ':' };

            // Check if the character is in the allowed special characters
            return allowedSpecialChars.Contains(c);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void CPpictureBox_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtLocalHost.Text);
            Clipboard.SetText(txtLocalHost.Text);
            label12.Visible = true;
            // Start the timer
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Hide label12
            label12.Visible = false;
        }
        private void txtnsg_MouseHover(object sender, EventArgs e)
        {
            
            //txtnsg.Clear();
        }

        private void txtRemoteIP_MouseHover(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            panelchat.Visible = true;
            setting_panel.Visible = false;
            txtnsg.Visible = true;
            button4.Visible = true;
            r_chat.Visible = true;
            panel1.Visible = true;
            panel2.Visible = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

            panelchat.Visible = false;
           
            panel1.Visible = true;
            panel2.Visible = true;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            if (txtnsg.Text != "")
            {
                if (bConnected == true)
                {
                    Writetxtchatrom("Blue", txtnsg.Text);
                    SenderReceiver.SendMessage("c|" + txtnsg.Text);
                    txtnsg.Text = "";
                }
                else
                {
                    MessageBox.Show("You are not connected to peer endpoint");
                }
            }
            else
            {
                MessageBox.Show("Nothing write to send!");
            }
        }

        private void settingBtn_Click(object sender, EventArgs e)
        {
            txtnsg.Visible = false;
            button4.Visible = false;
            r_chat.Visible = true;
            panelchat.Visible = true;
            panel1.Visible = true;
            panel2.Visible = true;
            setting_panel.Visible = true;
        }

        private void logout_label_Click(object sender, EventArgs e)
        {

            _authToken = null;
            Program.SaveAuthToken(_authToken);

            Login login = new Login();
            this.Hide();
            login.ShowDialog();

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(this, e.Location);
            }
        }

        private void setting_panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtRemoteIP_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Ctrl+V is pressed
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Paste the text from the clipboard
                txtRemoteIP.Text = Clipboard.GetText();
            }
        }

        private void txtnsg_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtnsg_MouseHover_1(object sender, EventArgs e)
        {

           
        }

        private void txtnsg_TextChanged_1(object sender, EventArgs e)
        {
            //   AdjustRichTextBoxHeight();
        }
        private void AdjustRichTextBoxHeight()
        {
            // Calculate the new height based on the number of lines
            int lineHeight = txtnsg.GetPositionFromCharIndex(txtnsg.GetFirstCharIndexFromLine(1)).Y;
            int newHeight = txtnsg.Lines.Length * lineHeight + txtnsg.Margin.Vertical;

            // Set a minimum height to prevent excessive shrinking
            int minHeight = lineHeight * 3;

            // Adjust the height of the RichTextBox
            txtnsg.Height = Math.Max(minHeight, newHeight);
        }


        private void txtnsg_SizeChanged(object sender, EventArgs e)
        {

        }

        private void txtnsg_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            AdjustRichTextBoxHeight();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // AdjustRoundControl();

            float newSize = this.ClientSize.Width / 70;

            label2.Font = new Font(label2.Font.FontFamily, newSize);

            //int newWidth = this.ClientSize.Width - 1180; // Adjust as needed
            //int newHeight = panel1.Height; // Keep the same height

            //// Set the new size for the Panel
            //panel1.Size = new Size(newWidth, newHeight);

            // Calculate the new width and height for the PictureBox
            //int  newWidthpic = this.ClientSize.Width / 2; // Adjust as needed
            // int newHeightpic = this.ClientSize.Height / 2; // Adjust as needed

            // // Set the new size for the PictureBox
            // pictureBox5.Size = new Size(newWidthpic, newHeightpic);

            // Size currentSizedis = btndisconnect.Size;

            // // Calculate the new width and height for the Button
            // int newWidthdisconbtn = currentSizedis.Width / 1; // Adjust as needed
            // int newHeightdisconbtn = currentSizedis.Height / 1; // Adjust as needed

            // // Set the new size for the Button
            // btndisconnect.Size = new Size(newWidthdisconbtn, newHeightdisconbtn);


            // Size currentSizecon = button2.Size;

            // // Calculate the new width and height for the Button
            // int newWidthconbtn = currentSizecon.Width / 5; // Adjust as needed
            // int newHeightconbtn = currentSizecon.Height / 5; // Adjust as needed

            // // Set the new size for the Button
            // button2.Size = new Size(newWidthconbtn, newHeightconbtn);
        }
       
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
           
        }

        private void txtnsg_Enter(object sender, EventArgs e)
        {
            txtnsg.Clear();
        }

        private void r_chat_TextChanged_1(object sender, EventArgs e)
        {
            r_chat.SelectionStart = r_chat.TextLength;
            r_chat.ScrollToCaret();
        }

        private void btnchat_Click(object sender, EventArgs e)
        {
            panelchat.Visible=true;
        }
    }

    public class GlobalVariables
    {
        public static Form1 Root;
        public static pDesktop p2pDesktop;
    }
   public class RunningApp
    {
        public int count { get; set; }
        public string IP { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

    }
}

