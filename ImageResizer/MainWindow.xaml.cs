using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace ImageResizer
{
    public partial class MainWindow : Window
    {
        BitmapImage selectedImage;
        string imageFileSourcePath;
        string imageFileSourceName;

        public MainWindow()
        {
            InitializeComponent();

            toBox.Text = Properties.Settings.Default.Save;
            sizeBox.Text = Properties.Settings.Default.Size;
            quality.Text = Properties.Settings.Default.Quality;

            fromBox.GotFocus += RemoveText;
            fromBox.LostFocus += AddText;
            toBox.GotFocus += RemoveText;
            toBox.LostFocus += AddText;

            string openImagePath = (System.Windows.Application.Current as App).openImagePath;
            if(openImagePath != null)
            if (openImagePath.Contains(".jpg") || openImagePath.Contains(".png"))
            {
                imageFileSourcePath = openImagePath;
                fromBox.Text = imageFileSourcePath;

                string[] strs = imageFileSourcePath.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
                imageFileSourceName = strs[strs.Length - 1];

                selectedImage = new BitmapImage();
                selectedImage.BeginInit();
                selectedImage.UriSource = new Uri(imageFileSourcePath);
                selectedImage.EndInit();

                image.Source = selectedImage;
            }
        }

        public void RemoveText(object sender, EventArgs e)
        {
            if(sender.Equals(fromBox))
                if (fromBox.Text.Equals("Choose image..."))
                    fromBox.Text = "";
            if (sender.Equals(toBox))
                if (toBox.Text.Equals("Save image to..."))
                    toBox.Text = "";
        }

        public void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fromBox.Text))
                fromBox.Text = "Choose image...";
            if (string.IsNullOrWhiteSpace(toBox.Text))
                toBox.Text = "Save image to...";
        }

        private void fromButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension
            dlg.Filter = "All files (*.*)|*.*|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
            
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document
                imageFileSourcePath = dlg.FileName;
                fromBox.Text = imageFileSourcePath;

                string[] strs = imageFileSourcePath.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
                imageFileSourceName = strs[strs.Length - 1];

                selectedImage = new BitmapImage();
                selectedImage.BeginInit();
                selectedImage.UriSource = new Uri(imageFileSourcePath);
                selectedImage.EndInit();

                image.Source = selectedImage;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            string side = sizeBox.Text;
            int bigSize = 0;
            try
            {
                bigSize = Int32.Parse(side);
            }
            catch
            {
                System.Windows.MessageBox.Show("Select another size", "Size error");
                return;
            }
            
            string filePath = toBox.Text + @"\" + imageFileSourceName;
            try
            {
                System.IO.Path.GetFullPath(filePath);
            }
            catch
            {
                System.Windows.MessageBox.Show("Selected destination path is not correct!", "Path error");
                return;
            }
            var imageToSave = System.Drawing.Image.FromFile(imageFileSourcePath);

            //saveImage(filePath, ScaleImage(imageToSave, bigSize, bigSize));
            Image newImage = null;
            if (imageToSave.Size.Width < bigSize && imageToSave.Size.Height < bigSize)
            {
                saveImage(filePath, imageToSave);
            }
            else
            {
                newImage = ScaleImage(imageToSave, bigSize, bigSize);

                saveImage(filePath, newImage);
            }
            //Process.Start(toBox.Text);
        }

        private void saveImage(string pathToSave, Image image)
        {
            try
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, Int64.Parse(quality.Text));
                myEncoderParameters.Param[0] = myEncoderParameter;

                string[] parts = pathToSave.Split('.');
                string newPath = parts[0];
                for (int i = 0; i < parts.Length - 2; i++)
                {
                    newPath = newPath + '.' + parts[i];
                }
                newPath += "(1).jpg";

                image.Save(newPath, jpgEncoder, myEncoderParameters);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                string[] parts = pathToSave.Split('.');
                string newPath = parts[0];
                for(int i = 0; i < parts.Length - 2; i++)
                {
                    newPath = newPath + '.' + parts[i];
                }
                newPath += "(1).jpg";
                saveImage(newPath, image);
            }
        }

        private void toButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                toBox.Text = fbd.SelectedPath;
            }
        }

        public Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        public Image TrimImage(Image image)
        {
            Bitmap newImage = (Bitmap) image;

            var pixel = newImage.GetPixel(0, 0);

            bool colorChanged = false;

            int heightPoint = 0;
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    var currentPixel = newImage.GetPixel(j, i);

                    if(Math.Abs(pixel.R - currentPixel.R) > 5 &&
                        Math.Abs(pixel.G - currentPixel.G) > 5 &&
                        Math.Abs(pixel.B - currentPixel.B) > 5)
                    {
                        //if (i - 1 >= 0)
                            heightPoint = i;
                        //else heightPoint = i;
                        colorChanged = true;
                        break;
                    }
                }
                if (colorChanged)
                    break;
            }
            colorChanged = false;
            int lowPoint = 0;
            for (int i = image.Height - 1; i > 0; i--)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    var currentPixel = newImage.GetPixel(j, i);

                    if (Math.Abs(pixel.R - currentPixel.R) > 5 ||
                        Math.Abs(pixel.G - currentPixel.G) > 5 ||
                        Math.Abs(pixel.B - currentPixel.B) > 5)
                    {
                        if (i + 1 < image.Height)
                            lowPoint = i + 1;
                        else lowPoint = i;
                        colorChanged = true;
                        break;
                    }
                }
                if (colorChanged)
                    break;
            }
            colorChanged = false;

            int leftPoint = 0;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var currentPixel = newImage.GetPixel(i, j);
                    if (Math.Abs(pixel.R - currentPixel.R) > 5 ||
                        Math.Abs(pixel.G - currentPixel.G) > 5 ||
                        Math.Abs(pixel.B - currentPixel.B) > 5)
                    {
                        //if (i - 1 > 0)
                           leftPoint = i;
                        //else leftPoint = i;
                        colorChanged = true;
                        break;
                    }
                }
                if (colorChanged)
                    break;
            }

            colorChanged = false;
            int rightPoint = 0;

            for (int i = image.Width - 1; i > 0; i--)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var currentPixel = newImage.GetPixel(i, j);
                    if (Math.Abs(pixel.R - currentPixel.R) > 5 ||
                        Math.Abs(pixel.G - currentPixel.G) > 5 ||
                        Math.Abs(pixel.B - currentPixel.B) > 5)
                    {
                        if (i + 1 < image.Width)
                            rightPoint = i + 1;
                        else rightPoint = i;
                        colorChanged = true;
                        break;
                    }
                }
                if (colorChanged)
                    break;
            }

            Rectangle source = new Rectangle(leftPoint, heightPoint, rightPoint, lowPoint);
            Rectangle target = new Rectangle(0, 0, rightPoint, lowPoint);
            var trimmedImage = new Bitmap(rightPoint - leftPoint, lowPoint - heightPoint);

            Graphics g = Graphics.FromImage(trimmedImage);

            g.DrawImage(image, target, source, GraphicsUnit.Pixel);

            return trimmedImage;
        }

        private void image_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

                imageFileSourcePath = files[0];
                fromBox.Text = imageFileSourcePath;

                string[] strs = imageFileSourcePath.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
                imageFileSourceName = strs[strs.Length - 1];

                /*
                Image tempImage;
                using (var bmpTemp = new Bitmap(imageFileSourcePath))
                {
                    tempImage = new Bitmap(bmpTemp);
                    
                    File.Delete(Environment.CurrentDirectory + "\\temp.jpg");
                    tempImage.Save(Environment.CurrentDirectory + "\\temp.jpg");
                }
                */

                selectedImage = new BitmapImage();
                selectedImage.BeginInit();
                selectedImage.CacheOption = BitmapCacheOption.OnLoad;
                selectedImage.UriSource = new Uri(imageFileSourcePath);
                //selectedImage.UriSource = new Uri(Environment.CurrentDirectory + "\\temp.jpg");
                selectedImage.EndInit();

                image.Source = selectedImage;

                // Resizing and saving
                //var trimmedImage = TrimImage(Image.FromFile(Environment.CurrentDirectory + "\\temp.jpg"));
                var trimmedImage = TrimImage(Image.FromFile(imageFileSourcePath));

                int bigSize = 0;
                try
                {
                    bigSize = Int32.Parse(sizeBox.Text);
                }
                catch
                {
                    bigSize = 735;
                }

                if (trimmedImage.Width > bigSize || trimmedImage.Height > bigSize)
                {
                    var resizedImage = ScaleImage(trimmedImage, bigSize, bigSize);
                    saveImage(imageFileSourcePath, resizedImage);
                }
                else
                {
                    saveImage(imageFileSourcePath, trimmedImage);
                }
            }
        }

        private void trimButton_Click(object sender, RoutedEventArgs e)
        {
            var imageToTrim = Image.FromFile(imageFileSourcePath);
            var trimmedImage = TrimImage(imageToTrim);

            try
            {
                trimmedImage.Save("temp.jpg", ImageFormat.Jpeg);
                /*if (imageFileSourcePath.Contains("jpg"))
                {
                    trimmedImage.Save("temp.jpg", ImageFormat.Jpeg);
                }
                if (imageFileSourcePath.Contains("png"))
                {
                    trimmedImage.Save("temp.png", ImageFormat.Png);
                }*/
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }

            selectedImage = new BitmapImage();
            selectedImage.BeginInit();
            selectedImage.UriSource = new Uri(Environment.CurrentDirectory + "\\temp.jpg");
            imageFileSourcePath = Environment.CurrentDirectory + "\\temp.jpg";
            /*if (imageFileSourcePath.Contains("jpg"))
            {
                selectedImage.UriSource = new Uri(Environment.CurrentDirectory + "\\temp.jpg");
                imageFileSourcePath = Environment.CurrentDirectory + "\\temp.jpg";
            }
            if (imageFileSourcePath.Contains("png"))
            {
                selectedImage.UriSource = new Uri(Environment.CurrentDirectory + "\\temp.png");
                imageFileSourcePath = Environment.CurrentDirectory + "\\temp.png";
            }*/
            selectedImage.EndInit();

            image.Source = selectedImage;
        }

        private void untrimButton_Click(object sender, RoutedEventArgs e)
        {
            if (fromBox.Text.Equals("Choose image...") || fromBox.Text.Equals(""))
                return;

            imageFileSourcePath = fromBox.Text;
            
            selectedImage = new BitmapImage();
            selectedImage.BeginInit();
            selectedImage.UriSource = new Uri(imageFileSourcePath);
            selectedImage.EndInit();

            image.Source = selectedImage;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        
        void Application_ApplicationExit(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save = toBox.Text;
            Properties.Settings.Default.Size = sizeBox.Text;
            Properties.Settings.Default.Quality = quality.Text;
            Properties.Settings.Default.Save();
        }
    }
}
