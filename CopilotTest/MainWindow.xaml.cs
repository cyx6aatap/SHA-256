using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;

namespace SHA256
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public int remainder;
        public int originalRemainder;
        public byte[][] hash_tbl = new byte[32][];
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < hash_tbl.Length; i++)
            {
                hash_tbl[i] = new byte[32];
                for (int j = 0; j < hash_tbl[i].Length; j++)
                {
                    hash_tbl[i][j] = 0x00;
                }
            }
            TextBlock2x2Password.Foreground = Brushes.Wheat;
            updateTextBlock();
            // call function Window_KeyDown
            this.KeyDown += new KeyEventHandler(Window_KeyDown);

        }
        // 這是一個測試的按鈕
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            passwordBox.IsEnabled = true;
        }
        //Method to check  for internet connection, return true if connected,
        private bool CheckForInternetConnection()
        {
            try
            {
                System.Net.IPHostEntry ipHostEntry = System.Net.Dns.GetHostEntry("www.google.com");
                return true;
            }
            catch
            {
                return false;
            }
        }

        //if CheckForInternetConnection() return true, show me the progress bar value increase by 10 every 0.1 second. (please use async Task to solve)
        private async void Internet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckForInternetConnection())
            {
                for (int i = 0; i <= 100; i += 1)
                {
                    await Task.Delay(5);
                    progressBar.Value = i;
                }
                MessageBox.Show("Internet Connected");
            }
            else {MessageBox.Show("No Internet Connection");}
        }
        //button to call Encrypt method, show me the encrypted string in a message box.
        private async void Encrypt_Button_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            byte[] hash = Encrypt(passwordBox.Password);
            hash_tbl = new byte[32][];
            for (int i = 0; i < hash_tbl.Length; i++)
            {
                hash_tbl[i] = new byte[32];
                for (int j = 0; j < hash_tbl[i].Length; j++)
                {
                    byte randomNumber = (byte)random.Next(0x00, 0xFF);
                    hash_tbl[i][j] = randomNumber;
                }
            }

            for (int i = 0; i < hash_tbl.Length; i++)
            {
                if (remainder != 0)
                    hash_tbl[i][(remainder + i) % 32] = hash[i];
                else
                    hash_tbl[i][(i+1)%32] = hash[i];
                
            }
            TextBlock2x2Password.Foreground = Brushes.Wheat;
            updateTextBlock();

            for (int i = 0; i <= 50; i++ )
            {
                await Task.Delay(1);
                progressBar.Value = 2*i;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save .tbl file.";
            saveFileDialog.Filter = "Table Files (*.tbl)|*.tbl|All Files (*.*)|*.*";
            saveFileDialog.FileName = "dft.tbl";
            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                byte[] flattenedArray = hash_tbl.SelectMany(arr => arr).ToArray();

                File.WriteAllBytes(saveFileDialog.FileName, flattenedArray);
                MessageBox.Show("Save tbl. file success!");

            }

            //passwordBox -> enable = false
            passwordBox.IsEnabled = false;
        }

        // method to use sha-256 encryption to encrypt the input string(in passwordBox), return the 32bytes encrypted string.
        private byte[] Encrypt(string input)
        {
            int chksum = 0;
            remainder = 0;
            for (int i =0;i<input.Length;i++)
            {
                chksum += (byte)input[i];
            }
            remainder = chksum % 32;
            originalRemainder = chksum % 32;
            TextBlockChkSum.Text = $"ChkSum:\n{chksum}";
            TextBlockRemainder.Text = $"Remainder:\n{remainder}";

            System.Security.Cryptography.SHA256 sha256 = new System.Security.Cryptography.SHA256CryptoServiceProvider();
            byte[] source = Encoding.Default.GetBytes(input);
            byte[] crypto = sha256.ComputeHash(source);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < crypto.Length; i++)
            {
                
                builder.Append(crypto[i].ToString("x2")); // 將每個位元組轉換為十六進位字串
            }
            MessageBox.Show(builder.ToString());

            return crypto;
        }

        // in any time, when user press "Right", remainder+=1; when user press "Left", remainder-=1;
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            TextBlock2x2Password.Foreground = Brushes.MintCream;
            if (e.Key == Key.Right)
            {
                remainder += 1;
            }
            else if (e.Key == Key.Left)
            {
                remainder -= 1;
            }
            if (remainder%32 == originalRemainder || (remainder%32 == 1 && originalRemainder%32 == 0) || (remainder % 32 == 0 && originalRemainder % 32 == 1))
                TextBlock2x2Password.Foreground = Brushes.Wheat;
            updateTextBlock();
        }

        private void updateTextBlock()
        {
            string s = "";

            for (int i = 0; i < hash_tbl.Length; i++)
            {
                for (int j = 0; j < hash_tbl[i].Length; j++)
                {
                    if (j == (i + remainder+16384) % 32 || i==j)
                        s += "  ";
                    else
                        s += hash_tbl[i][j].ToString("X2");

                    if (j != 31)
                        s += " ";
                    else
                        s += "\n";
                }

            }

            string s2 = "";
            bool original0 = false;
            if (remainder%32 == 0)
            {
                remainder = 1;
                original0 = true;
            }
                
            for (int i = 0; i < hash_tbl.Length; i++)
            {
                for (int j = 0; j < hash_tbl[i].Length; j++)
                {
                    
                    if (j != (i + remainder + 16384) % 32 || i == j)
                        s2 += "  ";
                    else
                        s2 += hash_tbl[i][j].ToString("X2");

                    if (j != 31)
                        s2 += " ";
                    else
                        s2 += "\n";
                }
            }
            if (original0 == true)
            {
                remainder = 0;
                original0 = false;
            }
            TextBlock2x2.Text = s;
            TextBlock2x2Password.Text = s2;
            if (remainder < 0)
                remainder += 16384;
            TextBlock2x2Remainder.Text = (remainder%32).ToString("X2");
        }   
    }
}
