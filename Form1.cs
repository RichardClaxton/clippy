using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipBoardManager
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();


        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private Clippy clippy = new Clippy();

        private Boolean firstSet = false;

        private ContextMenuStrip trayMenu;

        /// <summary>
        /// Form1
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            this.dataGridView1.CellContentClick += new DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            AddClipboardFormatListener(this.Handle);

            // Register the hotkey (e.g., Ctrl + Shift + L) // Last Clip
            RegisterHotKey(this.Handle, Constants.HOTKEY_ID_LAST, Constants.MOD_CONTROL | Constants.MOD_SHIFT, (int)Keys.L);

            // Register the hotkey (e.g., Ctrl + Shift + D) // Delete All Clips
            RegisterHotKey(this.Handle, Constants.HOTKEY_ID_DELETE_ALL, Constants.MOD_CONTROL | Constants.MOD_SHIFT, (int)Keys.D);

            // Register the numbers
            for (int i = 0; i <= 9; i++)
            {
                RegisterHotKey(this.Handle, Constants.HOTKEY_ID + i, Constants.MOD_CONTROL | Constants.MOD_SHIFT, (int)Keys.D0 + i);
            }

            // Initialize Context Menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, OnShowClicked);
            trayMenu.Items.Add("Hide", null, OnHideClicked);
            trayMenu.Items.Add("Exit", null, OnExitClicked);

            // Set Context Menu to Tray Icon
            trayIcon.ContextMenuStrip = trayMenu;

            // Display the Tray Icon
            trayIcon.Visible = true;

            this.Text = "ClipBoardManager v0.1";

        }

        /// <summary>
        /// refereshDataGrid
        /// </summary>
        private void refereshDataGrid()
        {
            dataGridView1.DataSource = (from clip in clippy.Clips
                                        orderby clip.Key
                                        select new
                                        {
                                            Index = clip.Key,
                                            Clip = clip.Value.Data,
                                        }).ToList();

            if (!dataGridView1.Columns.Contains(Constants.DEL_BUTTON))
            {
                addButtons();
            }

            if (firstSet == false)
            {
                dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                firstSet = true;
            }
        }

        /// <summary>
        /// addButton
        /// </summary>
        private void addButtons()
        {
            // Create new button column
            DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
            buttonColumn.Name = Constants.DEL_BUTTON;
            buttonColumn.HeaderText = "Remove";
            buttonColumn.Text = "Remove";
            buttonColumn.UseColumnTextForButtonValue = true;

            dataGridView1.Columns.Add(buttonColumn);
        }


        /// <summary>
        /// Main Windows Messaging Protocol Handler.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardChange();
                Console.WriteLine("Clipboard Message");
            }

            else if (m.Msg == Constants.WM_HOTKEY)
            {
                if (m.WParam == Constants.HOTKEY_ID_LAST)
                {
                    OnHotKeyPressed(Constants.HOTKEY_ID_LAST);
                }
                else if (m.WParam == Constants.HOTKEY_ID_DELETE_ALL)
                {
                    OnHotKeyDeleteAllPressed();
                }
                else
                {
                    var id = m.WParam.ToInt32() - Constants.HOTKEY_ID;
                    OnHotKeyPressed(id);
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// OnClipboardChange
        /// </summary>
        private void OnClipboardChange()
        {
            if (clippy.getNumberOfClips() < Clippy.MAX_CLIPS)
            {
                IDataObject iData = Clipboard.GetDataObject();
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    string clipboardText = (string)iData.GetData(DataFormats.Text);

                    if (!clippy.checkForClip(clipboardText!))
                    {
                        clippy.addClip(clipboardText!);
                        refereshDataGrid();
                    }
                }
            }
            else
            {
                MessageBox.Show("Reached MAX Clips");
            }

        }

        /// <summary>
        /// dataGridView1_CellContentClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the clicked cell is in the button column
            if (e.ColumnIndex == dataGridView1.Columns[Constants.DEL_BUTTON].Index && e.RowIndex >= 0)
            {
                // Your custom logic for button click
                MessageBox.Show($"DelButton clicked in row {e.RowIndex} for Index {dataGridView1.Rows[e.RowIndex].Cells["Index"].Value}");
                clippy.deleteClip(e.RowIndex + 1);
                refereshDataGrid();
            }
        }

        /// <summary>
        /// OnHotKeyPressed
        /// </summary>
        private void OnHotKeyPressed(int index)
        {
            IntPtr activeWindowHandle = GetForegroundWindow();
            var clip = clippy.getClip(index);
            if (clip != null)
            {
                Clipboard.Clear();
                Clipboard.SetText(clip);
                SendKeys.SendWait("^V");
            }
            else
            {
                MessageBox.Show("No Clips");
            }
        }

        /// <summary>
        /// OnHotKeyDeleteAllPressed
        /// </summary>
        private void OnHotKeyDeleteAllPressed()
        {
            MessageBox.Show("DELETE ALL CLIPS");
        }



        /// <summary>
        /// OnFormClosing
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Unregister the hotkey when the form is closing
            RemoveClipboardFormatListener(this.Handle);
            UnregisterHotKey(this.Handle, Constants.HOTKEY_ID);
            UnregisterHotKey(this.Handle, Constants.HOTKEY_ID_DELETE_ALL);

            for (int i = 0; i <= 9; i++)
            {
                UnregisterHotKey(this.Handle, Constants.HOTKEY_ID + i);
            }

            base.OnFormClosing(e);
        }

        /// <summary>
        /// saveAllClipsToolStripMenuItem_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAllClipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clippy.saveClips();
        }

        private void openAllClipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.Rows.Clear();
            }

            clippy.loadClips();
            refereshDataGrid();
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void OnShowClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void OnHideClicked(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            // Close the application
            Application.Exit();
        }


     

    }
}
