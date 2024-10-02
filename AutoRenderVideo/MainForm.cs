using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;
using static System.Net.WebRequestMethods;

namespace AutoRenderVideo
{
    public partial class MainForm : Form
    {
        private bool _running = false;
        private static Random rnd = new Random();

        public MainForm()
        {
            InitializeComponent();
        }

        private void txtAmNen_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FolderAmNen = txtAmNen.Text;
            Properties.Settings.Default.Save();
        }

        private void txtSoundNhac_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FolderSoundNhac = txtSoundNhac.Text;
            Properties.Settings.Default.Save();
        }

        private void txtHinhAnh_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FolderHinhAnh = txtHinhAnh.Text;
            Properties.Settings.Default.Save();

        }

        private void txtSoLuongToiDa_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SoLuongToiDa = txtSoLuongToiDa.Text;
            Properties.Settings.Default.Save();

        }

        private void txtSoLuongToiDa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            txtAmNen.Text = Properties.Settings.Default.FolderAmNen;
            txtSoundNhac.Text = Properties.Settings.Default.FolderSoundNhac;
            txtHinhAnh.Text = Properties.Settings.Default.FolderHinhAnh;
            txtSoLuongToiDa.Text = Properties.Settings.Default.SoLuongToiDa;
            txtSoPhutDauRa.Text = Properties.Settings.Default.SoPhutDauRa;
            txtSoLuongFileRender.Text = Properties.Settings.Default.SoLuongFileRender;
        }

        private void btnChonAmNen_Click(object sender, EventArgs e)
        {
            using (var selectFolder = new FolderBrowserDialog())
            {
                if (selectFolder.ShowDialog() == DialogResult.OK)
                {
                    txtAmNen.Text = selectFolder.SelectedPath;
                }
            }
        }

        private void btnChonSoundNhac_Click(object sender, EventArgs e)
        {
            using (var selectFolder = new FolderBrowserDialog())
            {
                if (selectFolder.ShowDialog() == DialogResult.OK)
                {
                    txtSoundNhac.Text = selectFolder.SelectedPath;
                }
            }
        }

        private void btnChonHinhAnh_Click(object sender, EventArgs e)
        {
            using (var selectFolder = new FolderBrowserDialog())
            {
                if (selectFolder.ShowDialog() == DialogResult.OK)
                {
                    txtHinhAnh.Text = selectFolder.SelectedPath;
                }
            }
        }


        private string CreateAudio(string saveFolder)
        {
            string folderAmNen = Properties.Settings.Default.FolderAmNen;
            string folderSoundNhac = Properties.Settings.Default.FolderSoundNhac;

            var amNenFiles = Directory.GetFiles(folderAmNen, "*.mp3").ToList();
            var soundNhacFiles = Directory.GetFiles(folderSoundNhac, "*.wav").ToList();

            if (amNenFiles.Count == 0 || soundNhacFiles.Count == 0)
            {
                MessageBox.Show($"Không có file trong folder\n{folderAmNen}\nhoặc\n{folderSoundNhac}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            string firstAudioName = "firstAudio.wav";
            using (var outputWaveFile = new WaveFileWriter(firstAudioName, new WaveFormat(48000, 32, 2)))
            {
                using (var mp3Reader = new Mp3FileReader(amNenFiles[rnd.Next(amNenFiles.Count)]))
                {
                    mp3Reader.CopyTo(outputWaveFile);
                }
            }

            var firstAudioWave = new AudioFileReader(firstAudioName);

            List<AudioFileReader> audioFiles = new List<AudioFileReader>();
            audioFiles.Add(firstAudioWave);

            TimeSpan totalTime = firstAudioWave.TotalTime;

            while ((int)totalTime.TotalMinutes < int.Parse(Properties.Settings.Default.SoPhutDauRa))
            {
                soundNhacFiles.Shuffle();
                foreach (var soundNhacFile in soundNhacFiles)
                {
                    AudioFileReader audioFileReader = new AudioFileReader(soundNhacFile);
                    totalTime += audioFileReader.TotalTime;
                    audioFiles.Add(audioFileReader);
                    if ((int)totalTime.TotalMinutes >= int.Parse(Properties.Settings.Default.SoPhutDauRa))
                    {
                        break;
                    }
                }
            }

            var playlist = new ConcatenatingSampleProvider(audioFiles);

            string result = Path.Combine(saveFolder, "audio.wav");
            WaveFileWriter.CreateWaveFile16(result, playlist);

            firstAudioWave.Dispose();
            foreach (var audioFile in audioFiles)
            {
                audioFile.Dispose();
            }

            System.IO.File.Delete(firstAudioName);
            return result;
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                System.IO.File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (Directory.Exists("data") == false)
            {
                Directory.CreateDirectory("data");
            }
            if (Directory.Exists("temp") == false)
            {
                Directory.CreateDirectory("temp");
            }
            if (Directory.Exists("OUTPUT") == false)
            {
                Directory.CreateDirectory("OUTPUT");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.FolderAmNen)
               || string.IsNullOrEmpty(Properties.Settings.Default.FolderSoundNhac)
               || string.IsNullOrEmpty(Properties.Settings.Default.FolderHinhAnh)
               || string.IsNullOrEmpty(Properties.Settings.Default.SoLuongToiDa))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folderHinhAnh = Properties.Settings.Default.FolderHinhAnh;
            var hinhAnhFiles = Directory.GetFiles(folderHinhAnh, "*.mp4").ToList();
            if (hinhAnhFiles.Count == 0)
            {
                MessageBox.Show($"Không có file trong folder\n{folderHinhAnh}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _running = true;
            EnableControl(false);

            Dictionary<string, DataModel> dicConvertHinhAnhs = await ProcessConvertCodecVideo(false);

            for (int index = 0; index < int.Parse(Properties.Settings.Default.SoLuongFileRender) && _running; index++)
            {
                if (!_running) break;
                int currentIndex = index;
                string folderSave = @"temp\" + currentIndex;
                if (Directory.Exists(folderSave) == false)
                {
                    Directory.CreateDirectory(folderSave);
                }

                ClearFolder(folderSave);

                TimeSpan totalDuration = TimeSpan.Zero;

                string musicPath = CreateAudio(folderSave);
                //string musicPath = @"temp\0\audio.wav";
                if (!_running) break;
                TimeSpan wavDuration = (await FFmpeg.GetMediaInfo(musicPath)).Duration;

                List<string> videoList = new List<string>();
                string randomVide0 = hinhAnhFiles[rnd.Next(hinhAnhFiles.Count)];
                while (totalDuration.TotalSeconds < (wavDuration.TotalSeconds * 1.5))
                {
                    videoList.Add(dicConvertHinhAnhs[randomVide0].Converted);

                    totalDuration += dicConvertHinhAnhs[randomVide0].Total;

                    if (totalDuration.TotalSeconds >= (wavDuration.TotalSeconds * 1.5))
                        break;
                }
                if (!_running) break;

                string tempFilePath = Path.Combine(folderSave, "temp_video_list.txt");
                System.IO.File.WriteAllLines(tempFilePath, videoList.Select(v => $"file '{v}'"));

                string mergeVideo = Path.Combine(folderSave, @"output.mp4");
                AddLog($"Luồng {currentIndex + 1} tạo file transition");
                await MergeVideosWithTransition(currentIndex + 1, videoList, mergeVideo);
                if (!_running) break;

                string trimVideo = Path.Combine(folderSave, @"trim.mp4");
                AddLog($"Luồng {currentIndex + 1} cắt video");
                await TrimVideos(currentIndex + 1, mergeVideo, trimVideo, wavDuration);
                if (!_running) break;

                string demoVideo = Path.Combine("OUTPUT", "Video_" + DateTime.Now.ToString("mmssfff") + ".mp4");
                AddLog($"Luồng {currentIndex + 1} ghép nhạc");
                await MergeAudio(currentIndex + 1, trimVideo, musicPath, demoVideo);
                if (!_running) break;

                System.IO.File.Delete(mergeVideo);
                System.IO.File.Delete(trimVideo);
                System.IO.File.Delete(musicPath);
            }

            EnableControl(true);
        }

        private void txtSoPhutDauRa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtSoPhutDauRa_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SoPhutDauRa = txtSoPhutDauRa.Text;
            Properties.Settings.Default.Save();
        }

        private void AddLog(string log)
        {
            rtbLog.Invoke(new Action(() =>
            {
                if (rtbLog.Lines.Length > 1000)
                {
                    rtbLog.Clear();
                }
                rtbLog.Text += log + Environment.NewLine;
            }));
        }


        private async Task<string> ConvertVideo(string inputPath, string outputPath)
        {
            try
            {
                var arg = $"-i \"{inputPath}\" -vf scale=1920:1080 -r 30 -c:v libx265 -b:v 1000k \"{outputPath}\"";
                var conversion = FFmpeg.Conversions.New()
                    .AddParameter(arg);

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Đang convert file {Path.GetFileName(inputPath)}: {args.Percent}%");
                };

                await conversion.Start();
                return outputPath;
            }
            catch
            {

            }
            return string.Empty;
        }

        async Task MergeVideosWithTransition(int index, List<string> videoFiles, string output)
        {
            try
            {
                var transitions = new List<string>();
                double preDuration = 0;
                for (int i = 0; i < videoFiles.Count - 1; i++)
                {
                    var mediaInfo = await FFmpeg.GetMediaInfo(videoFiles[i]);


                    double randomDuration = rnd.Next(3, 5);

                    double transitionOffset = mediaInfo.Duration.Seconds + preDuration - randomDuration;

                    if (i == 0)
                    {
                        transitions.Add($"[0][{i + 1}]xfade=transition=fade:duration={randomDuration}:offset={transitionOffset}[V{i}];");
                    }
                    else if (i == (videoFiles.Count - 2))
                    {
                        transitions.Add($"[V{i - 1}][{i + 1}]xfade=transition=fade:duration={randomDuration}:offset={transitionOffset}[video];");
                    }
                    else
                    {
                        transitions.Add($"[V{i - 1}][{i + 1}]xfade=transition=fade:duration={randomDuration}:offset={transitionOffset}[V{i}];");
                    }
                    preDuration = transitionOffset;
                }

                var command = FFmpeg.Conversions.New();

                foreach (var video in videoFiles)
                {
                    command.AddParameter($"-i \"{video}\"");
                }

                command.AddParameter($"-filter_complex \"{string.Join("", transitions)}\"");

                command.AddParameter($"-map \"[video]\" -preset ultrafast \"{output}\"");

                command.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Luồng {index} tạo file transition: {args.Percent}%");
                };

                await command.Start();
            }
            catch
            {

            }
        }

        private async Task TrimVideos(int index, string inputFilePath, string outputFilePath, TimeSpan timeSpan)
        {
            try
            {
                int second = (int)timeSpan.TotalSeconds;
                var conversion = FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{inputFilePath}\" -t {second} -c copy \"{outputFilePath}\"");

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Luồng {index} cắt video: {args.Percent}%");
                };

                await conversion.Start();
            }
            catch
            {

            }
        }

        private async Task MergeAudio(int index, string inputFilePathVideo, string inputFilePathAudio, string outputFilePath)
        {
            try
            {
                var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputFilePathVideo}\" -i \"{inputFilePathAudio}\" -vf scale=1920:1080 -r 30 -c:v libx265 -b:v 1000k \"{outputFilePath}\" ");

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Luồng {index} ghép nhạc: {args.Percent}%");
                };

                await conversion.Start();
            }
            catch
            {

            }
        }

        private void EnableControl(bool enable)
        {
            txtAmNen.Enabled = enable;
            txtSoundNhac.Enabled = enable;
            txtHinhAnh.Enabled = enable;
            btnChonAmNen.Enabled = enable;
            btnChonHinhAnh.Enabled = enable;
            btnChonSoundNhac.Enabled = enable;
            txtSoLuongToiDa.Enabled = enable;
            txtSoPhutDauRa.Enabled = enable;
            btnConvertCodecVideo.Enabled = enable;
            btnStart.Enabled = enable;
            //btnStop.Enabled = enable;
            txtSoLuongFileRender.Enabled = enable;
        }


        private async void btnConvertCodecVideo_Click(object sender, EventArgs e)
        {
            EnableControl(false);

            await ProcessConvertCodecVideo(true);

            EnableControl(true);
        }

        private async Task<Dictionary<string, DataModel>> ProcessConvertCodecVideo(bool deleteFileInDataFolder)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.FolderHinhAnh))
            {
                MessageBox.Show("Vui lòng nhập folder hình ảnh", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            string folderHinhAnh = Properties.Settings.Default.FolderHinhAnh;
            var hinhAnhFiles = Directory.GetFiles(folderHinhAnh, "*.mp4").ToList();
            if (hinhAnhFiles.Count == 0)
            {
                MessageBox.Show($"Không có file trong folder\n{folderHinhAnh}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (Directory.Exists("data") == false)
            {
                Directory.CreateDirectory("data");
            }
            if (deleteFileInDataFolder)
            {
                ClearFolder("data");
            }

            Dictionary<string, DataModel> dicConvertHinhAnhs = new Dictionary<string, DataModel>();

            foreach (var hinhAnhFile in hinhAnhFiles)
            {
                string convertedVideo = await ConvertVideo(hinhAnhFile, $"data\\{Path.GetFileName(hinhAnhFile)}");
                var fileDuration = (await FFmpeg.GetMediaInfo(convertedVideo)).Duration;
                dicConvertHinhAnhs[hinhAnhFile] = new DataModel() { Converted = convertedVideo, Total = fileDuration };
            }
            AddLog($"Convert file xong");
            return dicConvertHinhAnhs;
        }


        private void ClearFolder(string folder)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(folder);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void rtbLog_TextChanged(object sender, EventArgs e)
        {
            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.ScrollToCaret();
        }

        private void txtSoLuongFileRender_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtSoLuongFileRender_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SoLuongFileRender = txtSoLuongFileRender.Text;
            Properties.Settings.Default.Save();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _running = false;
            CloseAllFFMPEG();
        }

        private void CloseAllFFMPEG()
        {
            try
            {
                Process[] arrayProcesses = Process.GetProcessesByName("ffmpeg");
                if (arrayProcesses != null && arrayProcesses.Length > 0)
                {
                    foreach (var process in arrayProcesses)
                    {
                        process.Kill();
                    }
                }
            }
            catch
            {

            }
        }

    }
}
