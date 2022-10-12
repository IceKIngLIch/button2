using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;

namespace test_button
{
    public class CarPrice
    { 
        private int _currentYear =2022;
         
        private double _result = 0;
        public double calcRuCustomsTax(int year,int engineV,double price,int electro=0)
        {
            int _yearsOld = _currentYear - year;
            int _engineV = engineV;
            double _price = price;
            int _electro = electro;
            if (_electro == 1)
            {
                return _price * 0.15;
            }
            if (_yearsOld <= 3)
            {
                _result = _price * 0.48;
            }
            else if (_yearsOld > 3 && _yearsOld <= 5)
            {
                if (_engineV <= 1000)
                {
                    _result = _engineV * 1.5;
                }
                if (_engineV <= 1500)
                {
                    _result = _engineV * 1.7;
                }
                if (_engineV <= 1800)
                {
                    _result = _engineV * 2.5;
                }
                if (_engineV <= 2300)
                {
                    _result = _engineV * 2.7;
                }
                if (_engineV <= 3000)
                {
                    _result = _engineV * 3;
                }
                if (_engineV > 3000)
                {
                    _result = _engineV * 3.6;
                }
            }
            else if (_yearsOld > 5)
            {
                if (_engineV <= 1000)
                {
                    _result = _engineV * 3;
                }
                if (_engineV <= 1500)
                {
                    _result = _engineV * 3.2;
                }
                if (_engineV <= 1800)
                {
                    _result = _engineV * 3.5;
                }
                if (_engineV <= 2300)
                {
                    _result = _engineV * 4.8;
                }
                if (_engineV <= 3000)
                {
                    _result = _engineV * 5;
                }
                if (_engineV > 3000)
                {
                    _result = _engineV * 5.7;
                }
            }
            if (_result > 0)
            {                
                if (_yearsOld < 3)
                {
                    _result += 333 * 0.17;
                }
                else
                {
                    _result += 333 * 0.26;
                }
            }
            return _result;
        }
    }

    public class ClientObject
    {
        private TcpClient client; // класс для клиентского подключения по TCP
        public ClientObject(TcpClient tcpClient, SqliteConnection connection)
        {
            client = tcpClient;
            this.connection = connection;//бд в конструкторе
        }
        private NetworkStream stream = null;
        private SqliteConnection connection; // бд
        
        byte[] data;// буфер для получаемых данных
        public void RunClient()
        {
            stream = client.GetStream();// возвращает объект network streem для отправки и получения данных по данному клиентскому подключению
           
            
        }
        public string GetMes()
        {
            data = new byte[64];
            // получаем сообщение
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length); //считывает данные из потока в массив байтов
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));//байты в строку
            }
            while (stream.DataAvailable); // возвращает значение есть ли в потоке данные для чтения

            return builder.ToString();
        }
        public void SendMes(string message)
        {
            data = Encoding.Unicode.GetBytes(message);//сообщение в байты 
            stream.Write(data, 0, data.Length); //отправляет сообщение через поток
        }
        public void CloseConect()
        {
            stream.Close();
            client.Close();
        }
        public void Process()        
        {            
            try
            {                
                RunClient();
                double price = Convert.ToDouble(GetMes());                
                int year= Convert.ToInt32(GetMes());
                int engineV= Convert.ToInt32(GetMes());
               


                string ip = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();// вытаскиваем ip клиента
                connection.Open();// открыть бд
                SqliteCommand command = connection.CreateCommand();// класс для запросов к бд                
                command.CommandText = $"INSERT INTO calcRuCustomsTax (IP_Client,Year,EngineV,Price) VALUES ('{ip}',{year},{engineV},{price})";//создаем команду для заполнеия бд
                command.ExecuteNonQuery();// заполняем таблицу в бд
                connection.Close();

                CarPrice Car = new CarPrice();
                string message = "Стоимость:" + Car.calcRuCustomsTax(year, engineV, price);
                SendMes(message);



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                CloseConect();                    
            }
        }
    }
    


    class Program
    {
            
            const int port = 8888; // выбираем порт для соединия
            static TcpListener listener; //  класс для  хоста чтобы осеществлять прослушку
            static void Main(string[] args)
            {
            SqliteConnection connection; // бд

            /*
            //SqliteCommand command; //класс для выполнения запросов */  
            string path= Environment.CurrentDirectory.ToString();
            path=Directory.GetParent(path).ToString();
            path = Directory.GetParent(path).ToString();
            path = Directory.GetParent(path).ToString()+ "\\usersdata.db";// путь до бд
            
            using ( connection = new SqliteConnection($"Data Source={path}"))//  подключениек  бд в папке с проектом, при попытки открытия бд при ее отсутствии гененрит новую
            {
                /*connection.Open();                 
                //ComandText хранит команду 
                //создание таблицы
                //command = connection.CreateCommand();
                //command.CommandText = "CREATE TABLE calcRuCustomsTax(_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, IP_Client TEXT NOT NULL, Year INTEGER NOT NULL,EngineV INTEGER NOT NULL,Price DOUBLE NOT NULL)";//инициализация таблицы
                //command.ExecuteNonQuery();// исполнения команды*/
            }

            try
                {
                    listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port); // задаем параметры хоста
                    listener.Start(); // запускает режим ожидания для входязих запросов в метож можно передать конечное число ожидающих подключения
                    Console.WriteLine("Ожидание подлкючений ....");
                    
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient(); // принимает запрос на подключение из очереди запросов  в listener и создает клиента  по данным запроса
                        ClientObject clientObject = new ClientObject(client,connection); // создаем экземпляр своего класса для обмена данными в поделючении + бд

                        // создаем новый поток для обслуживания нового клиента
                        Thread clientThread = new Thread(new ThreadStart(clientObject.Process)); // создание новго потока  threadstart нужен для указания метода используемого при запуске потока
                        clientThread.Start(); 
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (listener != null)
                        listener.Stop();
                }
            }


            


    }
    
}
