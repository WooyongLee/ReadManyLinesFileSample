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

            // 240 만 줄의 파일에 대해서 await Task 할 경우 약 6초

            // fileReader.SimpleTask();
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

        bool isWorking = false;
        private async void Button2_Click(object sender, RoutedEventArgs e)
        {
            // 240 만 줄의 파일에 대해서 Thread 로는 약 1~1.5초 정도
            if (isWorking)
            {
                MessageBox.Show("Already Working");
                return;
            }

            isWorking = true;
            await Task.Factory.StartNew(() => { 
                FileReaderThread fileThread = new FileReaderThread();

                fileThread.ReadFileWithMultipleThread(2);
            });
            isWorking = false;
        }
    }
}
