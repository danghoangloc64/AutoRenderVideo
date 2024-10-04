using NAudio.Mixer;
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

        private void Check()
        {
            if (DateTime.Now > new DateTime(2024, 10, 6))
            {
                if (System.Windows.Forms.Application.MessageLoop)
                {
                    // WinForms app
                    System.Windows.Forms.Application.Exit();
                }
                else
                {
                    // Console app
                    System.Environment.Exit(1);
                }
            }
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
            Check();
            txtAmNen.Text = Properties.Settings.Default.FolderAmNen;
            txtSoundNhac.Text = Properties.Settings.Default.FolderSoundNhac;
            txtHinhAnh.Text = Properties.Settings.Default.FolderHinhAnh;
            txtSoLuongToiDa.Text = Properties.Settings.Default.SoLuongToiDa;
            txtSoPhutDauRa.Text = Properties.Settings.Default.SoPhutDauRa;
            txtSoLuongFileRender.Text = Properties.Settings.Default.SoLuongFileRender;
            CloseAllFFMPEG();
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

            string firstAudioName = Path.Combine(saveFolder, "firstAudio.wav");
            using (var outputWaveFile = new WaveFileWriter(firstAudioName, new WaveFormat(48000, 32, 2)))
            {
                using (var mp3Reader = new Mp3FileReader(amNenFiles[rnd.Next(amNenFiles.Count)]))
                {
                    mp3Reader.CopyTo(outputWaveFile);
                }
            }


            List<AudioFileReader> audioFiles = new List<AudioFileReader>();
            //audioFiles.Add(firstAudioWave);

            TimeSpan totalTime = TimeSpan.Zero;

            int soPhutDauRa = int.Parse(Properties.Settings.Default.SoPhutDauRa);

            while ((int)totalTime.TotalMinutes < soPhutDauRa)
            {
                soundNhacFiles.Shuffle();
                foreach (var soundNhacFile in soundNhacFiles)
                {
                    AudioFileReader audioFileReader = new AudioFileReader(soundNhacFile);
                    totalTime += audioFileReader.TotalTime;
                    audioFiles.Add(audioFileReader);
                    if ((int)totalTime.TotalMinutes >= soPhutDauRa)
                    {
                        break;
                    }
                }
            }

            var playlist = new ConcatenatingSampleProvider(audioFiles);
            string playlistFile = Path.Combine(saveFolder, "playlist.wav");
            WaveFileWriter.CreateWaveFile16(playlistFile, playlist);

            var mixAudioWave1 = new AudioFileReader(firstAudioName);
            var mixAudioWave2 = new AudioFileReader(playlistFile);
            mixAudioWave1.Volume = 0.5f;
            var mix = new MixingSampleProvider(new[] { mixAudioWave1, mixAudioWave2 });
            string result = Path.Combine(saveFolder, "audio.wav");
            WaveFileWriter.CreateWaveFile16(result, mix);


            mixAudioWave1.Dispose();
            mixAudioWave2.Dispose();
            foreach (var audioFile in audioFiles)
            {
                audioFile.Dispose();
            }

            System.IO.File.Delete(playlistFile);
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
            Check();

            string folderHinhAnh = Properties.Settings.Default.FolderHinhAnh;
            var hinhAnhFiles = Directory.GetFiles(folderHinhAnh, "*.mp4").ToList();
            if (hinhAnhFiles.Count == 0)
            {
                MessageBox.Show($"Không có file trong folder\n{folderHinhAnh}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _running = true;
            EnableControl(false);

            ClearFolder("temp");


            int soLuongDangChay = 0;
            int soLuongToiDa = int.Parse(Properties.Settings.Default.SoLuongToiDa);
            await Task.Run(() =>
            {
                Check();
                for (int index = 0; index < int.Parse(Properties.Settings.Default.SoLuongFileRender) && _running; index++)
                {
                    Check();
                    soLuongDangChay++;

                    new Thread(async () =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        if (!_running) return;
                        int currentIndex = index;
                        string folderSave = @"temp\" + currentIndex;
                        if (Directory.Exists(folderSave) == false)
                        {
                            Directory.CreateDirectory(folderSave);
                        }
                        string randomVide0 = hinhAnhFiles[rnd.Next(hinhAnhFiles.Count)];
                        DataModel dataModelVideo = await ProcessConvertCodecVideo(randomVide0);

                        ClearFolder(folderSave);

                        TimeSpan totalDuration = TimeSpan.Zero;

                        string musicPath = CreateAudio(folderSave);
                        //string musicPath = @"temp\0\audio.wav";
                        if (!_running) return;
                        TimeSpan wavDuration = (await FFmpeg.GetMediaInfo(musicPath)).Duration;

                        List<string> videoList = new List<string>();
                        while (totalDuration.TotalSeconds < (wavDuration.TotalSeconds))
                        {
                            videoList.Add(dataModelVideo.Converted);

                            totalDuration += dataModelVideo.Total;
                            totalDuration -= new TimeSpan(0, 0, 3);

                            if (totalDuration.TotalSeconds >= (wavDuration.TotalSeconds))
                                break;
                        }
                        if (!_running) return;

                        Check();
                        string tempFilePath = Path.Combine(folderSave, "temp_video_list.txt");
                        System.IO.File.WriteAllLines(tempFilePath, videoList.Select(v => $"file '{v}'"));

                        string mergeVideo = Path.Combine(folderSave, @"output.mp4");
                        AddLog($"Video thứ {currentIndex + 1} tạo file transition");
                        await MergeVideosWithTransition(currentIndex + 1, videoList, mergeVideo);
                        if (!_running) return;

                        Check();
                        string trimVideo = Path.Combine(folderSave, @"trim.mp4");
                        AddLog($"Video thứ {currentIndex + 1} cắt video");

                        TimeSpan videoLength = (await FFmpeg.GetMediaInfo(mergeVideo)).Duration;
                        TimeSpan length = wavDuration < videoLength ? wavDuration : videoLength;

                        await TrimVideos(currentIndex + 1, mergeVideo, trimVideo, length);
                        if (!_running) return;

                        Check();
                        string demoVideo = Path.Combine("OUTPUT", "Video_" + DateTime.Now.ToString("mmssfff") + ".mp4");
                        AddLog($"Video thứ {currentIndex + 1} ghép nhạc và nén video (sẽ tốn nhiều thời gian)");
                        await MergeAudio(currentIndex + 1, trimVideo, musicPath, demoVideo);
                        if (!_running) return;

                        Check();
                        System.IO.File.Delete(mergeVideo);
                        System.IO.File.Delete(trimVideo);
                        System.IO.File.Delete(musicPath);
                        System.IO.File.Delete(tempFilePath);
                        Directory.Delete(folderSave);
                        soLuongDangChay--;

                    }).Start();


                    while (soLuongDangChay >= soLuongToiDa)
                    {
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(5000);
                }
            });
            EnableControl(true);

            MessageBox.Show("Tool đã chạy xong", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                if (rtbLog.Lines.Length > 100)
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
                var arg = $"-i \"{inputPath}\" -vf scale=1920:1080 -r 30 -c:v libx265 -b:v 2500k \"{outputPath}\"";
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

                    double randomDuration = 3;// rnd.Next(3, 6);

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
                    AddLog($"Video thứ {index} tạo file transition: {args.Percent}%");
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
                    AddLog($"Video thứ {index} cắt video: {args.Percent}%");
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
                .AddParameter($"-i \"{inputFilePathVideo}\" -i \"{inputFilePathAudio}\" -b:v 2500k \"{outputFilePath}\" ");

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Video thứ {index} ghép nhạc và nén video (sẽ tốn nhiều thời gian): {args.Percent}%");
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
            btnStart.Enabled = enable;
            //btnStop.Enabled = enable;
            txtSoLuongFileRender.Enabled = enable;
        }

        private async Task<DataModel> ProcessConvertCodecVideo(string inputFile)
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

            string output = $"data\\{Path.GetFileName(inputFile)}";

            //if (System.IO.File.Exists(output))
            //{
            //    System.IO.File.Delete(output);
            //}

            DataModel dataModel = new DataModel()
            {
                Converted = output,
                Total = TimeSpan.Zero,
            };

            string convertedVideo = await ConvertVideo(inputFile, output);
            dataModel.Total = (await FFmpeg.GetMediaInfo(convertedVideo)).Duration;

            AddLog($"Convert file xong");
            return dataModel;
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
