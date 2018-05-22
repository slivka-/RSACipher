using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RSACipher
{
    public partial class MainWindow : Window
    {
        private readonly int PRIME_PRECISION = 200;

        private byte[] InputImage = null;
        private RSACore EncryptionCore = null;
        private EncryptedImage encryptedImage = null;
        private bool encrypting;

        public MainWindow()
        {
            InitializeComponent();
            Task.Factory.StartNew(() => { InitializeEncryption(); });
        }

        private void InitializeEncryption()
        {
            StatusLabel.Dispatcher.Invoke(() =>
            {
                StatusLabel.Text = "Please wait, initializing encryption core...";
            });
            EncryptionCore = new RSACore(PRIME_PRECISION);
            StatusLabel.Dispatcher.Invoke(() =>
            {
                StatusLabel.Text = string.Format("Done! Core initialized in {0} ms", EncryptionCore.initTime);
                StatusLabel.Foreground = System.Windows.Media.Brushes.Green;
            });
        }

        private void Btn_LoadImage_Click(object sender, RoutedEventArgs e)
        {
            Btn_EncryptImage.IsEnabled = false;
            Btn_DecryptImage.IsEnabled = false;
            Btn_SaveImage.IsEnabled = false;
            encrypting = true;
            OpenFileDialog imageDialog = new OpenFileDialog()
            {
                InitialDirectory = "G:\\",
                Filter = "jpg images (*.jpg)|*.jpg|Encrypted image (*.eims)|*.eims",
                RestoreDirectory = true,
                AddExtension = true
            };

            if (imageDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (imageDialog.FileName.Split('.').Last() == "jpg")
                {
                    InputImage = File.ReadAllBytes(imageDialog.FileName);
                    Btn_EncryptImage.IsEnabled = true;
                    encrypting = true;
                }
                if (imageDialog.FileName.Split('.').Last() == "eims")
                {
                    IFormatter formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(imageDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        encryptedImage = (EncryptedImage)formatter.Deserialize(stream);
                    }
                    if (encryptedImage != null)
                    {
                        encrypting = false;
                        Btn_DecryptImage.IsEnabled = true;
                    }
                }
            }
        }

        private void Btn_ClearImage_Click(object sender, RoutedEventArgs e)
        {
            Btn_EncryptImage.IsEnabled = false;
            Btn_DecryptImage.IsEnabled = false;
            Btn_SaveImage.IsEnabled = false;
            encrypting = true;
        }

        private void Btn_EncryptImage_Click(object sender, RoutedEventArgs e)
        {
            if (EncryptionCore != null)
            {
                StatusLabel.Text = "Encryption in progress...";
                Task.Factory.StartNew(()=> {
                    encryptedImage = EncryptionCore.Encrypt(InputImage);
                    StatusLabel.Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Text = string.Format("Encryption done in {0} ms!", EncryptionCore.initTime);
                        Btn_SaveImage.IsEnabled = true;
                    });
                });
            }
            else
            {
                System.Windows.MessageBox.Show("Please wait for encryption core initialization!","Please wait",MessageBoxButton.OK,MessageBoxImage.Asterisk);
            }
        }


        private void Btn_DecryptImage_Click(object sender, RoutedEventArgs e)
        {
            if (EncryptionCore != null)
            {
                StatusLabel.Text = "Decryption in progress...";
                Task.Factory.StartNew(() => {
                    InputImage = EncryptionCore.Decrypt(encryptedImage);
                    StatusLabel.Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Text = string.Format("Decryption done in {0} ms!", EncryptionCore.initTime);
                        Btn_SaveImage.IsEnabled = true;
                    });
                });
            }
            else
            {
                System.Windows.MessageBox.Show("Please wait for encryption core initialization!", "Please wait", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void Btn_SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (encrypting)
            {
                SaveFileDialog saveDialog = new SaveFileDialog()
                {
                    InitialDirectory = "G:\\",
                    Filter = "Encrypted image (*.eims)|*.eims",
                    AddExtension = true,
                    RestoreDirectory = true
                };
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IFormatter formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        formatter.Serialize(stream, encryptedImage);
                    }
                }
            }
            else
            {
                SaveFileDialog saveDialog = new SaveFileDialog()
                {
                    InitialDirectory = "G:\\",
                    Filter = "jpg images (*.jpg)|*.jpg",
                    AddExtension = true,
                    RestoreDirectory = true
                };
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    File.WriteAllBytes(saveDialog.FileName, InputImage);
                }
            }
        }
    }
}
