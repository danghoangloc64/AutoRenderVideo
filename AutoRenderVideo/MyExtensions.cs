using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRenderVideo
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }


    public static class WavFileUtils
    {
        public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                    int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                    startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                    int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                    endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                    int endPos = (int)reader.Length - endBytes;

                    TrimWavFile(reader, writer, startPos, endPos);
                }
            }
        }

        private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public static void CutWavFile(string inputFile, string outputFile, TimeSpan from, TimeSpan to)
        {
            using (var reader = new AudioFileReader(inputFile))
            {
                // Kiểm tra nếu thời gian kết thúc vượt quá thời lượng của file
                if (to > reader.TotalTime)
                    to = reader.TotalTime;

                // Tính toán số lượng mẫu cần đọc (samples)
                int sampleRate = reader.WaveFormat.SampleRate;
                int channels = reader.WaveFormat.Channels;
                int bytesPerSample = reader.WaveFormat.BitsPerSample / 8;

                // Tính toán vị trí byte
                long startPos = (long)(from.TotalSeconds * sampleRate * channels * bytesPerSample);
                long endPos = (long)(to.TotalSeconds * sampleRate * channels * bytesPerSample);

                // Di chuyển đến vị trí bắt đầu
                reader.Position = startPos;

                // Tạo file mới và ghi dữ liệu từ vị trí đã chọn
                using (var writer = new WaveFileWriter(outputFile, reader.WaveFormat))
                {
                    byte[] buffer = new byte[1024];
                    long bytesRemaining = endPos - startPos;
                    while (bytesRemaining > 0)
                    {
                        int bytesToRead = (int)Math.Min(buffer.Length, bytesRemaining);
                        int bytesRead = reader.Read(buffer, 0, bytesToRead);
                        if (bytesRead == 0)
                            break;
                        writer.Write(buffer, 0, bytesRead);
                        bytesRemaining -= bytesRead;
                    }
                }
            }
        }
    }
}
