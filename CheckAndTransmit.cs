using MCargaModBus.Persistence;
using MySQLClass;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using log4net;
using log4net.Config;
using EasyModbus;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing MCargaModBus.exe)

namespace MCargaModBus
{
    class CheckAndTransmit
    {
        private Thread threadConexao = null;
        private Thread threadPrincipal = null;
        private Thread threadHeartBeat = null;
        private ModbusClient modbusClient = null;
        private static Semaphore transmissionSemaphore;
        private ModBusRegistry modbusRegistry;


        private static readonly ILog logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ILog LOGGER
        {
            get
            {
                return logger;
            }
        }

        private int registroInicialHolding = 0;

        private ProcessIcon processIcon;

        public ProcessIcon ProcessIcon
        {
            get
            {
                return processIcon;
            }
            set
            {
                processIcon = value;
            }            
        }

        public Thread ThreadConexao
        {
            get
            {
                return threadConexao;
            }
            set
            {
                threadConexao = value;
            }
        }

        public Thread ThreadPrincipal
        {
            get
            {
                return threadPrincipal;
            }
            set
            {
                threadPrincipal = value;
            }
        }

        public Thread ThreadHeartBeat
        {
            get
            {
                return threadHeartBeat;
            }
            set
            {
                threadHeartBeat = value;
            }
        }
        private Boolean databaseCheckerShouldRun = true;

        public Boolean DatabaseCheckerShouldRun
        {
            get
            {
                return databaseCheckerShouldRun;
            }
            set
            {
                databaseCheckerShouldRun = value;
            }
        }

        private Boolean forcedRestart = false;

        public Boolean ForcedRestart
        {
            get
            {
                return forcedRestart;
            }
            set
            {
                forcedRestart = value;
            }
        }

        public CheckAndTransmit()
        {
            modbusRegistry = new ModBusRegistry();
            modbusRegistry.LOGGER = logger;
        }

        public void iniciarThreadConexao()
        {
            // Iniciando/Reiniciando travas.
            transmissionSemaphore = new Semaphore(1, 1);

            string enderecoModBus = modbusRegistry.obterModBusEndereco();
            int portaModBus = modbusRegistry.obterModBusPorta();

            ThreadConexao = new Thread(() =>
            {
                bool conectado = false;

                databaseCheckerShouldRun = true;

                while (conectado == false && DatabaseCheckerShouldRun)
                {
                    try
                    {
                        modbusClient = new ModbusClient(enderecoModBus, portaModBus);    //Ip-Address and Port of Modbus-TCP-Server
                        modbusClient.Connect();                                          //Connect to Server
                        conectado = true;
                        ProcessIcon.ChangeIcon((int)ProcessIcon.ICONS.Normal);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Erro ao conectar ao servidor: " + ex.Message);
                        logger.Warn("Tentando novamente em 3 segundos...");
                        Thread.Sleep(3000);
                    }
                }

                iniciarThreadPrincipal();
                iniciarThreadHeartBeat();
                ThreadConexao.Join();
            });
            ThreadConexao.Name = "Conexão";
            ThreadConexao.Start();

        }

        private void reiniciarThreadConexao()
        {
            // Ícone sem conexão
            ProcessIcon.ChangeIcon((int)ProcessIcon.ICONS.Red);

            logger.Warn("Retentando conexão com ModBus...");
            // Interrompe as threads de produção dado que a conexão caiu
            pararThreads();
            // Inicia a thread de tentativas de conexão.
            iniciarThreadConexao();
        }

