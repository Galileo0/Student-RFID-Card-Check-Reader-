using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.IO.Ports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;

namespace check_activation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        delegate void SetTextCallback(string text);
        SqlConnection con = new SqlConnection(ConfigurationSettings.AppSettings["Connection"]);
        SerialPort myport = new SerialPort();
        private void button1_Click(object sender, EventArgs e)
        {
            label20.Text = "";
            listBox1.Items.Clear();
            SqlCommand cmd = new SqlCommand();
            SqlCommand cmd2 = new SqlCommand();
            string q = "select EnrollNumber,CardNumber,CreateDate FROM [Attendance].[dbo].[StudentsFingerPrints] where EnrollNumber = @id"; // to get card number
            string q2 = "select * from  [Attendance].[dbo].[Attendance] inner join Settings on [Attendance].acdyear = Settings.EducationalYear and Attendance.semester = Settings.Semester where stu_id = @stu";
            cmd = new SqlCommand(q,con);
            cmd2 = new SqlCommand(q2, con);
            cmd.Connection = con;
            cmd2.Connection = con;
            cmd.Parameters.AddWithValue("@id",textBox1.Text.ToString());
            string temp_stu = textBox1.Text.ToString() + '.';
            cmd2.Parameters.AddWithValue("@stu", temp_stu);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            int c = 0;
            int c2 = 0;
            int flag = 0;
            try
            {
                string id, card,createdate;
                
                while (reader.Read())
                {
                    c = 0;
                    id = reader["EnrollNumber"].ToString();
                    card = reader["CardNumber"].ToString();
                    createdate = reader["CreateDate"].ToString();
                    if (id != "" && card != "")
                    {
                        label5.Text = id;
                        label6.ForeColor = Color.Green;
                        label6.Text = card;
                        label7.ForeColor = Color.Green;
                        label7.Text = "Found";
                        label22.Text = createdate;
                        c++;
                        flag = 1;
                    }
                   
                    
                    break;
                }
                
            }
            finally
            {
                // Always call Close when done reading.
                reader.Close();
            }

            // check registeration
            SqlDataReader reader2 = cmd2.ExecuteReader();
            try
            {
                string name,fac;
                while (reader2.Read())
                {
                    c2 = 0;
                    name = reader2["stu_name"].ToString();
                    fac = reader2["fac_name"].ToString();
                    if (name != "" && fac != "")
                    {
                        label9.Text = name;
                        
                        label11.Text = fac;
                        label9.ForeColor = Color.Green;
                        label11.ForeColor = Color.Green;
                        label13.ForeColor = Color.Green;
                        label13.Text = "True";
                        c2++;
                        flag = 1;
                    }


                    break;
                }
            }
            finally
            {
                // Always call Close when done reading.
                reader2.Close();
            }
           

            if (c == 0)
            {
                label5.Text = textBox1.Text.ToString();
                label6.ForeColor = Color.Red;
                label7.ForeColor = Color.Red;
                label6.Text = "Not Found";
                label7.Text = "Not Found";
                label22.Text = "";
            }

            if (c2 == 0)
            {
                
                label9.ForeColor = Color.Red;
                label11.ForeColor = Color.Red;
                label13.ForeColor = Color.Red;
                label9.Text = "Not Registred";
                label11.Text = "Not Registred";
                label13.Text = "False";
            }
            con.Close();


            if (label13.Text.ToString() == "True" && label7.Text.ToString() == "Found")
            {
                //get machines
                con.Open();
                DataSet MachinesDataSet = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter("sp_Machines_GetAll_new", con);
                da.Fill(MachinesDataSet);
                foreach (DataRow R in MachinesDataSet.Tables[0].Rows)
                {
                    string IP = R["ip"].ToString();
                    string Machine_name = R["PlaceName"].ToString();
                    SqlCommand cmd3 = new SqlCommand();
                    cmd3 = new SqlCommand("sp_Machines_check_student", con);
                    cmd3.Parameters.Clear();
                    cmd3.Parameters.Add("@IP", SqlDbType.VarChar).Value = IP;
                    //cmd.Parameters.Add("EnrollNumber", SqlDbType.Int).Value = textBox2.Text.ToString();
                    cmd3.Parameters.Add("@ID", SqlDbType.Int).Value = textBox1.Text.ToString();
                    cmd3.CommandType = CommandType.StoredProcedure;
                    SqlDataReader reader3 = cmd3.ExecuteReader();
                    int flag2 = 0;
                    while (reader3.Read())
                    {
                        flag2 = 0;
                        string tmp_id = reader3["EnrollNumber"].ToString();
                        if (tmp_id == textBox1.Text.ToString())
                        {
                            string msg = Machine_name + " : Found ";
                            listBox1.ForeColor = Color.Green;
                            listBox1.Items.Add(msg);
                           // listBox1.Items.
                            
                            flag2 = 1;
                        }
                       
                        break;

                    }

                    if (flag2 == 0)
                    {
                        string msg = Machine_name + " : Not Found ";
                        //listBox1.ForeColor = Color.Red;
                        //listBox1.Items.Add(msg);
                    }


                    reader3.Close();
                }
                con.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(textBox2.Text.ToString() != "" && textBox3.Text.ToString() != "" )
            {
                int EnrollNumber = int.Parse(textBox2.Text.ToString());
                int card = int.Parse(textBox3.Text.ToString());
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd = new SqlCommand("sp_StudentsFingerPrints_Add", con);
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@EnrollNumber", SqlDbType.Int).Value = EnrollNumber;
                    //cmd.Parameters.Add("EnrollNumber", SqlDbType.Int).Value = textBox2.Text.ToString();
                    cmd.Parameters.Add("FingerIndex", SqlDbType.Int).Value = 11;
                    cmd.Parameters.Add("FingerPrint", SqlDbType.NText).Value = "";
                    cmd.Parameters.Add("CardNumber", SqlDbType.Int).Value = card;
                    cmd.Parameters.Add("Password", SqlDbType.NVarChar).Value = "";
                    cmd.Parameters.Add("IsAdmin", SqlDbType.BigInt).Value = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                    label17.ForeColor = Color.Green;
                    label17.Text = "Done";
                }
                catch (Exception)
                {

                    label17.ForeColor = Color.Red;
                    label17.Text = "Error : Check inputs and Contact IT";
                }
                
            }

            con.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                listBox2.Items.Add(port);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            

            
            string data = myport.ReadLine();
            textBox3.Text = data;
            //myport.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            myport = new SerialPort();
            myport.BaudRate = 9600;
            myport.PortName = listBox2.SelectedItem.ToString();
            myport.Open();
            //timer1.Start();
            
            Thread demoThread = new Thread(new ThreadStart(this.ThreadProcUnsafe));

            demoThread.Start();
            // backgroundWorker1.RunWorkerAsync();
            /* new Thread(() =>
             {
                 while (true)
                 {
                     //int data = int.Parse(myport.ReadLine().ToString());
                     //textBox4.Text = data.ToString();
                     textBox4.Text = "4";
                     //other tasks
                 }
             }).Start();*/
            /* while (true)
             {
                 if (backgroundWorker1.IsBusy == false)
                 {
                     backgroundWorker1.RunWorkerAsync();
                 }

                 int data = int.Parse(myport.ReadLine().ToString());
                 textBox4.Text = data.ToString();
             }*/
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private void ThreadProcUnsafe()
        {
            while (true)
            {
                string data = myport.ReadLine().ToString();
                //string[] hexValuesSplit = data.Split(' ');
                // string temp = "";
                int num = Int32.Parse(data, System.Globalization.NumberStyles.HexNumber);
                /*foreach (string hex in hexValuesSplit)
                {
                    int value = Convert.ToInt32(hex, 16);
                    string stringValue = Char.ConvertFromUtf32(value);
                    temp += stringValue;
                }
                   */
                // int intValue = Convert.ToInt32(data, 16);

                //int.TryParse(data, out temp);

                //textBox4.Text = data.ToString();
                //textBox4.Text = "4";
                //other tasks
                this.SetText(num.ToString());
            }
            
        }
        public static byte[] FromHex(string hex)
        {
            var result = new byte[hex.Length / 2];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox3.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                try
                {
                    this.textBox3.Text = text;
                    this.label20.Text = text;

                    //this.label18.Text = text;
                }
                catch (Exception)
                {

                    throw;
                }
               
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label6.Text = "";
            label20.Text = "";
            label7.Text = "";
            label9.Text = "";
            label11.Text = "";
            label13.Text = "";
            label5.Text = "";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = "";
            label16.Text = "";
            label17.Text = "";
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2.PerformClick();
            }
        }
    }
}
