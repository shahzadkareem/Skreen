using Firebase.Auth;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using TextBox = System.Windows.Forms.TextBox;

namespace Rocket_Remote
{
    public partial class SignUp : Form
    {
        private string _authToken;
        private const int MaxLength = 20;
        public SignUp()
        {
            InitializeComponent();
          
        }
        IFirebaseConfig config = new FireSharp.Config.FirebaseConfig()
        {
            AuthSecret = "KxuFtJgR3gg3I9byaokqEG1DA1m7vkvbE6RQywqA",
            BasePath = "https://remotive-e5933-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;

        private void bntchoosenfile_Click(object sender, EventArgs e)
        {
            // Show a file dialog to select an image file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                Title = "Select an Image File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected image file name (without the full path)
                string imageName = Path.GetFileName(openFileDialog.FileName);
                txtchoosenfile.Text  = imageName;

                // Load the image into PictureBox
                Image image = Image.FromFile(openFileDialog.FileName);

                // Convert the image to base64
                string base64String = ImageToBase64(image, System.Drawing.Imaging.ImageFormat.Jpeg);

               
            }

        }

        private string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        private void SignUp_Load(object sender, EventArgs e)
        {
            btnsignup.Enabled = false;
            btnsignup.BackColor = Color.Gray;
            try
            {
                client = new FireSharp.FirebaseClient(config);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Check your connection");

            }
        }

        private void btnsignup_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("All fields must be filled");
            }
            else
            {
                
                try
                {
                    RegisterUser(txtEmail.Text,txtPassword.Text);

                    
                }
                catch (FirebaseAuthException ex)
                {
                    MessageBox.Show($"Error creating user: {ex.Reason}", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


        private async void RegisterUser(string email, string password)
        {
            string apiKey = "AIzaSyCteZwUnKHqv1CDmhSzEciU7ESO3YhvFa4";
            // Create user in Firebase Authentication
            var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(apiKey));
            try
            {
                var authResult = await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
                await authProvider.SendEmailVerificationAsync(authResult);
                //_authToken = authResult.FirebaseToken;

                if (authProvider.SendEmailVerificationAsync(authResult).IsCompleted)
                {
                    if (authResult.User != null)
                    {

                        var userID = authResult.User.LocalId;
                        var register = new RegisterPerson
                        {
                            UserId = userID,
                            Name = txtUserName.Text,
                            Email = txtEmail.Text,
                            Password = txtPassword.Text
                        };

                        var details = new RegisterPerson
                        {
                            UserId = userID,
                            CreatedAt = DateTime.Now.Date.ToString("dd/MMM/yy"),
                            LocalMac = GetMacAddress(),
                            LocalIp = GetLocalIPAddress()
                        };

                        FirebaseResponse response = client.Set("users/" + userID, register);
                        FirebaseResponse detResp = client.Set("user_details/" + userID, details);

                        MessageBox.Show("Registered Successfully! A Verification email has been sent to your email address. Kindly verify your email before Login.", "Registered!", MessageBoxButtons.OK);

                        //Program.SaveAuthToken(_authToken);

                        Login form = new Login();
                        this.Hide();
                        form.ShowDialog();
                    }
                }
                else
                {
                    txtEmail.Focus();
                    toolTip1.Show("Email Address is wrong. Please check Email address.", txtEmail, 0, -20);
                }
            }
            catch(FirebaseAuthException ex)
            {
                MessageBox.Show($"Error creating user: Check your email & Try Again with a correct email.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void label3_Click(object sender, EventArgs e)
        {
            //Login LOG = new Login();
            //this.Hide();
            //LOG.ShowDialog();
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtUserName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed key is a letter or a space, or it's the Backspace key
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != (char)Keys.Back)
            {
                // If it's not a letter, space, or Backspace, suppress the key press
                e.Handled = true;
            }
        }
        private bool IsValidGmailAddress(string email)
        {
            // Use a regular expression to validate the Gmail address
            string pattern = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(email);
        }
        private void txtEmail_Leave(object sender, EventArgs e)
        {
            string enteredEmail = txtEmail.Text.Trim();

            if (!IsValidGmailAddress(enteredEmail))
            {
                // Show tooltip with error message
                toolTip1.Show("Not a valid Email address. Please enter a valid Email address.", txtEmail, 0, -20);
                txtEmail.Clear();
                txtEmail.Focus();
            }
            else
            {
                // Hide the tooltip if the entered email is valid
                toolTip1.Hide(txtEmail);
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            string password = txtPassword.Text;

            // Check if the password meets the criteria
            if (password.Length >= 8 &&
                password.Any(char.IsUpper) &&
                password.Any(char.IsLower))
            {
                // Password meets the criteria, you can enable a button or provide feedback
                // For example, enable a "Submit" button
                btnsignup.Enabled = true;
                btnsignup.BackColor = Color.FromArgb(46, 46, 46);

            }
            else
            {
                // Password doesn't meet the criteria, you can disable a button or provide feedback
                // For example, disable a "Submit" button
                btnsignup.Enabled = false;
                btnsignup.BackColor = Color.Gray;
            }
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
             if(txtPassword.Text.Length == 8 && btnsignup.Enabled == true)
                {
                    if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
                    {
                        MessageBox.Show("All fields must be filled");
                    }
                    else
                    {

                        try
                        {
                            RegisterUser(txtEmail.Text, txtPassword.Text);


                        }
                        catch (FirebaseAuthException ex)
                        {
                            MessageBox.Show($"Error creating user: {ex.Reason}", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }
                }
                else
                {
                    btnsignup.Enabled = false;
                }
            
            }
            }

        private void txtUserName_Leave(object sender, EventArgs e)
        {
           

            if (txtUserName.Text =="")
            {
                // Show tooltip with error message
                toolTip1.Show("Please Enter FullName first.", txtUserName, 0, -20);
                txtUserName.Clear();
                txtUserName.Focus();
            }
            else
            {
                // Hide the tooltip if the entered email is valid
                toolTip1.Hide(txtUserName);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login LOG = new Login();
            this.Hide();
            LOG.ShowDialog();
        }

        private void txtUserName_TextChanged_1(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.TextLength > MaxLength)
            {
                textBox.Text = textBox.Text.Substring(0, MaxLength);
                textBox.SelectionStart = MaxLength;
                textBox.SelectionLength = 0;
                labinput.Text = "Max. 20 characters allowed";
            }
            else
            {
                // Remove any invalid characters (non-alphabetic and non-space)
                string filteredText = new string(textBox.Text.Where(c => char.IsLetter(c) || c == ' ').ToArray());

                // Only update the TextBox.Text if the filtered text is different
                if (filteredText != textBox.Text)
                {
                    textBox.Text = filteredText;
                    textBox.SelectionStart = textBox.Text.Length;
                }

                labinput.Text = ""; // Clear the label if the input is valid
            }

        }
    }
}

