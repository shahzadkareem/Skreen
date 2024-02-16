using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Auth;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;


using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Rocket_Remote
{
    


    public partial class Login : Form
    {
        private int loginAttempts = 0;
        private DateTime lockoutTime = DateTime.MinValue;
        private string _authToken;

        public string userid;
        IFirebaseConfig config = new FireSharp.Config.FirebaseConfig()
        {
            AuthSecret = "KxuFtJgR3gg3I9byaokqEG1DA1m7vkvbE6RQywqA",
            BasePath = "https://remotive-e5933-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;

        public Login()
        {
            //_authToken = null;
            InitializeComponent();
            //  pictureBox1.Image = Properties.Resources.loading_7528_128;
            //this.Anchor = AnchorStyles.None;
            
            
        }
       
        private void Login_Load(object sender, EventArgs e)
        {
            
            btnlogin.Enabled = false;
            btnlogin.BackColor = Color.Gray;
            try 
            {
                client = new FireSharp.FirebaseClient(config);
            
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message + "Check your connection");
            
            }

        }

        private void label3_Click(object sender, EventArgs e)
        {
            SignUp form = new SignUp();
            this.Hide();
            form.ShowDialog();
        }
        private bool IsValidGmailAddress(string email)
        {
            // Use a regular expression to validate the Gmail address
            string pattern = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(email);
        }
       
        private async void btnlogin_Click(object sender, EventArgs e)
        {
            
            
            if (string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("All fields must be filled");
            }
            else 
            {
                if (DateTime.Now < lockoutTime)
                {
                    MessageBox.Show("Your account is locked. Please try again later.");
                    return;
                }
                string apiKey = "AIzaSyCteZwUnKHqv1CDmhSzEciU7ESO3YhvFa4";
                try
                {

                    var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(apiKey));

                    var authResult = await authProvider.SignInWithEmailAndPasswordAsync(txtEmail.Text, txtPassword.Text);
                    _authToken = authResult.FirebaseToken;
                    userid = authResult.User.LocalId;
                    // Check if the User property is not null
                    if (authResult.User != null)
                    {


                        var logs = new LoginLogs
                        {
                            UserId = authResult.User.LocalId,
                            dateOfLogin = DateTime.Now.ToString("dd/MMM/yyyy HH:mm:ss"),
                            LocalIP = GetLocalIPAddress(),
                            LocalMac = GetMacAddress()
                        };

                        FirebaseResponse logResp = client.Set("login_logs/" + authResult.User.LocalId + "/" + DateTime.Now.ToString("dd-MMM-yyyy") + "/" + Guid.NewGuid().ToString(), logs);
                        // User login was successful

                        MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK);

                        Program.SaveAuthToken(_authToken);

                        Form1 main = new Form1(authResult.User.LocalId);
                        this.Hide();
                        main.ShowDialog();
                        loginAttempts = 0;
                        // Additional actions you may want to perform after successful login
                    }
                    else
                    {
                        // User property is null, indicating a failure in login
                        MessageBox.Show("User Not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (FirebaseAuthException ex)
                {
                    loginAttempts++;

                    if (loginAttempts >= 3)
                    {
                        // Lock account for 5 minutes
                        lockoutTime = DateTime.Now.AddMinutes(5);
                        MessageBox.Show("Too many unsuccessful login attempts. Your account is now locked for 5 minutes.");
                    }
                    else
                    {
                        // Handle exceptions or errors during login
                        MessageBox.Show($"Email or Password is wrong! Kindly check and try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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

      
        private async void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("All fields must be filled");
                }
                else
                {
                    string apiKey = "AIzaSyCteZwUnKHqv1CDmhSzEciU7ESO3YhvFa4";
                    try
                    {

                        var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(apiKey));

                        var authResult = await authProvider.SignInWithEmailAndPasswordAsync(txtEmail.Text, txtPassword.Text);
                        _authToken = authResult.FirebaseToken;
                        userid = authResult.User.LocalId;
                        // Check if the User property is not null
                        if (authResult.User != null)
                        {


                            var logs = new LoginLogs
                            {
                                UserId = authResult.User.LocalId,
                                dateOfLogin = DateTime.Now.ToString("dd/MMM/yyyy HH:mm:ss"),
                                LocalIP = GetLocalIPAddress(),
                                LocalMac = GetMacAddress()
                            };

                            FirebaseResponse logResp = client.Set("login_logs/" + authResult.User.LocalId + "/" + DateTime.Now.ToString("dd-MMM-yyyy") + "/" + Guid.NewGuid().ToString(), logs);
                            // User login was successful
                            MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK);

                            Program.SaveAuthToken(_authToken);

                            Form1 main = new Form1(authResult.User.LocalId);
                            this.Hide();
                            main.ShowDialog();

                            // Additional actions you may want to perform after successful login
                        }
                        else
                        {
                            // User property is null, indicating a failure in login
                            MessageBox.Show("User Not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (FirebaseAuthException ex)
                    {
                        // Handle exceptions or errors during login
                        MessageBox.Show($"Email or Password is wrong! Kindly check and try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            string password = txtPassword.Text;

            // Check if the password meets the criteria
            if (password.Length >= 8 )
            {
                // Password meets the criteria, you can enable a button or provide feedback
                // For example, enable a "Submit" button
                btnlogin.Enabled = true;
                btnlogin.BackColor = Color.FromArgb(46, 46, 46);

            }
            else
            {
                // Password doesn't meet the criteria, you can disable a button or provide feedback
                // For example, disable a "Submit" button
                btnlogin.Enabled = false;
                btnlogin.BackColor = Color.Gray;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SignUp form = new SignUp();
            this.Hide();
            form.ShowDialog();
        }
    }
}
