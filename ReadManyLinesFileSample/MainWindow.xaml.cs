using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

            Thread thread = new Thread(new ThreadStart(() => { FileThreadFunc(); }));
            Thread thread2 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
            Thread thread3 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
            Thread thread4 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));
            Thread thread5 = new Thread(new ThreadStart(() => { while (true) { Thread.Sleep(1000); } }));

            thread.Start();
        }

        // 파일을 읽는 별도의 스레드를 제어하는 플래그
        bool isReadFileRequested = false;

        // 파일을 읽는 별도의 스레드
        // UI에서 요청이 있을 때, 해당 플래그의 활성화 여부에 따라 파일 읽기를 진행
        public void FileThreadFunc()
        {
            FileReaderTask fileReader = new FileReaderTask();

            while (true)
            {
                if (isReadFileRequested)
                {
                    fileReader.NotTask();
                    isReadFileRequested = false;
                }

                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        public async Task ReadFileTasks()
        {
            FileReaderTask fileReader = new FileReaderTask();

            // 240 만 줄의 파일에 대해서 await Task 할 경우 약 6초

            fileReader.NotTask();

            // fileReader.SimpleTask();
            await fileReader.SimpleTask();

            //for (int i = 1; i <= 10; i++)
            //{
            //    await fileReader.ReadFileWithMultipleTasks(i);
            //}
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            isReadFileRequested = true;
            // await ReadFileTasks();
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
            await Task.Factory.StartNew(() =>
            {
                FileReaderThread fileThread = new FileReaderThread();

                fileThread.ReadFileWithMultipleThread(2);
            });
            isWorking = false;
        }

        private void T1Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            int intTag = int.Parse(btn.Tag.ToString());
            string filename = "LTE_DL_10MHz_16QAM_QPSK.wf";

            FileReaderUtil.ShowConsoleWriteLine($"Start to Read All of File [{intTag}] method");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            switch (intTag)
            {
                case 1: FileReaderMethodTest.T1(filename); break;
                case 2: FileReaderMethodTest.T2(filename); break;
                case 3: FileReaderMethodTest.T3(filename); break;
                case 4: FileReaderMethodTest.T4(filename); break;
                case 5: FileReaderMethodTest.T5(filename); break;
                case 6: FileReaderMethodTest.T6(filename); break;
                case 7: FileReaderMethodTest.T7(filename); break;
                case 8: FileReaderMethodTest.T8(filename); break;
                case 9: FileReaderMethodTest.T9(filename); break;  
            }

            TimeSpan elapsedTime = stopwatch.Elapsed;
            FileReaderUtil.ShowConsoleWriteLine($"End to Read All of File [{intTag}] method, ElaseTime = {elapsedTime}");
        }
    }
}
