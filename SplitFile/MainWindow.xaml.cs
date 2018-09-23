using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;

namespace SplitFile
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove(); //实现窗体移动
            }
        }

        private void OnMinBtnClicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnCloseBtnClicked(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown(0);
        }

        private void chooseFileToSplit(object sender, RoutedEventArgs e)
        {
            OpenFileDialog chooseFile = new OpenFileDialog();
            chooseFile.Multiselect = false;
            chooseFile.ShowDialog();
            if (chooseFile.FileName == "")
            {
                Message message = new Message("请选择文件");
                message.Show();
                return;
            }
            string filePath = chooseFile.FileName;
            fileToSplit.Text = filePath;
            string fileName = chooseFile.SafeFileName;
            newFileName.Text = "new - " + fileName;
        }

        private void chooseFileToSplitMerge(object sender, RoutedEventArgs e)
        {
            OpenFileDialog chooseFile = new OpenFileDialog();
            chooseFile.Multiselect = true;
            chooseFile.ShowDialog();
            if (chooseFile.FileNames.Length < 2)
            {
                Message message = new Message("至少选择两个文件");
                message.Show();
                return;
            }
            string[] FilePath = chooseFile.FileNames;
            string newFileName = chooseFile.SafeFileNames[0];
            newFileName = newFileName.Substring(0, newFileName.LastIndexOf(".") - 1) +
                         newFileName.Substring(newFileName.LastIndexOf("."), newFileName.Length - newFileName.LastIndexOf("."));
            finalFileName.Text = newFileName;
            IEnumerable<String> query = FilePath.OrderBy(x => x);
            foreach (string filePath in query)
            {
                fileToMerge.Text += filePath + "\r\n";
            }
        }

        private void sureSplit(object sender, RoutedEventArgs e)
        {
            Message message;
            if (fileToSplit.Text == "")
            {
                message = new Message("请选择文件");
                message.Show();
                return;
            }
            if (newFileName.Text == "")
            {
                message = new Message("请填写文件名称");
                message.Show();
                return;
            }

            message = new Message("开始分割");
            message.Show();
            FileInfo fileInfo = new FileInfo(fileToSplit.Text);
            FileStream reader = fileInfo.OpenRead();
            String newFilePath = fileInfo.Directory.FullName + "\\" + newFileName.Text;
            double totalSize = reader.Length;
            long totalWrite = 0;
            int fileNumber = int.Parse(newFileNumber.Text);
            int buffLength = 1 * 1024 * 1024;
            double buffNumber = Math.Ceiling(totalSize / buffLength);
            long fileSize = (long)Math.Ceiling(buffNumber / fileNumber);// 每个文件的buff数
            byte[] buff = new byte[buffLength];
            for (int i = 0; i < fileNumber; i++)
            {
                string fileName = newFilePath.Substring(0, newFilePath.LastIndexOf(".") - 1) + i +
                         newFilePath.Substring(newFilePath.LastIndexOf("."), newFilePath.Length - newFilePath.LastIndexOf("."));
                FileInfo newFile = new FileInfo(fileName);
                FileStream writer = newFile.Create();
                reader.Seek(fileSize * buffLength * i, 0);
                int contentLen = 0;
                long countBuff = 0;
                while ((contentLen = reader.Read(buff, 0, buffLength)) != 0 && countBuff < fileSize)
                {
                    writer.Write(buff, 0, contentLen);
                    totalWrite += contentLen;
                    countBuff ++;
                }
                writer.Close();
                if (message.IsLoaded)
                {
                    message.Close();
                }
                message = new Message("已分割 " + (i + 1) + " 个文件");
                message.Show();
            }
            reader.Close();
            if (afterSplit.Text == "是")
            {
                fileInfo.Delete();
            }
            if (message.IsLoaded)
            {
                message.Close();
            }
            message = new Message("分割完成");
            message.Show();
            return;
        }

        private void sureMerge(object sender, RoutedEventArgs e)
        {
            Message message;
            if (fileToMerge.Text == "")
            {
                message = new Message("请选择文件");
                message.Show();
                return;
            }
            if (finalFileName.Text == "")
            {
                message = new Message("请填写文件名称");
                message.Show();
                return;
            }

            message = new Message("开始合并");
            message.Show();

            string[] filePath = fileToMerge.Text.Split("\r\n".ToCharArray());
            string finalPath = filePath[0].Substring(0, filePath[0].LastIndexOf("\\") + 1) + finalFileName.Text;
            FileInfo finalFile = new FileInfo(finalPath);
            FileStream writer = finalFile.Create();
            long fileSize = 0;
            int count = 0;
            for (int i = 0; i < filePath.Length; i++)
            {
                if (filePath[i] == "")
                {
                    continue;
                }
                count ++;
                FileInfo fileInfo = new FileInfo(filePath[i]);
                FileStream reader = fileInfo.OpenRead();
                writer.Seek(fileSize, 0);
                int buffLength = 1 * 1024 * 1024;
                byte[] buff = new byte[buffLength];
                int contentLen = 0;
                while ((contentLen = reader.Read(buff, 0, buffLength)) != 0 )
                {
                    writer.Write(buff, 0, contentLen);
                }
                fileSize += reader.Length;
                reader.Close();
                if (message.IsLoaded)
                {
                    message.Close();
                }
                message = new Message("已合并 " + count + " 个文件");
                message.Show();
                if (afterMerge.Text == "是")
                {
                    fileInfo.Delete();
                }
            }
            writer.Close();
            if (message.IsLoaded)
            {
                message.Close();
            }
            message = new Message("合并完成");
            message.Show();
            return;
        }
    }
}
