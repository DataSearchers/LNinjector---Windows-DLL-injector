using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace LNjector
{
    public partial class Form1 : Form
    {
        // Import necessary Windows API functions
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint size, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        private string pidstr;
        public Form1()
        {
            InitializeComponent();
            
            this.Text = Guid.NewGuid().ToString(); // generate new name so the app does not have always the same name
            this.Name = Guid.NewGuid().ToString(); // generate new name so the app does not have always the same name
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox1.SelectedItem.ToString();
            int startIndex = selectedItem.LastIndexOf('(') + 1;
            int endIndex = selectedItem.LastIndexOf(')');
            string pid = selectedItem.Substring(startIndex, endIndex - startIndex);
            pidstr = pid;
            // richTextBox1.AppendText($"Selected PID: {pidstr}");
        }

        public void InjectDll(int processId, string dllPath)
        {
            // Open the target process
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                richTextBox1.AppendText("Failed to open process." + Environment.NewLine);
                return;
            }

            // Allocate memory in the target process for the DLL path
            IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (allocMemAddress == IntPtr.Zero)
            {
                richTextBox1.AppendText("Failed to allocate memory." + Environment.NewLine);
                CloseHandle(hProcess);
                return;
            }

            // Write the DLL path into the allocated memory
            byte[] dllPathBytes = System.Text.Encoding.ASCII.GetBytes(dllPath);
            if (!WriteProcessMemory(hProcess, allocMemAddress, dllPathBytes, (uint)dllPathBytes.Length, out _))
            {
                richTextBox1.AppendText("Failed to write process memory." + Environment.NewLine);
                CloseHandle(hProcess);
                return;
            }

            // Get the address of LoadLibraryA
            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            // Create a remote thread to execute LoadLibraryA
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, out _);
            if (hThread == IntPtr.Zero)
            {
                richTextBox1.AppendText("Failed to create remote thread." + Environment.NewLine);
            }

            richTextBox1.AppendText("Successfully injected the DLL." + Environment.NewLine);

            // Clean up
            CloseHandle(hThread);
            CloseHandle(hProcess);
        }


        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);


        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            LoadProcesses();
        }
        private void LoadProcesses()
        {
            comboBox1.Items.Clear();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                string displayString = $"{process.ProcessName} ({process.Id})";
                comboBox1.Items.Add(displayString);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";
                openFileDialog.Title = "Select a DLL File";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string selectedFile in openFileDialog.FileNames)
                    {
                        if (!IsFileAlreadyInListView(selectedFile))
                        {
                            ListViewItem item = new ListViewItem(Path.GetFileName(selectedFile));
                            item.SubItems.Add(selectedFile);
                            listView1.Items.Add(item);
                        }
                        else
                        {
                            richTextBox1.AppendText($"The file {Path.GetFileName(selectedFile)} is already in the list." + Environment.NewLine);
                        }

                    }
                }
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";
                openFileDialog.Title = "Select a DLL File";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string selectedFile in openFileDialog.FileNames)
                    {
                        if (!IsFileAlreadyInListView(selectedFile))
                        {
                            ListViewItem item = new ListViewItem(Path.GetFileName(selectedFile));
                            item.SubItems.Add(selectedFile);
                            listView1.Items.Add(item);
                        }
                        else
                        {
                            richTextBox1.AppendText($"The file {Path.GetFileName(selectedFile)} is already in the list." + Environment.NewLine);
                        }

                    }
                }
            }
        }
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selectedItem in listView1.SelectedItems)
            {
                listView1.Items.Remove(selectedItem);
            }
        }
        private bool IsFileAlreadyInListView(string filePath)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.SubItems[1].Text.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(pidstr, out int pid))
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    string dllPath = item.SubItems[1].Text;

                    try
                    {
                        InjectDll(pid, dllPath);
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.AppendText($"Failed to inject {dllPath}: {ex.Message}" + Environment.NewLine);
                    }
                }
            }
            else
            {
                richTextBox1.AppendText("Please enter a valid PID." + Environment.NewLine);
            }
        }
    }
}
