using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Rocket_Remote
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static IFirebaseConfig config = new FireSharp.Config.FirebaseConfig()
        {
            AuthSecret = "KxuFtJgR3gg3I9byaokqEG1DA1m7vkvbE6RQywqA",
            BasePath = "https://remotive-e5933-default-rtdb.firebaseio.com/"
        };

        static IFirebaseClient client;

        private static string _authToken;

        [STAThread]
        static void Main()
        { 
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if it's the first time running the application
            if (Properties.Settings.Default.first_run)
            {
                // Perform actions for the first run
                // For example, initialize variables, setup configurations, etc.
                // You can also increment the count variable here
                Properties.Settings.Default.count++;
                Properties.Settings.Default.first_run = false;
                Properties.Settings.Default.Save();

                try
                {
                    client = new FireSharp.FirebaseClient(config);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();

                }

                string macAddress = GetMacAddress();
                string ipAddress = GetLocalIPAddress();

                var install = new Details
                {
                    install_date = DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss"),
                    local_ip = ipAddress,
                    local_mac = macAddress
                };

                FirebaseResponse response = client.Set("installations/" + Guid.NewGuid().ToString(), install);

                Application.Run(new Login());
            }
            else
            {
                // Load the authentication token from shared preferences
                LoadAuthToken();

                if (_authToken != "")
                {
                    Application.Run(new Form1());
                }
                else
                {
                    Application.Run(new Login());
                }
            } 
        }

        // Method to load the authentication token from shared preferences
        private static void LoadAuthToken()
        {
            // Check if the authentication token is stored in shared preferences
            _authToken = Properties.Settings.Default.AuthToken;
        }

        // Method to save the authentication token to shared preferences
        public static void SaveAuthToken(string authToken)
        {
            // Save the authentication token to shared preferences
            Properties.Settings.Default.AuthToken = authToken;
            Properties.Settings.Default.Save();
            _authToken = authToken;
        }

        // Other methods to set and access the authentication token as needed
        public static string GetAuthToken()
        {
            return _authToken;
        }

        private static string GetMacAddress()
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

        private static string GetLocalIPAddress()
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


    }
}
