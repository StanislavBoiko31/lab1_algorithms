using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab_1_algorithms
{
    public static class modified_sort
    {
        public static void SortInFile(int num_series)
        {

            if (num_series <= 0)
            {
                throw new ArgumentException("Кількість серій повинна бути більше нуля", nameof(num_series));
            }

            string inputFile = "input.txt";
            string outputFile = "sorted_series.txt";

            long totalNumbers = CountNumbers(inputFile);

            long baseSeriesSize = totalNumbers / num_series;
            long remainingNumbers = totalNumbers % num_series;

            using (StreamReader reader = new StreamReader(inputFile))
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                for (int i = 0; i < num_series; i++)
                {
                    long currentSeriesSize = baseSeriesSize + (i < remainingNumbers ? 1 : 0);
                    SortAndWriteSeries(reader, writer, currentSeriesSize);
                }
            }
            File.Delete(inputFile);
            File.Move(outputFile, inputFile);
        }

        public static void SortAndWriteSeries(StreamReader reader, StreamWriter writer, long seriesSize)
        {
            List<long> series = new List<long>();

            for (long i = 0; i < seriesSize; i++)
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    series.Add(long.Parse(line));
                }
            }
            series.Sort();

            foreach (long number in series)
            {
                writer.WriteLine(number);
            }
        }

        public static long CountNumbers(string fileName)
        {
            return File.ReadLines(fileName).LongCount();
        }
    }

}
