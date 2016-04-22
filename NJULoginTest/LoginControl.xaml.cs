using System;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NJULoginTest
{
    public sealed partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            this.InitializeComponent();
        }
        private void StateSwitch(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, (sender as Button).Content as string, true);
        }
        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            switch (RequestedTheme)
            {
                case ElementTheme.Dark:
                    this.RequestedTheme = ElementTheme.Light;
                    break;
                case ElementTheme.Default:
                case ElementTheme.Light:
                    this.RequestedTheme = ElementTheme.Dark;
                    break;
            }
        }
    }
}
