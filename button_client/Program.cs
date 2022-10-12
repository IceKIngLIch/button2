using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace button_client
{

    class Program
    {
        
        class Client
        {
            const int port = 8888;
            const string address = "127.0.0.1"; // ip сервера
            private TcpClient client = null; // создание экземпляра класса клиента
            private NetworkStream stream;
            private byte[] data;
            public void SendMes(string message)
            {
                // преобразуем сообщение в массив байтов
                byte[] data = Encoding.Unicode.GetBytes(message);
                // отправка сообщения
                stream.Write(data, 0, data.Length);
            }
            public string GetMes()
            {
                data = new byte[64]; // буфер для получаемых данных
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                return builder.ToString();
            }
            public bool RunClient()
            {
                try
                {
                    client = new TcpClient(address, port);// заполнение клиента
                    stream = client.GetStream(); //  получение на основе клиента потока для обмена информациоей
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            public void CloseСonnect()
            {
                client.Close();
            }
        }
        static void Main(string[] args)
        {
            Client user = new Client();

            if (user.RunClient())
            {
                Console.WriteLine("Подключение с сервером установлено ");
                Console.Write("Введите стоимость машины: ");
                user.SendMes(Console.ReadLine());
                Console.Write("Введите год выпуска машины: ");
                user.SendMes(Console.ReadLine());
                Console.Write("Введите объем двигателя: ");
                user.SendMes(Console.ReadLine());
                Console.WriteLine("Стоимость растаможки: " + user.GetMes());
                user.CloseСonnect();

                Console.Write("сессия закончена  введите close для выхода: ");
                do
                {
                    if (Console.ReadLine() == "close")
                    { break; }
                    else
                    {
                        Console.Write("close!!!!: ");
                    }
                } while (true);
            }
            else 
            {
                Console.Write("Подключение с сервером не установлено введите close и попытайтесь переподключиться: ");
                do {
                    if (Console.ReadLine() == "close")
                    { break; }
                    else 
                    {
                        Console.Write("close!!!!: ");
                    }
                        } while (true);
                
            }
            
        }
    }
}
