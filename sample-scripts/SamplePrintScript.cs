using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//internal references
using BioSero.ReadyLabel.Scripting;
using BioSero.ReadyLabel;
using BioSero.Utilities;
using MahApps.Metro.Controls;
using BioSero.Utilities.Metro.Controls;

namespace BioSero.ReadyLabel.Scripting
{
    public class Script
    {
        string PrintLogFileName;
        //EmptyMetroWindow window;

        public void OnStart(int numberOfLabelsToPrint)
        {
            // This procedure is called before any printing begins.  It can be used to open the connection to a database or create a log file for logging all printed data.

            //Set log path
            PrintLogFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Ready Label\\Print Log " + DateTime.NowdToString("yyyy_MM_dd_HH_mm_ss") + ".txt";


            //User Input -> display a message or get user input here
            //Thread thread = new Thread(() =>
            // {
            //     window = new EmptyMetroWindow();
            //     window.Width = 400;
            //     window.Height = 200;
            //     window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //     window.Title = "Example Window";

            //     TextBlock block = new TextBlock();
            //     block.Text = "This is an example.";
            //     block.Margin = new Thickness(5);
            //     block.FontSize = 16;

            //     Button okButton = new Button();
            //     okButton.Content = "Okay";
            //     okButton.Margin = new Thickness(5,20,5,5);
            //     okButton.Width = 80;
            //     okButton.Click += okButton_Click;

            //     StackPanel stackpanel = new StackPanel();
            //     stackpanel.Orientation = Orientation.Vertical;
            //     stackpanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //     stackpanel.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            //     stackpanel.Children.Add(block);
            //     stackpanel.Children.Add(okButton);

            //     window.LayoutRoot.Children.Add(stackpanel);

            //     window.ShowDialog();
            // });
            //thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            //thread.Start();
            //thread.Join();          

            
        }

        //void okButton_Click(object sender, RoutedEventArgs e)
        //{
        //    window.Close();
        //}

        public byte[] ManipulateRawData(BioSero.ReadyLabel.LabelPrinter printer, byte[] rawData, BioSero.ReadyLabel.Label label)
        {
            // This procedure can be used to either manipulate the raw printer data before it is sent to the printer (for instance, special printer control characters could be added), 
            // or used to display data for debugging purposes. 

            //File.WriteAllBytes(@"c:\output.txt");
            //System.Diagnostics.Process.Start("notepad.exe", @"c:\output.txt")

            return rawData;

            //Alternatively, data could be converted to a string then displayed in a message box.
            //string text = System.Text.Encoding.Default.GetString(rawData);
            //MessageBox.Show(text);
            //return System.Text.Encoding.Default.GetBytes(text);
        }

        public void OnEachLabelPrintComplete(BioSero.ReadyLabel.Label label, Dictionary<string, string> data)
        {
            // this procedure is called after each label is printed

            if (!File.Exists(PrintLogFileName)) // write header if first time
            {
                List<string> headers = new List<string>(data.Keys);
                headers.Insert(0, "Element Count");
                headers.Insert(1, "Print Error");
                headers.Insert(2, "Error Text");
                File.WriteAllText(PrintLogFileName, string.Join(",", headers) + Environment.NewLine);
            }
            string values = label.PrintElements.Count() + "," + label.LabelHasError.ToString() + "," + label.LabelErrorText + ",";
            values += string.Join(",", data.Values);
            File.AppendAllText(PrintLogFileName, values + Environment.NewLine);          
        }

        public void OnEnd()
        {
            // this procedure is called after all labels have been printed.
        }
    }
}