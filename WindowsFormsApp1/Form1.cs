/* Name:    Jovany Romo
 * Date:    1/26/2021
 * Summary: Application that should allow the user to make changes to a phone database.
 */
using System;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class frmPhoneDB : Form
    {
        SqlConnection phoneConnection;
        SqlCommand phoneCommand;
        SqlDataAdapter phoneAdapter;
        DataTable phoneTable;
        CurrencyManager phoneManager;
        string myState;
        int myBookmark;
        public frmPhoneDB()
        {
            InitializeComponent();
        }

        private void frmPhoneDB_Load(object sender, EventArgs e)
        {
            SetState("Connect");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            // Method of opening the database file needed for the program to function.
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //Sets the inital Directory to automatically open to the database folder.
                string path = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
                path = System.IO.Path.GetDirectoryName(path);
                path = path + @"\Databases";
                openFileDialog.InitialDirectory = path;
                openFileDialog.Filter = "mdf files (*.mdf)|*.mdf";
                openFileDialog.FilterIndex = 2;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                    phoneConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;" +
                        "AttachDbFilename=" + filePath + ";" +
                        "Integrated Security=True;" +
                        "Connect Timeout=30;");
                    phoneConnection.Open();

                    phoneCommand = new SqlCommand("SELECT * " +
                        "FROM phoneTable " +
                        "ORDER BY ContactName",
                        phoneConnection);

                    phoneAdapter = new SqlDataAdapter();
                    phoneAdapter.SelectCommand = phoneCommand;
                    phoneTable = new DataTable();
                    phoneAdapter.Fill(phoneTable);

                    txtID.DataBindings.Add("Text", phoneTable, "ContactID");
                    txtName.DataBindings.Add("Text", phoneTable, "ContactName");
                    txtNumber.DataBindings.Add("Text", phoneTable, "ContactNumber");

                    phoneManager = (CurrencyManager)
                        this.BindingContext[phoneTable];
                    
                    SetState("View");
                }
                else
                {
                    string Message = "Please select a valid file to open.",
                        Title = "Error!";

                    MessageBox.Show(Message,
                        Title,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            
        }

        private void frmPhoneDB_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SqlCommandBuilder phoneAdapterCommands = new SqlCommandBuilder(phoneAdapter);
                phoneAdapter.Update(phoneTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving database to file:\r\n" +
                    ex.Message,
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            phoneConnection.Close();

            phoneConnection.Dispose();
            phoneCommand.Dispose();
            phoneAdapter.Dispose();
            phoneTable.Dispose();
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            phoneManager.Position = 0;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            phoneManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            phoneManager.Position++;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            phoneManager.Position = phoneManager.Count - 1;
        }
        private void SetState(string appState)
        {
            myState = appState;
            switch (appState)
            {
                case "Connect":
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnConnect.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAdd.Enabled = false;
                    txtName.ReadOnly = false;
                    txtNumber.ReadOnly = false;
                    break;
                case "View":
                    btnFirst.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                    btnEdit.Enabled = true;
                    btnConnect.Enabled = false;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAdd.Enabled = true;
                    txtID.BackColor = Color.White;
                    txtID.ForeColor = Color.Black;
                    txtName.ReadOnly = true;
                    txtNumber.ReadOnly = true;
                    break;
                default:
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnConnect.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAdd.Enabled = false;
                    txtID.BackColor = Color.Red;
                    txtID.ForeColor = Color.White;
                    txtName.ReadOnly = false;
                    txtNumber.ReadOnly = false;
                    break;
            }
            txtName.Focus();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            phoneManager.EndCurrentEdit();
            phoneTable.DefaultView.Sort = "ContactName";
            SetState("View");

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            phoneManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
                phoneManager.Position = myBookmark;
            SetState("View");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            myBookmark = phoneManager.Position;
            SetState("Add");
            phoneManager.AddNew();
        }
    }
}
