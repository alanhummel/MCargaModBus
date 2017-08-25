using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Finisar.SQLite;
using System.IO;


namespace MCargaModBus.Persistence
{
    class SQLiteClient
    {
        // Objetos necessários:
        private SQLiteConnection sqlite_conn;
        private SQLiteCommand sqlite_cmd;
        private Boolean baseAberta = false;
        private Boolean BaseAberta
        {
            set { this.baseAberta = value; }
            get { return this.baseAberta; }
        }
        private SQLiteDataReader sqlite_datareader;
        private String dataBasePathFileName = ".\\Config.db";
        public String DataBasePathFileName
        {
            set { this.dataBasePathFileName = value; }
            get { return this.dataBasePathFileName; }
        }

        public bool verificarExistenciaBase()
        {
            return File.Exists(dataBasePathFileName);
        }

        public void CriarBase(string dbPathAndFileName)
        {
            // cria uma nova conexão com o banco de dados:
            sqlite_conn = new SQLiteConnection("Data Source=" + dataBasePathFileName + ";Version=3;New=True;Compress=True;");
            // abre a conexão:
            sqlite_conn.Open();
            this.BaseAberta = true;
        }

        public void AbrirBase(string dbPathAndFileName)
        {
            // cria uma nova conexão com o banco de dados:
            sqlite_conn = new SQLiteConnection("Data Source=" + dataBasePathFileName + ";Version=3;New=False;Compress=True;");
            // abre a conexão:
            sqlite_conn.Open();
            this.BaseAberta = true;
        }

        public void FecharBase()
        {
            if (BaseAberta)
                sqlite_conn.Close();
        }

        public void ExecutarNaoQuery(string comando)
        {
            if (BaseAberta)
            {
                // cria um novo comando SQL:
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = comando;
                sqlite_cmd.ExecuteNonQuery();
            }
            else
                throw new Exception("Base não aberta.");
        }


        public SQLiteDataReader ExecutarQuery(string comando)
        {
            if (BaseAberta)
            {
                // cria um novo comando SQL:
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = comando;
                // Agora o objeto SQLiteCommand pode nos oferecer um objeto DataReader:
                sqlite_datareader = sqlite_cmd.ExecuteReader();
                return sqlite_datareader;
            }
            else
                throw new Exception("Base não aberta.");
        }

        public void CriarConfiguracoes()
        {
            // cria uma nova conexão com o banco de dados:
            sqlite_conn = new SQLiteConnection("Data Source=" + dataBasePathFileName + ";Version=3;New=True;Compress=True;");


            // abre a conexão:
            sqlite_conn.Open();

            // cria um novo comando SQL:
            sqlite_cmd = sqlite_conn.CreateCommand();
            // Tabela Configuracoes:
            sqlite_cmd.CommandText = "CREATE TABLE Configuracoes (" +
                                                                "Chave varchar(255) primary key," +
                                                                "Valor varchar(255) " +
                                                            ");";

            sqlite_cmd.ExecuteNonQuery();

            // Tudo ok. Podemos encerrar e limpar a conexão:
            sqlite_conn.Close();

            InserirChave("EnderecoMSSQL", "");
            InserirChave("PortaMSSQL", "");
            InserirChave("BaseMSSQL", "");
            InserirChave("UsuarioMSSQL", "");
            InserirChave("SenhaMSSQL", "");

            InserirChave("EnderecoMySQL", "");
            InserirChave("PortaMySQL", "");
            InserirChave("BaseMySQL", "");
            InserirChave("UsuarioMySQL", "");
            InserirChave("SenhaMySQL", "");

        }

        public void InserirChave(String chave, String valor)
        {
            AbrirBase(DataBasePathFileName);
            //a ? "yes" : "no"

            ExecutarNaoQuery("INSERT INTO Configuracoes (Chave, Valor) VALUES ('"
                + chave + "', '" + valor + "');");
            FecharBase();
        }

        public void AtualizarChave(String chave, String valor)
        {
            //UPDATE Books SET Author='Lev Nikolayevich Tolstoy' WHERE Id=1;
            AbrirBase(DataBasePathFileName);

            ExecutarNaoQuery("UPDATE Configuracoes SET " +
                        "Valor='" + valor +
                        "' WHERE Chave='" + chave + "';");

            FecharBase();
        }

