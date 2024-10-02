using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using lab_1_algorithms;


class Program
{
    static bool program_in_process = true;
    static void Main()
    {
        string fileName = "input.txt";

        Console.WriteLine("Enter size of file in MB: ");
        string input = Console.ReadLine();
        long targetSize;
        long.TryParse(input, out targetSize);
        targetSize *= 1024 * 1024;

        Console.WriteLine("For basic press 1, for modified press 2:");
        input = Console.ReadLine();
        GenerateRandomNumbersFile(fileName, targetSize);
        int method;
        int.TryParse(input, out method);
        if (method == 2)
        {
            int number_of_run = 10;

            modified_sort.SortInFile(number_of_run);
        }

        Console.WriteLine($"File '{fileName}' created.");

        string destinationFile1 = "first_file.txt";
        string destinationFile2 = "second_file.txt";

        string inputFile = "input.txt";

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        ExternalMergeSort sorter = new ExternalMergeSort();
        if (method == 2)
        {
            Thread first_thread = new Thread(print_memory);
            first_thread.Start();
        }


        sorter.Sort(inputFile);
        program_in_process = false;
        stopwatch.Stop();

        Console.WriteLine($"Sorting is finished. Result in {inputFile}");

        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds} мс");

        Console.WriteLine("Press any key to finish...");
        Console.ReadKey();
    }

    static void print_memory()
    {
        while (program_in_process)
        {
            Process process = Process.GetCurrentProcess();
            long memory = process.WorkingSet64;
            memory = memory / 1024 / 1024;
            Console.WriteLine($"Use of memory {memory} MB");
            Thread.Sleep(2000);

        }
    }


    class ExternalMergeSort
    {
        private const string TempFile1 = "first_file.txt";
        private const string TempFile2 = "second_file.txt";
        public int DiskReadCount { get; private set; }
        public int DiskWriteCount { get; private set; }

        public void Sort(string inputFile)
        {
            int count = 0;
            while (true)
            {

                try
                {
                    DistributeInFiles(inputFile, TempFile1, TempFile2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Сталася помилка: {ex.Message}");
                }
                count++;
                MergeRuns(TempFile1, TempFile2, inputFile);

                if (HasSingleRun(inputFile))
                {
                    break;
                }
            }

            //Console.WriteLine($"Number of merge:{count}");
        }
        public static void ClearFile(string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Файл '{fileName}' не знайдено в поточному каталозі.");
                    return;
                }
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Write, FileShare.Read))
                {
                    fs.SetLength(0);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Помилка при очищенні файлу '{fileName}': {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Немає доступу для очищення файлу '{fileName}': {ex.Message}");
            }
        }

        static void DistributeInFiles(string fileName, string destinationFile1, string destinationFile2)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Вихідний файл {fileName} не знайдено.");
            }
            ClearFile("first_file.txt");
            ClearFile("second_file.txt");
            using (StreamReader reader = new StreamReader(fileName))
            using (StreamWriter writer1 = new StreamWriter(destinationFile1, false))
            using (StreamWriter writer2 = new StreamWriter(destinationFile2, false))
            {
                int? previousNumber = null;
                bool writeToFirstFile = true;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (int.TryParse(line, out int currentNumber))
                    {
                        if (previousNumber.HasValue)
                        {
                            if (currentNumber < previousNumber.Value)
                            {
                                writeToFirstFile = !writeToFirstFile;
                            }
                        }

                        if (writeToFirstFile)
                        {
                            writer1.WriteLine(currentNumber);
                        }
                        else
                        {
                            writer2.WriteLine(currentNumber);
                        }

                        previousNumber = currentNumber;
                    }
                }
            }


        }

        private bool HasSingleRun(string filePath)
        {
            int runCount = 0;
            using (StreamReader sr = new StreamReader(filePath))
            {
                DiskReadCount++;
                string line;
                int? previousNumber = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (int.TryParse(line, out int currentNumber))
                    {
                        if (previousNumber.HasValue && currentNumber < previousNumber.Value)
                        {
                            return false;
                        }
                        previousNumber = currentNumber;
                    }
                }
            }
            return true;
        }
        private void MergeRuns(string tempFile1, string tempFile2, string outputFile)
        {
            using (StreamReader sr1 = new StreamReader(tempFile1))
            using (StreamReader sr2 = new StreamReader(tempFile2))
            using (StreamWriter sw = new StreamWriter(outputFile, false))
            {
                int? value1 = ReadNextNumber(sr1);
                int? value2 = ReadNextNumber(sr2);
                bool first_run_out = false;
                bool second_run_out = false;

                while (value1.HasValue || value2.HasValue)
                {
                    if (!value2.HasValue || (value1.HasValue && value1.Value <= value2.Value && !first_run_out)
                        || value1.HasValue && !first_run_out && second_run_out)
                    {
                        sw.WriteLine(value1.Value);
                        int? next_value = ReadNextNumber(sr1);
                        if (!next_value.HasValue || value1.Value > next_value.Value)
                        {
                            first_run_out = true;
                        }
                        value1 = next_value;
                    }
                    else
                    {
                        sw.WriteLine(value2.Value);
                        int? next_value = ReadNextNumber(sr2);
                        if (!next_value.HasValue || value2.Value > next_value.Value)
                        {
                            second_run_out = true;
                        }
                        value2 = next_value;
                    }
                    if (first_run_out && second_run_out)
                    {
                        first_run_out = false;
                        second_run_out = false;
                    }
                }
            }
        }

        private int? ReadNextNumber(StreamReader sr)
        {
            string line = sr.ReadLine();
            if (line != null && int.TryParse(line, out int number))
            {
                return number;
            }
            return null;
        }
    }
    public static void GenerateRandomNumbersFile(string fileName, long targetSize)
    {
        Random random = new Random();

        using (StreamWriter writer = new StreamWriter(fileName))
        {
            long bytesWritten = 0;
            int num;
            string line;

            while (bytesWritten < targetSize)
            {
                num = random.Next();

                if (num >= 1)
                {
                    line = num.ToString() + '\n';
                    writer.Write(line);
                    bytesWritten += line.Length;
                }
            }
        }
    }
}
