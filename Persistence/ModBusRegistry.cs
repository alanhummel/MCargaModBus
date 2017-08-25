using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;

namespace MCargaModBus.Persistence
{
    class ModBusRegistry
    {
        private ILog logger;
        public ILog LOGGER
        {
            get
            {
                return logger;
            }
            set
            {
                logger = value;
            }
        }

        public int obterTempoAtualizacao()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string tempoAtualizacaoChave = "tempoAtualizacao";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(tempoAtualizacaoChave);

            if (valor == null)
            {
                valor = "60";
                manipulateRegistry.Write(tempoAtualizacaoChave, valor);
                logger.Warn("Não foi possível o tempo de atualização do registro, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Tempo de atualizacao obtido do registro:" + valor);
            }

            int tempoAtualizacao = 60;
            Int32.TryParse(valor, out tempoAtualizacao);

            return tempoAtualizacao;
        }

        public string obterUsuario()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string nomeUsuario = "nomeUsuario";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(nomeUsuario);

            if (valor == null)
            {
                valor = "root";
                manipulateRegistry.Write(nomeUsuario, valor);
            }

            return valor;
        }

        public string obterSenha()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string valorSenha = "valorSenha";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(valorSenha);

            if (valor == null)
            {
                valor = "mcarga";
                manipulateRegistry.Write(valorSenha, valor);
            }

            return valor;
        }

        public string obterServidor()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string servidor = "Servidor";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(servidor);

            if (valor == null)
            {
                valor = "localhost";
                manipulateRegistry.Write(servidor, valor);
                logger.Warn("Não foi possível obter o endereco do ModBus do registro, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Endereço do ModBus obtido do registro:" + valor);
            }


            return valor;
        }

        public string obterBaseDados()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string baseDados = "Base";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(baseDados);
            if (valor == null)
            {
                valor = "MCarga";
                manipulateRegistry.Write(baseDados, valor);
                logger.Warn("Não foi possível obter o banco de dados do registro, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Banco de dados obtido do registro:" + valor);
            }

            return valor;
        }

        public string obterModBusEndereco()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string baseDados = "ModBusEndereco";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(baseDados);

            if (valor == null)
            {
                valor = "127.0.0.1";
                manipulateRegistry.Write(baseDados, valor);
                logger.Warn("Não foi possível obter o endereço ModBus, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Banco de dados obtido do registro:" + valor);
            }

            return valor;
        }

        public int obterModBusPorta()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string modbusPortaChave = "ModBusPorta";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(modbusPortaChave);

            if (valor == null)
            {
                valor = "502";
                manipulateRegistry.Write(modbusPortaChave, valor);
                logger.Warn("Não foi possível obter a porta do ModBus, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Porta do banco de dados obtida do registro: " + valor);
            }

            int modbusPorta = 502;
            Int32.TryParse(valor, out modbusPorta);

            return modbusPorta;
        }

        public int obterModBusRegistroHoldingInicial()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string registroHoldingInic = "RegistroHoldingInicial";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(registroHoldingInic);

            if (valor == null)
            {
                valor = "40001";
                manipulateRegistry.Write(registroHoldingInic, valor);
                logger.Warn("Não foi possível obter a porta do ModBus, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Registro de Holding inicial obtida do banco de dados obtida do registro: " + valor);
            }

            int registroInicial = 40001;
            Int32.TryParse(valor, out registroInicial);

            return registroInicial;
        }

        public int obterModBusRegistroHeartBeatPosicao()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string registroHeartBeatPosicaoInicial = "RegistroHeartBeatPosicaoInicial";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(registroHeartBeatPosicaoInicial);

            if (valor == null)
            {
                valor = "40061";
                manipulateRegistry.Write(registroHeartBeatPosicaoInicial, valor);
                logger.Warn("Não foi possível obter a posição inicial do registro de Heart-Beat do ModBus, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Registro de Heart-Beat inicial obtida do banco de dados obtida do registro: " + valor);
            }

            int registroInicial = 40061;
            Int32.TryParse(valor, out registroInicial);

            return registroInicial;
        }

        public int obterModBusTempoHeartBeatMilisegundos()
        {
            string nomeSoftware = "MCargaModBus";
            string valor = null;
            string heartBeatTempoMilisegundos = "HeartBeatTempoMilisegundos";

            ManipulateRegistry manipulateRegistry = new ManipulateRegistry();
            manipulateRegistry.SubKey = "SOFTWARE\\HRM\\" + nomeSoftware;
            valor = manipulateRegistry.Read(heartBeatTempoMilisegundos);

            if (valor == null)
            {
                valor = "500";
                manipulateRegistry.Write(heartBeatTempoMilisegundos, valor);
                logger.Warn("Não foi possível obter a posição inicial do registro de Heart-Beat do ModBus, tentando criar novo:" + valor);
            }
            else
            {
                logger.Info("Registro de Heart-Beat inicial obtida do banco de dados obtida do registro: " + valor);
            }

            int registroInicial = 500;
            Int32.TryParse(valor, out registroInicial);

            return registroInicial;
        }
    }
}
