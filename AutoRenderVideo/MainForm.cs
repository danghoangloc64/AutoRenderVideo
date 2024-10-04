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
            if (DateTime.Now > new DateTime(2024, 10, 7))
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

            Properties.Settings.Default.SoLuongToiDa = "1";
            Properties.Settings.Default.Save();

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

                        var inputInfo = (await FFmpeg.GetMediaInfo(randomVide0)).Duration;

                        while (inputInfo.TotalSeconds <= 6)
                        {
                            randomVide0 = hinhAnhFiles[rnd.Next(hinhAnhFiles.Count)];
                            inputInfo = (await FFmpeg.GetMediaInfo(randomVide0)).Duration;
                        }


                        DataModel inputVideo = await ProcessConvertCodecVideo(randomVide0);

                        ClearFolder(folderSave);

                        if (!_running) return;



                        Check();

                        string videoWithTransition = Path.Combine("data", $"{Path.GetFileNameWithoutExtension(inputVideo.File)}" + "_videoWithTransition.mp4");
                        AddLog($"Video thứ {currentIndex + 1} tạo file transition: bắt đầu");
                        await MergeVideosWithTransition(currentIndex + 1, inputVideo.File, videoWithTransition);
                        AddLog($"Video thứ {currentIndex + 1} tạo file transition: xong");

                        if (!_running) return;

                        TimeSpan videoWithTransitionLength = (await FFmpeg.GetMediaInfo(videoWithTransition)).Duration;
                        AddLog($"Video thứ {currentIndex + 1} xử lý file transition: bắt đầu");
                        string part1 = Path.Combine("data", $"{Path.GetFileNameWithoutExtension(inputVideo.File)}" + "_01.mp4");
                        string part2 = Path.Combine("data", $"{Path.GetFileNameWithoutExtension(inputVideo.File)}" + "_02.mp4");
                        string part3 = Path.Combine("data", $"{Path.GetFileNameWithoutExtension(inputVideo.File)}" + "_03.mp4");

                        int duration = 3;                   

                        await TrimVideo(currentIndex + 1, videoWithTransition, part1, 0, (int)(inputVideo.Duration.TotalSeconds - duration));
                        await TrimVideo(currentIndex + 1, videoWithTransition, part2, (int)(inputVideo.Duration.TotalSeconds - duration), (int)(inputVideo.Duration.TotalSeconds));
                        await TrimVideo(currentIndex + 1, videoWithTransition, part3, (int)(inputVideo.Duration.TotalSeconds), (int)(videoWithTransitionLength.TotalSeconds) - duration);


                        AddLog($"Video thứ {currentIndex + 1} xử lý file transition: xong");

                        List<string> videoList = new List<string>();
                        videoList.Add(part1);

                        string musicPath = CreateAudio(folderSave);
                        TimeSpan musicDuration = (await FFmpeg.GetMediaInfo(musicPath)).Duration;

                        TimeSpan part1Duration = (await FFmpeg.GetMediaInfo(part1)).Duration;
                        TimeSpan part2Duration = (await FFmpeg.GetMediaInfo(part2)).Duration;
                        TimeSpan part3Duration = (await FFmpeg.GetMediaInfo(part3)).Duration;

                        TimeSpan mainDuration = part1Duration;
                        while (mainDuration.TotalSeconds < (musicDuration.TotalSeconds))
                        {
                            videoList.Add(part2);
                            mainDuration += part2Duration;
                            if (mainDuration.TotalSeconds >= (musicDuration.TotalSeconds))
                            {
                                break;
                            }
                            videoList.Add(part3);
                            mainDuration += part3Duration;
                            if (mainDuration.TotalSeconds >= (musicDuration.TotalSeconds))
                            {
                                break;
                            }
                        }
                        if (!_running) return;

                        string tempFilePath = Path.Combine(folderSave, "temp_video_list.txt");
                        System.IO.File.WriteAllLines(tempFilePath, videoList.Select(v => $"file '..\\..\\{v}'"));

                        string concatVideoWithTransition = Path.Combine(folderSave, @"concatVideoWithTransition.mp4");

                        AddLog($"Video thứ {currentIndex + 1} xử lý nối video: bắt đầu");

                        await ConcatVideo(currentIndex + 1, tempFilePath, concatVideoWithTransition);

                        string trimVideo = Path.Combine(folderSave, @"trimVideo.mp4");
                        await TrimVideos(currentIndex + 1, concatVideoWithTransition, trimVideo, musicDuration);
                        AddLog($"Video thứ {currentIndex + 1} đang xử lý nối video: xong");


                        Check();
                        string outputVideo = Path.Combine("OUTPUT", "Video_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".mp4");
                        AddLog($"Video thứ {currentIndex + 1} ghép nhạc: bắt đầu");
                        await MergeAudio(currentIndex + 1, trimVideo, musicPath, outputVideo);
                        AddLog($"Video thứ {currentIndex + 1} ghép nhạc: xong");
                        AddLog($"Video thứ {currentIndex + 1} đã tạo xong");
                        if (!_running) return;

                        Check();
                        System.IO.File.Delete(concatVideoWithTransition);
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

        private async Task TrimVideos(int index, string inputFilePath, string outputFilePath, TimeSpan timeSpan)
        {
            try
            {
                int second = (int)timeSpan.TotalSeconds;
                var conversion = FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{inputFilePath}\" -t {second} -c copy \"{outputFilePath}\"");

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Video thứ {index} đang xử lý nối video");
                };

                await conversion.Start();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ConcatVideo(int index, string videoListFile, string concatVideoWithTransition)
        {
            try
            {
                var arg = $"-safe 0 -f concat -i \"{videoListFile}\" -c copy \"{concatVideoWithTransition}\"";
                var conversion = FFmpeg.Conversions.New()
                    .AddParameter(arg);

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Video thứ {index} đang xử lý nối video");
                };

                await conversion.Start();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task TrimVideo(int index, string input, string output, int from, int to)
        {
            try
            {
                var arg = $"-i \"{input}\" -ss {from} -to {to} -vf scale=1920:1080 -r 25 -c:v libx265 -b:v 2500k -an \"{output}\"";


                //var arg = $"-ss {from} -i \"{input}\" -t {to} \"{output}\"";


                var conversion = FFmpeg.Conversions.New()
                    .AddParameter(arg);

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Video thứ {index} đang xử lý file transition");
                };

                await conversion.Start();
            }
            catch (Exception ex)
            {

            }
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
                rtbLog.Text += DateTime.Now.ToString("HH:mm:ss:fff dd/MM/yyyy") + ": " + log + Environment.NewLine;
            }));
        }

        private async Task<string> ConvertVideo(string inputPath, string outputPath)
        {
            try
            {
                var arg = $"-i \"{inputPath}\" -vf scale=1920:1080 -r 25 -c:v libx265 -b:v 2500k -an \"{outputPath}\"";
                var conversion = FFmpeg.Conversions.New()
                    .AddParameter(arg);

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Đang convert file {Path.GetFileName(inputPath)}");
                };

                await conversion.Start();
                return outputPath;
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }

        async Task MergeVideosWithTransition(int index, string videoFile, string output)
        {
            try
            {
                double duration = 3;
                var mediaInfo = await FFmpeg.GetMediaInfo(videoFile);

                double transitionOffset = mediaInfo.Duration.TotalSeconds - duration;
                var command = FFmpeg.Conversions.New();
                command.AddParameter($"-i \"{videoFile}\"");
                command.AddParameter($"-i \"{videoFile}\"");
                command.AddParameter($"-filter_complex \"[0:v]scale=1920:1080[v1];[1:v]scale=1920:1080[v2];[v1][v2]xfade=transition=fade:duration={duration}:offset={transitionOffset}[video];\"");
                command.AddParameter($"-map \"[video]\" \"{output}\"");

                command.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Video thứ {index} đang tạo file transition");
                };

                await command.Start();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task MergeAudio(int index, string inputFilePathVideo, string inputFilePathAudio, string outputFilePath)
        {
            try
            {
                var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputFilePathVideo}\" -i \"{inputFilePathAudio}\" -c copy \"{outputFilePath}\" ");

                conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
                {
                    AddLog($"Video thứ {index} đang ghép nhạc");
                };

                await conversion.Start();
            }
            catch (Exception ex)
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
                File = output,
                Duration = TimeSpan.Zero,
            };

            string convertedVideo = await ConvertVideo(inputFile, output);
            dataModel.Duration = (await FFmpeg.GetMediaInfo(convertedVideo)).Duration;

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
            catch (Exception ex)
            {

            }
        }

    }
}