        private void iniciarThreadPrincipal()
        {
            int tempoAtualizacao = modbusRegistry.obterTempoAtualizacao();
            string modbusEndereco = modbusRegistry.obterModBusEndereco();
            int modbusPorta = modbusRegistry.obterModBusPorta();
            registroInicialHolding = modbusRegistry.obterModBusRegistroHoldingInicial();

            // Log an info level message
            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            if (logger.IsInfoEnabled) logger.Info("Aplicação [MCargaModBus] inicializada.");

            logger.Info("Iniciando processo de checagem & transmissão para cada " + tempoAtualizacao + " segundos.");
            ThreadPrincipal = new Thread(() =>
            {
                logger.Info("Principal iniciado!");
                string usuario = modbusRegistry.obterUsuario();
                string senha = modbusRegistry.obterSenha();
                string servidor = modbusRegistry.obterServidor();
                string baseDados = modbusRegistry.obterBaseDados();


                verificarBase(usuario, senha, servidor, baseDados);


                double numberOfSeconds = 0;
                while (DatabaseCheckerShouldRun)
                {
                    // Deve checar a base e transmitir novas informações
                    if (tempoAtualizacao == numberOfSeconds)
                    {
                        logger.Info("Checando base após " + numberOfSeconds + " segundos...");
                        numberOfSeconds = 0;
                        processarChecagem(usuario, senha, servidor, baseDados);
                    }

                    // Espera um segundo e continua 
                    Thread.Sleep(1000);
                    numberOfSeconds++;
                }
            });
            ThreadPrincipal.Name = "Principal";

            ThreadPrincipal.Start();
            //threadPrincipal.Join();
        }

        private void iniciarThreadHeartBeat()
        {
            int registroInicialHeartBeat = modbusRegistry.obterModBusRegistroHeartBeatPosicao();
            int tempoHeartBeart = modbusRegistry.obterModBusTempoHeartBeatMilisegundos();

            string enderecoModBus = modbusRegistry.obterModBusEndereco();
            int portaModBus = modbusRegistry.obterModBusPorta();

            logger.Info("Lendo/Gravando registro de Heart-Beat na posição de Holding do ModBus: " + registroInicialHeartBeat);
            logger.Info("Tempo de atualização do Heart-Beat é de " + tempoHeartBeart + " milisegundos");
            int[] valoresRegistrados = null;
            ThreadHeartBeat = new Thread(() =>
            {
                logger.Info("Heart-Beat iniciado!");
                while (DatabaseCheckerShouldRun)
                {

                    try
                    {
                        transmissionSemaphore.WaitOne();
                        // Processando o heart-beat
                        valoresRegistrados = lerDoisRegistros(registroInicialHeartBeat);
                        if (valoresRegistrados == null)
                        {
                            // Zeramos o contador pois não tem nada registrado lá...
                            valoresRegistrados = new int[2];
                            valoresRegistrados[0] = 0;
                            valoresRegistrados[1] = 0;
                        }
                        // Efetuando a soma...
                        valoresRegistrados = Tools.IEEE754.addOneValue(true, valoresRegistrados[0], valoresRegistrados[1]);
                        escreverDoisRegistros(registroInicialHeartBeat, valoresRegistrados);
                        transmissionSemaphore.Release();
                    }
                    catch (Exception ex)
                    {
                        // Restart application
                        transmissionSemaphore.Release();
                        reiniciarThreadConexao();
                    }

                    // Aguardando para o próximo...
                    Thread.Sleep(tempoHeartBeart);
                }
            });

            ThreadHeartBeat.Name = "Heart-Beat";

            ThreadHeartBeat.Start();
        }



        private void reiniciarAplicacao()
        {
            ForcedRestart = true;
            logger.Error("Conexão encerrada com a CLP - Reiniciando novas tentativas");
            // Get the parameters/arguments passed to program if any
            string arguments = string.Empty;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++) // args[0] is always exe path/filename
                arguments += args[i] + " ";

            // Restart current application, with same arguments/parameters
            pararThreads();
            Application.Exit();
            System.Diagnostics.Process.Start(Application.ExecutablePath, arguments);
        }

