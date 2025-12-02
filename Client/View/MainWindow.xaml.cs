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
        private int onlineUsers;
        private readonly string userName;

        public ObservableCollection<string> Messages { get; } = new();
        public ICommand SendMessageCommand { get; }
        private string messageInput = "";
        public string MessageInput
        {
            get => messageInput;
            set { messageInput = value; OnPropertyChanged(); }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            userName = Guid.NewGuid().ToString().Substring(0, 5);
            udpService = new UdpClientService("192.168.31.146", 8080, userName);
            udpService.MessageReceived += OnMessageReceived;
            _ = udpService.ConnectAsync();

            SendMessageCommand = new RelayCommand(async _ =>
            {
                if (!string.IsNullOrWhiteSpace(MessageInput))
                {
                    await udpService.SendMessage(MessageInput);
                    Dispatcher.Invoke(() =>
                    {
                        Messages.Add($"Мое сообщение: {MessageInput}");
                        MessageInput = "";
                    });
                    OnPropertyChanged(nameof(MessageInput));
                }
            });
        }


        public int OnlineUsers
        {
            get => onlineUsers;
            set { onlineUsers = value; OnPropertyChanged(); }
        }


        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                Messages.Add(message);

                if (message.EndsWith("ONLINE"))
                {
                    OnlineUsers++;
                }
                else if (message.EndsWith("OFFLINE"))
                {
                    OnlineUsers = Math.Max(0, OnlineUsers - 1);
                }
                else if (message.StartsWith("MSG:"))
                {
                    var parts = message.Split(':', 3);
                    if (parts.Length == 3)
                    {
                        string sender = parts[1];
                        string content = parts[2];

                        if (sender == userName)
                        {
                            Messages.Add($"Я: {content}");
                        }

                        else
                        {
                            Messages.Add($"{sender}: {content}");
                        }
                    }
                }
                else
                {
                    Messages.Add(message); 
                }

            });
        }


        public async Task DisconnectAsync()
        {
            await udpService.DisconnectAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        public void SendMessageClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageInput))
            {
                _ = udpService.SendMessage(MessageInput);
                Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Мое сообщение: {MessageInput}");
                    MessageInput = "";
                });
                OnPropertyChanged(nameof(MessageInput));
            }
        }
    }
}