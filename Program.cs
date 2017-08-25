using System;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;


namespace MCargaModBus
{
	/// <summary>
	/// 
	/// </summary>
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var appGuid = attribute.Value;

            using(Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                bool interfaceStarted = false;
			    // Show the system tray icon.					
                using (ProcessIcon pi = new ProcessIcon())
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);


                    if (!mutex.WaitOne(0, false))
                    {
                        MessageBox.Show("Programa já em execução!");
                        return;
                    }

                    CheckAndTransmit checkAndTransmit = null;
                    try
                    {
                        checkAndTransmit = new CheckAndTransmit();
                        checkAndTransmit.ProcessIcon = pi;
                        pi.Display(checkAndTransmit);
                        pi.ChangeIcon((int)ProcessIcon.ICONS.Red);
                        checkAndTransmit.iniciarThreadConexao();
                        // Make sure the application runs!
                        Application.Run();
                    }
                    catch (Exception ex)
                    {
                        checkAndTransmit.LOGGER.Fatal("************* Não foi possível iniciar o processo *************");
                        checkAndTransmit.LOGGER.Fatal(ex.Message);
                    }

                }
			}
		}
	}
}