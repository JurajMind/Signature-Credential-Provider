using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using WcfServiceLibraryNamedPipe;

namespace SignForm
{
    
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
            var memStream = new MemoryStream();

            Stroke wholeStroke = InkCanvas.WholeStroke;
            try
            {
                var s = new StrokeCollection();
                s.Add(wholeStroke);
                s.Save(memStream);


                byte[] buffer = memStream.GetBuffer();

                string str = Encoding.Default.GetString(buffer);


                Speaker.Instance().SendMessage(new CommunicatUnit
                {
                    Message = str
                });
            }


            catch (Exception)
            {
            }
            InkCanvas.Strokes.Clear();

           
        }


        private void CleanButton_OnClick(object sender, RoutedEventArgs e)
        {
          InkCanvas.WholeStroke.StylusPoints.Clear();
         
        }
    }
}