using Client.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private readonly UdpClientService udpService;

        private string onlineUsers = "Пользователей онлайн: 0";
        public string UserName { get; }


        private static readonly string[] AvailableNames = new[]
        {
            "Пельмень 1", "Пельмень 2", "Пельмень 3", "Пельмень 4", "Пельмень 5", "Тапок",
            "Тюлень", "405 база", "Крутой чувак", "Вареник 1", "Вареник 2", "Дядя Женя",
            "Лебовски", "Просто чувак", "Найсик", "ИГОООООРЬ!!!", "Проходимец", "Йоу"
        };

        public ObservableCollection<string> Messages { get; } = new();
        public ObservableCollection<string> OnlineUserNames { get; } = new();
        public ICommand SendMessageCommand { get; }
        private string messageInput = "";
        public string MessageInput
        {
            get => messageInput;
            set { messageInput = value; OnPropertyChanged(); }
        }

        public string OnlineUsers
        {
            get => onlineUsers;
            set { onlineUsers = value; OnPropertyChanged(); }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Random random = new Random();
            UserName = AvailableNames[random.Next(AvailableNames.Length)];

            udpService = new UdpClientService("192.168.31.146", 8080, UserName);
            udpService.MessageReceived += OnMessageReceived;
            _ = udpService.ConnectAsync();

            Closing += async (_, __) => await udpService.DisconnectAsync();
        }


        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("USERS:"))
                {
                    OnlineUserNames.Clear();
                    var names = message.Substring(7).Split(',');
                    foreach (var n in names)
                        if (!string.IsNullOrWhiteSpace(n))
                        {
                            OnlineUserNames.Add(n); 
                        }

                    OnlineUsers = $"Пользователей онлайн: {OnlineUserNames.Count}";
                }
                else if (message.StartsWith("MSG:"))
                {
                    var parts = message.Split(':', 3);
                    if (parts.Length == 3)
                    {
                        string sender = parts[1];
                        string content = parts[2];
                        Messages.Add(sender == UserName ? $"Я: {content}" : $"{sender}: {content}");
                    }
                }
                else
                {
                    Messages.Add(message);
                }

                if (MessagesList.Items.Count > 0)
                {
                    MessagesList.ScrollIntoView(MessagesList.Items[^1]); 
                }
            });
        }


        public async Task DisconnectAsync()
        {
            await udpService.DisconnectAsync();
        }


        public async void SendMessageClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageInput))
            {
                await udpService.SendMessage(MessageInput);
                MessageInput = "";
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}