        public String BuscarChave(String chave)
        {
            String resultado = null;
            SQLiteDataReader dataReader = null;

            AbrirBase(DataBasePathFileName);

            dataReader = ExecutarQuery("SELECT * FROM Configuracoes WHERE Chave='" + chave + "';");
            // O SQLiteDataReader nos permite executar pelas linhas de resultado:
            if (dataReader.Read()) // Read() retorna verdadeiro se há linhas de resultados a serem lidas
            {
                resultado = Convert.ToString(dataReader["Valor"]);
            }

            FecharBase();

            return resultado;
        }

        public void criarBases()
        {
            // cria uma nova conexão com o banco de dados:
            sqlite_conn = new SQLiteConnection("Data Source=" + dataBasePathFileName + ";Version=3;New=True;Compress=True;");

            // abre a conexão:
            sqlite_conn.Open();

            // cria um novo comando SQL:
            sqlite_cmd = sqlite_conn.CreateCommand();

            // Tabela Visitante:
            sqlite_cmd.CommandText = "CREATE TABLE Visitante ( " +
                                                                "id integer primary key, " +
                                                                "Nome varchar(255), " +
                                                                "RG varchar(12), " +
                                                                "Telefone varchar(16) default null, " +
                                                                "Empresa varchar(100) default null, " +
                                                                "Acesso_id integer default null, " +
                                                                "Foto blob default null" +
                                                             ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Acesso:
            sqlite_cmd.CommandText = "CREATE TABLE Acesso (" +
                                                                "id integer primary key, " +
                                                                "Visitante_id integer," +
                                                                "EmpresaVisitada_id integer, " +
                                                                "Bloco_id integer, " +
                                                                "Unidade_id integer, " +
                                                                "Cracha_id integer, " +
                                                                "Responsavel varchar(100) default null, " +
                                                                "Observacao varchar(4096) default null" +
                                                         ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela EmpresaVisitada:
            sqlite_cmd.CommandText = "CREATE TABLE EmpresaVisitada (" +
                                                                        "id integer primary key, " +
                                                                        "Nome varchar(255)" +
                                                                   ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Bloco:
            sqlite_cmd.CommandText = "CREATE TABLE Bloco (" +
                                                            "id integer primary key," +
                                                            " Bloco varchar(255)" +
                                                         ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Unidade:
            sqlite_cmd.CommandText = "CREATE TABLE Unidade (" +
                                                                "id integer primary key, " +
                                                                "Unidade varchar(255)" +
                                                           ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Cracha:
            sqlite_cmd.CommandText = "CREATE TABLE Cracha (" +
                                                                "id integer primary key, " +
                                                                "Acesso_Id integer," +
                                                                "NumeroCracha integer, " +
                                                                "NumeroReduzido integer, " +
                                                                "HorarioLivre integer, " +
                                                                "Bloqueado integer, " +
                                                                "EmUso integer " +
                                                          ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Controle:
            sqlite_cmd.CommandText = "CREATE TABLE Controle (" +
                                                                "id integer primary key, " +
                                                                "Visitante_id integer, " +
                                                                "Acesso_id integer, " +
                                                                "Horario datetime default current_timestamp" +
                                                            ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Catraca:
            sqlite_cmd.CommandText = "CREATE TABLE Catraca (" +
                                                                "id integer primary key, " +
                                                                "NomeTerminal varchar(255), " +
                                                                "Endereco varchar(15), " +
                                                                "Porta integer" +
                                                           ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Catraca:
            sqlite_cmd.CommandText = "CREATE TABLE Parametros (" +
                                                                    "Atributo varchar(255) primary key, " +
                                                                    "Valor varchar(255)" +
                                                              ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tabela Usuarios:
            sqlite_cmd.CommandText = "CREATE TABLE Usuarios (" +
                                                                "id integer primary key," +
                                                                "Nome varchar(255), " +
                                                                "UserName varchar(255), " +
                                                                "Password varchar(255), " +
                                                                "UserType varchar(255), " +
                                                                "Block integer, " +
                                                                "Foto blob default null, " +
                                                                "LastVisitDate datetime default current_timestamp" +
                                                            ");";

            // executando o SQL
            sqlite_cmd.ExecuteNonQuery();

            // Tudo ok. Podemos encerrar e limpar a conexão:
            sqlite_conn.Close();

        }

    }
}
