using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WcfServiceLibraryNamedPipe;

namespace Client2
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherHelper.Instance().Init(this);
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Speaker.Instance().DisConnect();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Speaker.Instance().Connect(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Speaker.Instance().SendMessage(new CommunicatUnit
            {
                Message = TextBox_SendMessage.Text
            });
        }

        public void CreateOtherClientsButtons(string clientName)
        {
            foreach (object ui in StackPanel_ClientsButton.Children)
            {
                if ((ui as Button).Content.ToString() == clientName)
                    return;
            }
            var btn = new Button();
            btn.Content = clientName;
            btn.Padding = new Thickness(5, 0, 5, 0);
            btn.Margin = new Thickness(5, 0, 5, 0);
            btn.Click += OtherClientsButton_Click;
            StackPanel_ClientsButton.Children.Add(btn);
        }

        private void OtherClientsButton_Click(object sender, RoutedEventArgs e)
        {
            var datas = new List<CommunicatUnit>();
            string name = AppDomain.CurrentDomain.FriendlyName;
            for (int i = 0; i < 1000; i++)
            {
                datas.Add(new CommunicatUnit
                {
                    Message = name + " " + DateTime.Now
                });
            }
            Speaker.Instance().SendMessageToClient(datas, ((Button) sender).Content.ToString());
        }
    }
}