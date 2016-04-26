using System;
using System.Collections.Generic;
using System.ComponentModel;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NJULoginTest
{
    public sealed partial class LoginControl : UserControl,INotifyPropertyChanged
    {
        public LoginControl()
        {
            this.InitializeComponent();
            Title = "Login &amp; Logout";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Username))); }
        }
        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password))); }
        }
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))); }
        }

        private void StateSwitch(LoginUIState dest_state)
        {
            VisualStateManager.GoToState(this, dest_state.ToString(), true);
        }
    }
    public enum LoginUIState
    {
        Normal = 0,
        NoNetwork,
        LoggedIn,
        LoggedOut,
        Waiting
    }
}