        private void verificarBase(string usuario, string senha, string servidor, string baseDados)
        {
            MySQLClient mySQLClient = new MySQLClient(servidor, baseDados, usuario, senha);
            if (!mySQLClient.DatabaseExists())
            {
                logger.Warn(ThreadPrincipal.Name + " - base inexistente, tentando criar nova base.");
                if (mySQLClient.CreateDatabase())
                {
                    logger.Info(ThreadPrincipal.Name + " - base criada com sucesso.");
                    if (mySQLClient.FillUpDatabase())
                    {
                        logger.Info(threadPrincipal.Name + " - base preenchida com sucesso.");
                    }
                    else
                    {
                        logger.Error(threadPrincipal.Name + " - Problema ao preencher a base de dados");
                    }
                }
                else
                {
                    logger.Error(ThreadPrincipal.Name + " - Problema ao criar base.");
                }
            }
            else
            {
                logger.Info(ThreadPrincipal.Name + " - base encontrada, utilizando a existente.");
            }
        }

        private void processarChecagem(string usuario, string senha, string servidor, string baseDados)
        {
            MySQLClient mySQLClient = new MySQLClient(servidor, baseDados, usuario, senha);

            List<int> temperaturas = new List<int>();

            DataTable dataTable = mySQLClient.Select("BancoCircuitoMaxTemp", "BancoNumero>0");
            string rowData = "[";
            double valueDouble;
            float value;
            foreach (DataRow row in dataTable.Rows)
            {
                // obter a temperatura
                rowData += row[1];
                if (Double.TryParse(row[1].ToString(), out valueDouble))
                {
                    value = (float)valueDouble;
                    if (float.IsPositiveInfinity(value))
                    {
                        value = float.MaxValue;
                    }
                    else if (float.IsNegativeInfinity(value))
                    {
                        value = float.MinValue;
                    }

                    int[] values = Tools.IEEE754.GetBytesSingle(true, value);

                    temperaturas.Add(values[0]);
                    temperaturas.Add(values[1]);
                }
                // Não é o último
                if (dataTable.Rows[dataTable.Rows.Count - 1]!=row)
                {
                    rowData += "|";
                }
            }

            rowData += "]";
            if (temperaturas.Count > 0)
                escreverModBus(temperaturas);

            logger.Info("Temperaturas Acessadas da base: " + rowData);

        }

        private int[] lerDoisRegistros(int posicaoDoPrimeiro)
        {
            int[] registros = null;
            try
            {
                //modbusClient.WriteMultipleCoils(4, new bool[] { true, true, true, true, true, true, true, true, true, true });    //Write Coils starting with Address 5
                //bool[] readCoils = modbusClient.ReadCoils(9, 10);                        //Read 10 Coils from Server, starting with address 10
                registros = modbusClient.ReadHoldingRegisters(posicaoDoPrimeiro-1, 2);    //Read 2 Holding Registers from Server, starting with Address passed by a parameter
            }
            catch (Exception ex)
            {
                logger.Error("Não foi possível acessar o registro do Heart-Beat: " + ex.ToString());
                throw ex;
            }
            return registros;
        }

        private void escreverDoisRegistros(int posicaoDoPrimeiro, int[] registros)
        {

            try
            {
                //modbusClient.WriteMultipleCoils(4, new bool[] { true, true, true, true, true, true, true, true, true, true });    //Write Coils starting with Address 5
                //bool[] readCoils = modbusClient.ReadCoils(9, 10);                        //Read 10 Coils from Server, starting with address 10
                //int[] readHoldingRegisters = modbusClient.ReadHoldingRegisters(0, 10);    //Read 10 Holding Registers from Server, starting with Address 1
                modbusClient.WriteMultipleRegisters(posicaoDoPrimeiro-1, registros.ToArray());
            }
            catch (Exception ex)
            {
                logger.Error("Erro ao escrever o Heart-Beat: " + ex.ToString());
                throw ex;
            }
        }

