using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KareBulmaca
{
    public partial class Form1 : Form
    {

        SqlConnection sqlBaglanti = new SqlConnection(@"Data Source=HP\ZDB2100005899;Initial Catalog=KareBulmaca;Integrated Security=True");
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                sqlBaglanti.Open();
                string sqlsorgusu = "INSERT INTO Kelimeler VALUES ('" + textBox1.Text + "')";
                SqlCommand sqlCommand = new SqlCommand(sqlsorgusu, sqlBaglanti);
                sqlCommand.ExecuteNonQuery();
                MessageBox.Show("Kelime veritabanına eklendi.");
                textBox1.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantısında bir hata oluştu. biraz sonra tekrar deneyin.\n" +
                    ex.Message);
            }
            finally
            {
                if (sqlBaglanti != null)
                    sqlBaglanti.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }
    }
}
