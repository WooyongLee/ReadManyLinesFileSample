using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ReadManyLinesFileSample
{
    public static class FileReaderUtil
    {
        public static void ShowConsoleWriteLine(string log)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "]" + log);
        }

        public static long CalculateByteOffset(StreamReader reader, int targetLineNumber)
        {
            long byteOffset = 0;
            for (int i = 1; i < targetLineNumber; i++)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    throw new InvalidOperationException($"File has less than {targetLineNumber} lines.");
                }
                byteOffset += System.Text.Encoding.UTF8.GetBytes(line).Length + Environment.NewLine.Length;
            }
            return byteOffset;
        }
    }

    public class FileReaderTask
    {
        int[] currentLinesArray;
        long allOfLines;
        int printLineCount = 50000;
        List<string> readLines = new List<string>();

        public void NotTask()
        {
            string filePath = "5G_KT_QPSK_273RB_wfm.wf";

            FileReaderUtil.ShowConsoleWriteLine("Start to Read All of File at NotTask()");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (StreamReader reader = new StreamReader(filePath))
            {
                allOfLines = File.ReadLines(filePath).LongCount();
                string strLine = "";
                var lineNum = 0;

                FileReaderUtil.ShowConsoleWriteLine("Start to check line Num");
                while ((strLine = reader.ReadLine()) != null)
                {
                    lineNum++;
                }
                FileReaderUtil.ShowConsoleWriteLine($"lineNum = {lineNum}");
            }

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            FileReaderUtil.ShowConsoleWriteLine($"End to Read All of File at NotTask(), ElaseTime = {elapsedTime}");
        }

        public async Task SimpleTask()
        {
            string filePath = "5G_KT_QPSK_273RB_wfm.wf";

            FileReaderUtil.ShowConsoleWriteLine("Start to Read All of File at SimpleTask()");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (StreamReader reader = new StreamReader(filePath))
            {
                allOfLines = File.ReadLines(filePath).LongCount();
                string strLine = "";
                var lineNum = 0;

                FileReaderUtil.ShowConsoleWriteLine("Start to check line Num");
                while ((strLine = await reader.ReadLineAsync()) != null)
                {
                    lineNum++;
                }
                 FileReaderUtil.ShowConsoleWriteLine($"lineNum = {lineNum}");
            }

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            FileReaderUtil.ShowConsoleWriteLine($"End to Read All of File at SimpleTask(), ElaseTime = {elapsedTime}");
        }

        public async Task ReadFileWithMultipleTasks(int numberOfTasks)
        {
            if (numberOfTasks <= 0)
            {
                return;
            }

            // Create a Stopwatch instance
            Stopwatch stopwatch = new Stopwatch();

            // Start the stopwatch to measure elapsed time
            stopwatch.Start();

            FileReaderUtil.ShowConsoleWriteLine("Start to Read All of File");

            currentLinesArray = new int[2];
            string filePath = "5G_KT_QPSK_273RB_wfm.wf";

            allOfLines = File.ReadLines(filePath).LongCount();

            #region 2 Task, Forward/backward 방식 (주석)
            //var forwardTask = ReadLinesForwardAsync(filePath);
            //var backwardTask = ReadLinesBackwardAsync(filePath);

            //List<string> forwardLines = await forwardTask;
            //List<string> backwardLines = await backwardTask;

            //List<string> allLinesArray = new List<string>();
            //allLinesArray.AddRange(forwardLines);
            //allLinesArray.AddRange(backwardLines);

            // await Task.WhenAll(forwardTask, backwardTask);
            #endregion

            #region Only 1 Task
            // List<string> allLines = await ReadLinesAsync(filePath, 0, (int)allOfLines, 1);
            #endregion

            #region many Task ()
            List<Task<List<string>>> tasks = new List<Task<List<string>>>();

            int linesPerTask = (int)Math.Ceiling((double)allOfLines / numberOfTasks);

            for (int i = 0; i < numberOfTasks; i++)
            {
                int startLine = i * linesPerTask;
                int endLine = Math.Min((i + 1) * linesPerTask, (int)allOfLines);

                tasks.Add(ReadLinesAsync(filePath, startLine, endLine, i));
            }

            List<string>[] allLinesArray = await Task.WhenAll(tasks);

            // Merge all lists sequentially
            List<string> allLines = new List<string>();
            foreach (var linesList in allLinesArray)
            {
                allLines.AddRange(linesList);
            }
            #endregion

            // Stop the stopwatch when the code is done
            stopwatch.Stop();

            // Get the elapsed time
            TimeSpan elapsedTime = stopwatch.Elapsed;

            FileReaderUtil.ShowConsoleWriteLine($"End to Read All of File, numberOfTasks = {numberOfTasks}, ElaseTime = {elapsedTime}");
        }

        async Task<List<string>> ReadLinesAsync(string filePath, int startLine, int endLine, int taskIndex)
        {
            List<string> retReadLines = new List<string>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                long byteOffset = FileReaderUtil.CalculateByteOffset(reader, startLine);
                reader.BaseStream.Seek(byteOffset, SeekOrigin.Begin);

                for (var i = startLine; i < endLine; i++)
                {
                    string line = await reader.ReadLineAsync();
                    retReadLines.Add(line);
                    if (i % printLineCount == 0)
                    {
                        // ShowConsoleWriteLine($"Task:: num of lines = {i}, Task ID = {taskIndex}");
                    }
                }
            }
            return retReadLines;
        }

        async Task<List<string>> ReadLinesForwardAsync(string filePath)
        {
            List<string> retReadLines = new List<string>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                for (var i = 0; i < allOfLines / 2; i++)
                {
                    // i : current file lines
                    string line = await reader.ReadLineAsync();
                    retReadLines.Add(line);
                    if (i % printLineCount == 0)
                    {
                        FileReaderUtil.ShowConsoleWriteLine("Forward Task:: num of lines = " + i);
                    }
                }
            }
            return retReadLines;
        }

        async Task<List<string>> ReadLinesBackwardAsync(string filePath)
        {
            List<string> retReadLines = new List<string>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                // i : current file lines
                for (var i = allOfLines / 2; i < allOfLines; i++)
                {
                    string line = await reader.ReadLineAsync();
                    retReadLines.Add(line);
                    if (i % printLineCount == 0)
                    {
                        FileReaderUtil.ShowConsoleWriteLine("Backward Task:: num of lines = " + i);
                    }
                }
            }
            return retReadLines;
        }
    }

    public class FileReaderThread
    {
        long allOfLines;
        int printLineCount = 50000;
        
        // Shared lists to store lines
        static List<string> forwardLines = new List<string>();
        static List<string> backwardLines = new List<string>();


        public void ReadFileWithMultipleThread(int numberOfThread)
        {
            forwardLines.Clear();
            backwardLines.Clear();

            // Create a Stopwatch instance
            Stopwatch stopwatch = new Stopwatch();

            // Start the stopwatch to measure elapsed time
            stopwatch.Start();

            string filePath = "5G_KT_QPSK_273RB_wfm.wf"; // Replace with the actual file path

            allOfLines = File.ReadLines(filePath).LongCount();

            bool bReadData = false;

            Thread forwardThread = new Thread(() => ReadLinesForward(filePath, bReadData));
            Thread backwardThread = new Thread(() => ReadLinesBackward(filePath));

            forwardThread.Start();
            backwardThread.Start();

            forwardThread.Join();
            backwardThread.Join();

            // Combine the lines from both threads
            List<string> allLines = new List<string>(forwardLines);
            allLines.AddRange(backwardLines);

            // Stop the stopwatch when the code is done
            stopwatch.Stop();

            // Get the elapsed time
            TimeSpan elapsedTime = stopwatch.Elapsed;

            FileReaderUtil.ShowConsoleWriteLine($"End to Read All of File, numberOfThread = {numberOfThread}, ElaseTime = {elapsedTime}");
        }

        private void ReadLinesForward(string filePath, bool isReadData)
        {
            List<WaveFormItem> WaveFormData = new List<WaveFormItem>();

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                bool isDataField = false;
                int lineNum = 0;
                string strLine = string.Empty;
                while ((strLine = reader.ReadLine()) != null)
                {
                    // isWriteLog = lineNum % 100000 == 10;

                    // [22. 10. 26 LWY] :: 중지 요청 시 파일읽기 강제중지
                  

                    // 한 줄씩 읽을 때 마다 라인 수 증가
                    lineNum++;

                    var bReadData = true;
                    isDataField = true;

                    // Header를 먼저 파싱
                    if (!isDataField)
                    {
                    }

                    // 여부 확인 후 Data 파싱
                    if (isDataField)
                    {
                        // Header만 읽도록 하는 경우 반복이탈
                        if (!bReadData)
                        {
                            break;
                        }

                        ReadFileData(strLine, lineNum, ref WaveFormData);
                    }
                } // end while ((strLine = reader.ReadLine()) != null)   ////////

                // Header에 LineLength를 파싱하지 않았다면, 최종 line 수가 전체 waveform의 length임

                // 22. 05. 31 :: 파일 모두 읽은 후 닫기 처리 추가, 계속 점유하는 문제 발생함
                reader.Close();
                reader.Dispose();
            }
        }

        private void ReadLinesBackward(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                long byteOffset = FileReaderUtil.CalculateByteOffset(reader, (int)(allOfLines / 2 - 1));
                reader.BaseStream.Seek(byteOffset, SeekOrigin.Begin);

                // Read lines backward
                for (var i = allOfLines / 2; i < allOfLines; i++)
                {
                    string line = reader.ReadLine();

                    if (i % printLineCount == 0)
                    {
                        //FileReaderUtil.ShowConsoleWriteLine("Backward Thread:: num of lines = " + i);
                    }

                    // 일부 비정상적으로 의도한 데이터 이상의 길이로 읽혀지는 경우 있음
                    string errorData = line.Split(' ')[0].Trim();
                    int DataPartLength = 8;
                    if (errorData.Length > DataPartLength)
                    {
                        line = line.Remove(DataPartLength, errorData.Length - DataPartLength);
                    }

                    if (i >= allOfLines / 2 && i <= allOfLines / 2 + 3)
                    {
                        FileReaderUtil.ShowConsoleWriteLine("Backward Thread:: first line = " + line);
                    }

                    // Use lock to synchronize access to the shared list
                    lock (backwardLines)
                    {
                        backwardLines.Add(line);
                    }
                }
            }
        }


        public void ReadFileData(string strLine, int lineNum, ref List<WaveFormItem> list)
        {
            try
            {
                WaveFormItem item = new WaveFormItem();
                string[] splitedStr = strLine.Split(' ');

                // Marker 여부 확인하고, 있을 시 marker 값 저장하기
                if (splitedStr.Length == 2)
                {

                }
                else if (splitedStr.Length == 3)
                {
                    item.Marker1 = int.Parse(splitedStr[1]);
                    item.Marker2 = int.Parse(splitedStr[2]);
                }

                // 그냥 hex 값 앞 네자리 뒷 네자리 파싱하는 방식
                string strHexaIQ = splitedStr[0].Trim().PadLeft(8, '0');

                // 앞 4자리, 뒤 4자리 구분하기
                string str_I = strHexaIQ.Substring(0, 4);
                string str_Q = strHexaIQ.Substring(4, 4);

                // hex -> dec
                var value_I = Convert.ToInt16(str_I, 16);
                var value_Q = Convert.ToInt16(str_Q, 16);

                // -4000 이하일 때 로그 찍어보기(주석)
                //if ( value_I < -4000 || value_Q < -4000)
                //{
                //    string strLog = string.Format("line num = {2, 6}, I = {0, 5}, Q = {1, 5}", value_I, value_Q, lineNum);
                //    Console.WriteLine(strLog);
                //}

                item.IData = value_I;
                item.QData = value_Q;

                // 23. 02. 10 LWY :: MAX Count 지정
                list.Add(item);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
        }

    }

    public class WaveFormItem
    {
        public int IData { get; set; }

        public int QData { get; set; }

        public int? Marker1 { get; set; } = null;

        public int? Marker2 { get; set; } = null;
    }

    public class FileReaderMethodTest
    {
        // Reading the entire file into a single string using a StreamReader ReadToEnd() method
        public static void T1(string fileName)
        {
            char[] Separator = "\r\n".ToCharArray();
            
            using (StreamReader sr = File.OpenText(fileName))
            {
                string s = sr.ReadToEnd();

                string[] split_s = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                //you then have to process the string
            }
        }

        // Reading the entire file into a single StringBuilder object using a StreamReader ReadToEnd() method
        public static void T2(string fileName)
        {
            using (StreamReader sr = File.OpenText(fileName))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(sr.ReadToEnd());
                //you then have to process the string
            }
        }

        // Reading each line into a string using StreamReader ReadLine() method
        public static void T3(string fileName)
        {
            using (StreamReader sr = File.OpenText(fileName))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    //we're just testing read speeds
                }
            }
        }

        // Reading each line into a string using a BufferedStream
        public static void T4(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    //we're just testing read speeds    
                }
            }
        }

        // Reading each line into a string using a BufferedStream with a preset buffer size equal to the size of the biggest line
        public static void T5(string fileName)
        {
            char[] g = new char[1024];
            using (FileStream fs = File.Open(fileName, FileMode.Open ))
            using (BufferedStream bs = new BufferedStream(fs,
                System.Text.ASCIIEncoding.Unicode.GetByteCount(g)))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    //we're just testing read speeds
                }
            }
        }

        // Reading each line into a StringBuilder object using StreamReader ReadLine() method
        public static void T6(string fileName)
        {
            using (StreamReader sr = File.OpenText(fileName))
            {
                StringBuilder sb = new StringBuilder();
                while (sb.Append(sr.ReadLine()).Length > 0)
                {
                    //we're just testing read speeds
                    sb.Clear();
                }
            }
        }

        // Reading each line into a StringBuilder object with its size preset and equal to the size of the biggest line
        public static void T7(string fileName)
        {
            char[] g = new char[1024];
            using (StreamReader sr = File.OpenText(fileName))
            {
                StringBuilder sb = new StringBuilder(g.Length);
                while (sb.Append(sr.ReadLine()).Length > 0)
                {
                    //we're just testing read speeds
                    sb.Clear();
                }
            }
        }

        // Reading each line into a pre-allocated string array object
        public static void T8(string fileName)
        {
            int MAX = 10000000;
            string[] AllLines = new string[MAX];
            using (StreamReader sr = File.OpenText(fileName))
            {
                int x = 0;
                while (!sr.EndOfStream)
                {
                    //we're just testing read speeds
                    AllLines[x] = sr.ReadLine();
                    x += 1;
                }
            }
        }

        // Reading the entire file into a string array object using the .Net ReadAllLines() method.
        public static void T9(string fileName)
        {
            int MAX = 10000000;
            var AllLines = new string[MAX];
            AllLines = File.ReadAllLines(fileName);
        }
    }
}
