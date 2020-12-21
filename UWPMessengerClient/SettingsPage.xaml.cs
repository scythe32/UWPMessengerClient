﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UWPMessengerClient
{
    public sealed partial class SettingsPage : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private string server_address = "m1.escargot.log1p.xyz";//escargot address
        private int server_port = 1863;
        private SocketCommands TestSocket;
        private ObservableCollection<string> errors;

        public SettingsPage()
        {
            this.InitializeComponent();
            SetConfigDefaultValuesIfNull();
            TestSocket = new SocketCommands(server_address, server_port);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButton.IsEnabled = this.Frame.CanGoBack;
            SetSavedSettings();
            errors = (ObservableCollection<string>)e.Parameter;
            var task = TestServer();
            base.OnNavigatedTo(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void version_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["MSNP_Version"] = version_box.SelectedItem.ToString();
            localSettings.Values["MSNP_Version_Index"] = version_box.SelectedIndex;
        }

        private void localhost_toggle_Toggled(object sender, RoutedEventArgs e)
        {
            localSettings.Values["Using_Localhost"] = localhost_toggle.IsOn;
        }

        private void SetConfigDefaultValuesIfNull()
        {
            if (localSettings.Values["MSNP_Version"] == null)
            {
                localSettings.Values["MSNP_Version"] = "MSNP15";
            }
            if (localSettings.Values["MSNP_Version_Index"] == null)
            {
                localSettings.Values["MSNP_Version_Index"] = 0;
            }
            if (localSettings.Values["Using_Localhost"] == null)
            {
                localSettings.Values["Using_Localhost"] = false;
            }
        }

        private async Task TestServer()
        {
            server_status.Text = "Testing server response time...";
            Stopwatch stopwatch = new Stopwatch();
            string status = "";
            await Task.Run(() =>
            {
                TestSocket.ConnectSocket();
                TestSocket.SetReceiveTimeout(15000);//15 seconds means server offline
                byte[] buffer = new byte[4096];
                TestSocket.SendCommand("VER 1 MSNP15 CVR0\r\n");
                stopwatch.Start();
                try
                {
                    TestSocket.ReceiveMessage(buffer);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    if (e.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
                    {
                        status = "offline";
                    }
                }
                stopwatch.Stop();
            });
            if (stopwatch.Elapsed.TotalSeconds > 5 && stopwatch.Elapsed.TotalSeconds < 15)//acceptable response time is 5 seconds, greater than this means congested server
            {
                status = "congested";
            }
            else if (stopwatch.Elapsed.TotalSeconds < 5)
            {
                status = "online";
            }
            server_status.Text = $"The server is currently {status} - {stopwatch.Elapsed.TotalSeconds} seconds response time";
        }

        private void SetSavedSettings()
        {
            version_box.SelectedIndex = (int)localSettings.Values["MSNP_Version_Index"];
            localhost_toggle.IsOn = (bool)localSettings.Values["Using_Localhost"];
        }

        private async void testServerButton_Click(object sender, RoutedEventArgs e)
        {
            await TestServer();
        }
    }
}
