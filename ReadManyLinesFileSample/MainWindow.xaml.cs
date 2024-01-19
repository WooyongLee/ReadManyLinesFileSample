using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReadManyLinesFileSample
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Thread thread = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
            Thread thread2 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
            Thread thread3 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
            Thread thread4 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
            Thread thread5 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
        }

        public async Task ReadFileTasks()
        {
            FileReaderTask fileReader = new FileReaderTask();

            await fileReader.SimpleTask();

            //for (int i = 1; i <= 10; i++)
            //{
            //    await fileReader.ReadFileWithMultipleTasks(i);
            //}
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            await ReadFileTasks();
        }

        private async void Button2_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                FileReaderThread fileThread = new FileReaderThread();

                fileThread.ReadFileWithMultipleThread(2);
            });
            int a = 9;
        }
    }
}
