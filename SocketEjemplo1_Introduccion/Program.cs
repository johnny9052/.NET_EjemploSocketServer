using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*Using necesario para el uso de sockets*/
using System.Net.Sockets;
using System.Net;


namespace SocketEjemplo1_Introduccion
{
    class Program
    {

        /*Donde se almacena un dato recibido*/
        private static byte[] buffer = new byte[1024];

        /*Lista de clientes que se han aceptado*/
        private static List<Socket> listaClientes = new List<Socket>();


        /*Configuracion del socket servidor, con la iplocal y stream indica envio bidireccional*/
        /*new Socket (IP-V4, TIPO COMUNICACION (STREAM BIDIRECCIONAL, PROTOCOLO))*/
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork,
                                           SocketType.Stream,
                                           ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.Title = "Servidor";
            setupServer();

            /*Deja la consola permanentemente*/
            Console.ReadLine();
        }


        /*Para configurar el servidor*/
        static void setupServer()
        {
            Console.WriteLine("Configurando servidor");
            /*Asociar elemento (Se especifica un endpoint(IPLOCAL, PUERTO DE ESCUCHA ))*/
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            /*Longitud maxima de peticiones pendientes*/
            serverSocket.Listen(1);
            /*Inicia la escucha de peticiones, cuando recibe su primer cliente, 
             * llama a la funcion acceptCallBack y se manda el socket aceptado
             por parametro, como apenas inicio una conexion no es necesario mandarlo*/
            serverSocket.BeginAccept(new AsyncCallback(AceptarSolicitud), null);
        }


        private static void AceptarSolicitud(IAsyncResult AR)
        {
            /*Se acepta un cliente, mandando como referencia quien llamo 
             a la funcion de aceptar solicitud*/
            Socket clientSocket = serverSocket.EndAccept(AR);
            /*Se añade a la lista de clientes aceptados*/
            listaClientes.Add(clientSocket);
            Console.WriteLine("Cliente aceptado");

            /*Inicia la recepcion de un dato*/
            /*(Donde se almacena, posicion inicial buffer, posicion final buffer, bandera de 
             combinacion de bits,funcion que se ejecutara, socket referencia)*/
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                new AsyncCallback(RecibirDato), clientSocket);

            /*Se queda a la espera para aceptar otro nuevo cliente*/
            serverSocket.BeginAccept(new AsyncCallback(AceptarSolicitud), null);
        }


        private static void RecibirDato(IAsyncResult AR)
        {
            /*Se referencia el socket*/
            Socket clientSocket = (Socket)AR.AsyncState;

            /*Se finaliza la recepcion, obteniendo el tamaño de los datos recibidos*/
            int received = clientSocket.EndReceive(AR);

            /*Se define un buffer temporal del tamaño de los datos recibidos*/
            byte[] tempBuffer = new byte[received];

            /*Se copia el buffer temporal al buffer global*/
            Array.Copy(buffer, tempBuffer, received);

            /*Se saca el texto del buffer temporal que contiene la info recibida*/
            string text = Encoding.ASCII.GetString(tempBuffer);

            Console.WriteLine("Dato recibido: " + text);

            String response = "";

            if (text.ToLower() != "get time")
            {
                /*Si no se hace una solicitud valida*/
                response = "Solicitud invalida";
            }
            else
            {
                /*Si se solicito la fecha se obtiene*/
                response = DateTime.Now.ToLongTimeString();
            }

            /*Se coloca lo que se va a responder al cliente en un array de byte*/
            byte[] dataResponse = Encoding.ASCII.GetBytes(response);

            /*Se inicia la respuesta*/
            /*Datos a responder, inicio del array, posiciones a enviar, emparejamiento de bytes,
             funcion a llamar, socket referenciado*/
            clientSocket.BeginSend(dataResponse, 0, dataResponse.Length, SocketFlags.None,
                new AsyncCallback(EnviarDato), clientSocket);


            /*Se queda a la espera para recibir mas datos de ese mismo cliente, no de otros*/
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                new AsyncCallback(RecibirDato), clientSocket);
        }

        private static void EnviarDato(IAsyncResult AR)
        {
            /*Se referencia el socket*/
            Socket socket = (Socket)AR.AsyncState;
            /*Cierra la conexion con el cliente, enviado una respuesta con la configuracion 
             planteada*/
            socket.EndSend(AR);
        }

    }
}