        private void escreverModBus(List<int> listaTemperaturas) 
        {
            try
            {
                //modbusClient.WriteMultipleCoils(4, new bool[] { true, true, true, true, true, true, true, true, true, true });    //Write Coils starting with Address 5
                //bool[] readCoils = modbusClient.ReadCoils(9, 10);                        //Read 10 Coils from Server, starting with address 10
                //int[] readHoldingRegisters = modbusClient.ReadHoldingRegisters(0, 10);    //Read 10 Holding Registers from Server, starting with Address 1
                transmissionSemaphore.WaitOne();
                modbusClient.WriteMultipleRegisters(registroInicialHolding-1, listaTemperaturas.ToArray());
                transmissionSemaphore.Release();
            }
            catch (Exception ex)
            {
                logger.Error("Erro ao acessar o ModBus: " + ex.ToString());
                reiniciarThreadConexao();
            }

            // Console Output
            /*
            for (int i = 0; i < readCoils.Length; i++)
                Console.WriteLine("Value of Coil " + (9 + i + 1) + " " + readCoils[i].ToString());

            for (int i = 0; i < readHoldingRegisters.Length; i++)
                Console.WriteLine("Value of HoldingRegister " + (i + 1) + " " + readHoldingRegisters[i].ToString());
            modbusClient.Disconnect();                                                //Disconnect from Server
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
             */

        }

        public bool isRunning()
        {
            bool isOnTheRun = ThreadPrincipal.IsAlive || ThreadHeartBeat.IsAlive;

            return isOnTheRun;
        }

        public void pararThreads()
        {
            DatabaseCheckerShouldRun = false;
            logger.Warn("**** Interrompendo threads de produção/reconexão ****");
            Thread.Sleep(2000);

            // Limpa o Thread de Conexao
            if (ThreadConexao != null)
            {
                try
                {
                    if (ThreadConexao.IsAlive)
                    {
                        ThreadConexao.Abort();
                        //ThreadConexao = null;
                        logger.Fatal(ThreadConexao.Name + " exterminada de forma forçada");
                    }
                    else
                    {
                        ThreadConexao.Join();
                        //ThreadConexao = null;
                        logger.Info(ThreadConexao.Name + " terminada normalmente.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Erro ao interromper o thread " + ThreadConexao.Name + ": " + ex.Message);
                }
            }

            // Derruba a conexão se tiver...
            if (modbusClient != null)
            {

                try
                {
                    modbusClient.Disconnect();
                }
                catch (Exception ex)
                {
                    logger.Error("Erro ao desconectar do servidor: " + ex.Message);
                }
            }

            // Derruba o Thread Principal se estiver rodando...
            if (ThreadPrincipal != null)
            {
                try
                {
                    // Tenta parar a thread por bem.
                    if (ThreadPrincipal.IsAlive)
                    {// Senão para por mal...
                        ThreadPrincipal.Abort();
                        //ThreadPrincipal = null;
                        logger.Fatal(ThreadPrincipal.Name + " exterminada de forma forçada");
                    }
                    else
                    {
                        ThreadPrincipal.Join();
                        //ThreadPrincipal = null;
                        logger.Info(ThreadPrincipal.Name + " terminada normalmente.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Erro ao interromper o thread " + ThreadPrincipal.Name + ": " + ex.Message);
                }
            }

            // Derruba o Thread de Heartbeat se estiver rodando...
            if (ThreadHeartBeat != null)
            {
                try
                {

                    if (ThreadHeartBeat.IsAlive)
                    {
                        ThreadHeartBeat.Abort();
                        //ThreadHeartBeat = null;
                        logger.Fatal(ThreadHeartBeat.Name + " exterminada de forma forçada");
                    }
                    else
                    {
                        ThreadHeartBeat.Join();
                        //ThreadHeartBeat = null;
                        logger.Info(ThreadHeartBeat.Name + " terminada normalmente.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Erro ao interromper o thread " + ThreadHeartBeat.Name + ": " + ex.Message);
                }
            }
        }
    }



}
