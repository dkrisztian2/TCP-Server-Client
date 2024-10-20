using System.IO;
using System.Net.Sockets;
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

namespace ClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;

        public MainWindow()
        {
            InitializeComponent();
            client = new TcpClient("127.0.0.1", 5000);
            stream = client.GetStream();
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            if (privateMessage.Text != "")
                message = $"{userName.Content}■{MessageInput.Text}■{privateMessage.Text}";
            else
                message = $"{userName.Content}■{MessageInput.Text}■PUBLICMESSAGE";


            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            MessageInput.Clear();
        }

        private void ReceiveMessages()
        {
            Task.Run(() =>
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                try
                {
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        // Decode message
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string[] elements = message.Split('■');

                        // Now we switch back to the UI thread to update the MessagesList
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (elements[2] == "PUBLICMESSAGE")
                            {
                                StackPanel newStackPanel = new StackPanel();
                                newStackPanel.Orientation = Orientation.Horizontal;

                                TextBlock sender = new TextBlock();
                                sender.Foreground = new SolidColorBrush(Colors.Red);
                                sender.Text = $"{elements[0]}: ";
                                newStackPanel.Children.Add(sender);

                                TextBlock message = new TextBlock();
                                message.Foreground = new SolidColorBrush(Colors.Black);
                                message.Text = elements[1];
                                newStackPanel.Children.Add(message);

                                MessagesList.Items.Add(newStackPanel);
                            }
                            else if (elements[2] == userName.Content.ToString() || elements[2] == elements[0])
                            {
                                StackPanel newStackPanel = new StackPanel();
                                newStackPanel.Orientation = Orientation.Horizontal;

                                TextBlock sender = new TextBlock();
                                sender.Foreground = new SolidColorBrush(Colors.Blue);
                                sender.Text = $"({elements[0]}-->{elements[2]}): ";
                                newStackPanel.Children.Add(sender);


                                TextBlock message = new TextBlock();
                                message.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                                message.Text = elements[1];
                                newStackPanel.Children.Add(message);

                                MessagesList.Items.Add(newStackPanel);
                            }
                        });
                    }
                }
                catch (IOException ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"I/O error: {ex.Message}");
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Unexpected error: {ex.Message}");
                    });
                }
            });
        }

        string Private(string message)
        {
            string[] elements = message.Split('■');
            return $"({elements[0]}-->{elements[2]}): {elements[1]}";
        }

    }
}