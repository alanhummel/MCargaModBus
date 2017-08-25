using System;
using System.Diagnostics;
using System.Windows.Forms;
using MCargaModBus.Properties;
using System.Drawing;

namespace MCargaModBus
{
	/// <summary>
	/// 
	/// </summary>
	class ContextMenus
	{
		/// <summary>
		/// Is the About box displayed?
		/// </summary>
		bool isAboutLoaded = false;
        private CheckAndTransmit checkAndTransmit = null;

		/// <summary>
		/// Creates this instance.
		/// </summary>
		/// <returns>ContextMenuStrip</returns>
        public ContextMenuStrip Create(CheckAndTransmit CaT)
		{
			// Add the default menu options.
			ContextMenuStrip menu = new ContextMenuStrip();
			ToolStripMenuItem item;
			ToolStripSeparator sep;
            checkAndTransmit = CaT;
			// Windows Explorer.
            /*
			item = new ToolStripMenuItem();
			item.Text = "Explorer";
			item.Click += new EventHandler(Explorer_Click);
			item.Image = Resources.Explorer;
			menu.Items.Add(item);
            */

			// About.
			item = new ToolStripMenuItem();
			item.Text = "Sobre";
			item.Click += new EventHandler(About_Click);
			item.Image = Resources.About;
			menu.Items.Add(item);

			// Separator.
			sep = new ToolStripSeparator();
			menu.Items.Add(sep);

			// Exit.
			item = new ToolStripMenuItem();
			item.Text = "Sair";
			item.Click += new System.EventHandler(Exit_Click);
			item.Image = Resources.Exit;
			menu.Items.Add(item);

			return menu;
		}

		/// <summary>
		/// Handles the Click event of the Explorer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void Explorer_Click(object sender, EventArgs e)
		{
			Process.Start("explorer", null);
		}

		/// <summary>
		/// Handles the Click event of the About control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void About_Click(object sender, EventArgs e)
		{
			if (!isAboutLoaded)
			{
				isAboutLoaded = true;
				new AboutBox().ShowDialog();
				isAboutLoaded = false;
			}
		}

		/// <summary>
		/// Processes a menu item.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void Exit_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Esta aplicação deve permanecer em execução para continuidade da comunicação com a CLP. Você tem certeza que deseja interromper seu funcionamento?",
                                     "Interrupção do MCarga->ModBus",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                if (checkAndTransmit != null)
                {
                    checkAndTransmit.LOGGER.Warn("******* Usuário requisitou e confirmou a PARADA da aplicação *******");
                    checkAndTransmit.ForcedRestart = false;
                    checkAndTransmit.pararThreads();
                }
                // Quit without further ado.
                Application.Exit();
            }


		}
	}
}