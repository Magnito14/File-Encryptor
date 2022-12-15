using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace File_Encryptor
{
    public partial class MainFRM : Form
    {
        public MainFRM() => InitializeComponent();

        private void button1_Click(object sender, EventArgs e) => listBox1.Items.Remove(listBox1.SelectedItem);

        private void button2_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openFileDialog.FileNames)
                        listBox1.Items.Add(file);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    listBox2.Items.Add(folderBrowserDialog.SelectedPath);
            }
        }

        private void button4_Click(object sender, EventArgs e) => listBox2.Items.Remove(listBox2.SelectedItem);

        private void button5_Click(object sender, EventArgs e) => textBox1.Text = GeneratePassword();

        private void button6_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Ensure you've memorized your password before continue. Continue?", "FileEncryptor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            if (listBox1.Items.Count > 0)
            {
                foreach (var v in listBox1.Items)
                {
                    if ($"{v}".Trim().EndsWith(".!LOCKED") || !File.Exists($"{v}"))
                        continue;

                    EncryptFile($"{v}", $"{v}.!LOCKED", textBox1.Text);
                    File.Delete($"{v}");
                }
            }

            if (listBox2.Items.Count > 0)
            {
                foreach (var v in listBox2.Items)
                {
                    foreach (var nameFile in Directory.GetFiles($"{v}").Where(nameFile => !(nameFile.Trim().EndsWith(".!LOCKED") || !File.Exists(nameFile))))
                    {
                        EncryptFile(nameFile, $"{nameFile}.!LOCKED", textBox1.Text);
                        File.Delete(nameFile);
                    }
                }
            }

            MessageBox.Show("Completed operation!", "FileEncryptor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will return the files you encrypted before. Please double check your password, as a wrong password will result in damaged files. Continue?", "FileProtect™", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            var chars = new[] { '!', '.', 'L', 'O', 'C', 'K', 'E', 'D' };

            if (listBox1.Items.Count > 0)
            {
                foreach (var v in listBox1.Items)
                {
                    if (!(!$"{v}".Trim().EndsWith(".!LOCKED") || !File.Exists($"{v}")))
                    {
                        DecryptFile($"{v}", ($"{v}").TrimEnd(chars), textBox1.Text);
                        File.Delete($"{v}");
                    }
                }
            }

            if (listBox2.Items.Count > 0)
            {
                foreach (var v in listBox2.Items)
                {
                    foreach (var name_file in Directory.GetFiles($"{v}").Where(name_file => !(!name_file.Trim().EndsWith(".!LOCKED") || !File.Exists(name_file))))
                    {
                        DecryptFile(name_file, name_file.TrimEnd(chars), textBox1.Text);
                        File.Delete(name_file);
                    }
                }
            }

            MessageBox.Show("Completed operation!", "FileEncryptor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EncryptFile(string szInputFile, string szOutputFile, string szPassword)
        {
            var key = new UnicodeEncoding().GetBytes(szPassword);

            using (var aes = Aes.Create())
            using (var fileStreamIn = new FileStream(szInputFile, FileMode.Open))
            using (var fileStreamOut = new FileStream(szOutputFile, FileMode.Create))
            using (var cryptoStream = new CryptoStream(fileStreamOut, aes.CreateEncryptor(key, key), CryptoStreamMode.Write))
            {
                var data = 0;

                while ((data = fileStreamIn.ReadByte()) != -1)
                    cryptoStream.WriteByte((byte)data);
            }
        }

        private void DecryptFile(string szInputFile, string szOutputFile, string szPassword)
        {
            var key = new UnicodeEncoding().GetBytes(szPassword);

            using (var aes = Aes.Create())
            using (var fileStreamIn = new FileStream(szInputFile, FileMode.Open))
            using (var fileStreamOut = new FileStream(szOutputFile, FileMode.Create))
            using (var cryptoStream = new CryptoStream(fileStreamIn, aes.CreateDecryptor(key, key), CryptoStreamMode.Read))
            {
                var data = 0;

                while ((data = cryptoStream.ReadByte()) != -1)
                    fileStreamOut.WriteByte((byte)data);
            }
        }

        private string GeneratePassword()
        {
            var random = new Random();

            var characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
            var password = new string(Enumerable.Repeat(characters, 8).Select(str => str[random.Next(str.Length)]).ToArray());

            return password;
        }
    }
}