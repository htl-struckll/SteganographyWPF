using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Imaging;
using Steganography.Enums;
using MessageBox = System.Windows.MessageBox;
using DataFormats = System.Windows.DataFormats;

/*
 * decode = dekodieren
 */
namespace Steganography.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region var´s

        private string _fromPath;
        private string _toPath;
        private string _text;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }


        #region Events
        /// <summary>
        /// Drag drop of picture event encode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropAreaEncode_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null) { 
                    UpdatePictureEncode(files[0]);
                    FromFilePathInput.Text = files[0];
                }
            }
        }

        /// <summary>
        /// Drag drop of picture event decode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropAreaDecode_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null)
                {
                    UpdatePictureDecode(files[0]);
                    FromFilePathDecodeInput.Text = files[0];
                    ToFilePathInput.Text = GetNewPath(files[0]);
                }
            }
        }

        /// <summary>
        /// Decodes the file event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decode_Click(object sender, RoutedEventArgs e)
        {
            string path = FromFilePathDecodeInput.Text;
            if (File.Exists(path))
                ToDecodeOutput.Text = ExtractText(new Bitmap(Image.FromFile(path)));
            else
                Log("File path does not exist");
        }

        /// <summary>
        /// Encodes the file event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Encode_Click(object sender, RoutedEventArgs e)
        {
            if (!FromFilePathInput.Text.Equals(string.Empty) || !ToFilePathInput.Text.Equals(string.Empty) ||
                !ToEncodeInput.Text.Equals(string.Empty))
            {
                _fromPath = FromFilePathInput.Text;
                _toPath = ToFilePathInput.Text;
                _text = ToEncodeInput.Text;

                if (!_toPath.Equals(_fromPath))
                {
                    try
                    {
                        using (Bitmap bm = new Bitmap(Image.FromFile(_fromPath)))
                        {
                            EmbedText(_text, bm).Save(_toPath);
                        }

                        Log("Fin");
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                    }
                }
                else
                    Log("The paths can not be the same!");
            }
            else
                Log("Input has been let empty");
        }

        /// <summary>
        /// To file selecter event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToFileSelecter_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                RestoreDirectory = true,
                Filter= @"Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
        };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ToFilePathInput.Text = openFileDialog1.FileName;
                _toPath = openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// from file selector event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromFileSelecter_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                RestoreDirectory = true,
                Filter = @"Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FromFilePathInput.Text = openFileDialog1.FileName;
                _fromPath = openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// From path text changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdatePictureEncode(FromFilePathInput.Text);
            if (!FromFilePathInput.Text.Equals(string.Empty) && File.Exists(FromFilePathInput.Text))
                ToFilePathInput.Text = GetNewPath(FromFilePathInput.Text);
        }


        /// <summary>
        /// decode file selector event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecodePathFileSelector_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                RestoreDirectory = true,
                Filter = @"Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FromFilePathDecodeInput.Text = openFileDialog1.FileName;
                _fromPath = openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// Text changed event on decode file path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromDecodePath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!FromFilePathDecodeInput.Text.Equals(string.Empty) && File.Exists(FromFilePathDecodeInput.Text))
                UpdatePictureDecode(FromFilePathDecodeInput.Text);
        }
        #endregion

        #region En/Decoding

        /// <summary>
        /// Extracts the text from an image
        /// </summary>
        /// <param name="bmp">Image to extract from</param>
        /// <returns>Text in inmage</returns>
        public static string ExtractText(Bitmap bmp)
        {
            int colorUnitIndex = 0, charValue = 0;
            string extractedText = string.Empty;

            for (int height = 0; height < bmp.Height; height++)
            {
                for (int width = 0; width < bmp.Width; width++)
                {
                    Color pixel = bmp.GetPixel(width, height);

                    // pass trough rgb
                    for (int rgbIdx = 0; rgbIdx < 3; rgbIdx++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                charValue = charValue * 2 + pixel.R % 2;
                                break;
                            case 1:
                                charValue = charValue * 2 + pixel.G % 2;
                                break;
                            case 2:
                                charValue = charValue * 2 + pixel.B % 2;
                                break;
                        }
                        colorUnitIndex++;

                        //if 8 bits are fin add to the resultText
                        if (colorUnitIndex % 8 == 0)
                        {
                            // reverse: we start on the right (simpler)
                            charValue = ReverseBits(charValue);

                            // 0 equals 8 zeros
                            if (charValue == 0)
                                return extractedText;

                            extractedText += ((char)charValue).ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        /// <summary>
        /// Reverses bites
        /// </summary>
        /// <param name="b">bite</param>
        /// <returns>reversed bite</returns>
        public static int ReverseBits(int b)
        {
            int retVal = 0;

            for (int i = 0; i < 8; i++)
            {
                retVal = retVal * 2 + b % 2;

                b /= 2;
            }

            return retVal;
        }

        /// <summary>
        /// Embeds the text into the image
        /// </summary>
        /// <param name="text">Text to embed</param>
        /// <param name="bmp">Image to embed text into</param>
        /// <returns>The img with the embedded text</returns>
        public static Bitmap EmbedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;
            int charIndex = 0, pixelIdx = 0, zeros = 0;
            int letterInt = 0; //char value convertet to int to add

            for (int height = 0; height < bmp.Height; height++)
            {
                for (int width = 0; width < bmp.Width; width++)
                {
                    Color pixel = bmp.GetPixel(width, height);
                    //clear the least significant bit (LSB) from each pixel 
                    int r = pixel.R - pixel.R % 2, g = pixel.G - pixel.G % 2, b = pixel.B - pixel.B % 2;

                    //pass throug RGB
                    for (int rgbIdx = 0; rgbIdx < 3; rgbIdx++)
                    {
                        if (pixelIdx % 8 == 0) //Checks if 8 bits are over
                        {
                            if (state == State.FillingWithZeros && zeros == 8)
                            {
                                //applys the last pixels (RGB)
                                if ((pixelIdx - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(width, height, Color.FromArgb(r, g, b));
                                }

                                return bmp;
                            }

                            //Starts filling with zeros if finish / move to the next char if not finish
                            if (charIndex == text.Length)
                                state = State.FillingWithZeros;
                            else
                                letterInt = text[charIndex++];
                        }

                        // check which pixel element has the turn to hide a bit in its LSB
                        if (state.Equals(State.Hiding))
                        {
                            switch (pixelIdx % 3)
                            {
                                case 0:
                                    {
                                        r += letterInt % 2; //overwrite the old one
                                        letterInt /= 2; //remove it so we can reach the next one
                                    }
                                    break;
                                case 1:
                                    {
                                        g += letterInt % 2;
                                        letterInt /= 2;
                                    }
                                    break;
                                case 2:
                                    {
                                        b += letterInt % 2;
                                        letterInt /= 2;
                                    }
                                    break;
                            }
                        }

                        bmp.SetPixel(width, height, Color.FromArgb(r, g, b));
                        pixelIdx++;

                        if (state.Equals(State.FillingWithZeros))
                            zeros++;
                    }
                }
            }

            return bmp;
        }
        #endregion

        #region GUI updates

        /// <summary>
        /// Updates the picture in the gui
        /// </summary>
        /// <param name="path">Path of the picture</param>
        private void UpdatePictureEncode(string path)
        {
            if (File.Exists(path))
                ImageOutput.Source = new BitmapImage(new Uri(path)); 
        }

        /// <summary>
        /// Updates the picture in the gui
        /// </summary>
        /// <param name="path">Path of the picture</param>
        private void UpdatePictureDecode(string path)
        {
            if (File.Exists(path))
                ImageOutputDecode.Source = new BitmapImage(new Uri(path));
        }
        #endregion

        #region helpers
        /// <summary>
        /// Gets the new file path
        /// </summary>
        /// <param name="oldPath">Old path</param>
        /// <returns>New path</returns>
        private string GetNewPath(string oldPath)
        {
            string[] pathSplit = oldPath.Split('\\');
            string name = pathSplit[pathSplit.Length - 1], path = "";
            string ext = name.Split('.')[1];
            for (int idx = 0; idx < pathSplit.Length - 1; idx++)
                path += pathSplit[idx] + "\\";

            path += "newFile." + ext;
            return path;
        }
        #endregion

        /// <summary>
        /// Simple logging method
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="caption">Caption</param>
        /// <param name="btn">Nutton</param>
        /// <param name="image">Image</param>
        private void Log(string msg, string caption = "Info", MessageBoxButton btn = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information) => MessageBox.Show("[" + DateTime.Now + "] " + msg, caption,btn,image);

    }
}
