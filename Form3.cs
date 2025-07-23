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
    public partial class Form3 : Form
    {

        public Form3()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void BindGrid()
        {
            SqlConnection sqlBaglanti = new SqlConnection(@"Data Source=HP\ZDB2100005899;Initial Catalog=KareBulmaca;Integrated Security=True");
            string sqlsorgusu = "SELECT * FROM Kelimeler";
            {
                SqlCommand sqlCommand = new SqlCommand(sqlsorgusu, sqlBaglanti);
                {
                    SqlDataAdapter sda = new SqlDataAdapter(sqlCommand);
                    {
                        DataTable dt = new DataTable();
                            {
                            sda.Fill(dt);
                            dataGridView1.DataSource = dt;
                            {
                                DataRow yeniSatir = dt.NewRow();
                                dt.Rows.InsertAt(yeniSatir, 0); // 0 → en üst satır
                            }
                            }
                    }
                }
            }
            
        }

        private void dataGridView1_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            var row = dataGridView1.Rows[e.RowIndex];

            if (row.IsNewRow) return;

            var kelimeHücre = row.Cells["Kelime"].Value;
            var idHücre = row.Cells["KelimeID"].Value;

            // Eğer kelime hücresi boş veya null ise ve satır veritabanından geliyorsa -> sil
            if ((kelimeHücre == null || string.IsNullOrWhiteSpace(kelimeHücre.ToString())) && idHücre != DBNull.Value)
            {
                int kelimeId = Convert.ToInt32(idHücre);
                using (SqlConnection conn = new SqlConnection(@"Data Source=HP\ZDB2100005899;Initial Catalog=KareBulmaca;Integrated Security=True"))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Kelimeler WHERE KelimeID = @KelimeID", conn);
                    cmd.Parameters.AddWithValue("@KelimeID", kelimeId);
                    cmd.ExecuteNonQuery();
                }

                // Silme sonrası veriyi yeniden yükle
                this.BeginInvoke((MethodInvoker)delegate {
                    BindGrid();
                });
                return;
            }

            // Kelime doluysa ekleme/güncelleme işlemleri 
            if (kelimeHücre != null && !string.IsNullOrWhiteSpace(kelimeHücre.ToString()))
            {
                string kelime = kelimeHücre.ToString();

                if (e.RowIndex == 0 || idHücre == DBNull.Value)
                {
                    using (SqlConnection conn = new SqlConnection(@"Data Source=HP\ZDB2100005899;Initial Catalog=KareBulmaca;Integrated Security=True"))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO Kelimeler (Kelime) VALUES (@Kelime)", conn);
                        cmd.Parameters.AddWithValue("@Kelime", kelime);
                        cmd.ExecuteNonQuery();
                    }
                    this.BeginInvoke((MethodInvoker)delegate {
                        BindGrid();
                    });
                }
                else
                {
                    int kelimeId = Convert.ToInt32(idHücre);
                    using (SqlConnection conn = new SqlConnection(@"Data Source=HP\ZDB2100005899;Initial Catalog=KareBulmaca;Integrated Security=True"))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE Kelimeler SET Kelime = @Kelime WHERE KelimeID = @KelimeID", conn);
                        cmd.Parameters.AddWithValue("@Kelime", kelime);
                        cmd.Parameters.AddWithValue("@KelimeID", kelimeId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }






        private void Form3_Load(object sender, EventArgs e)
        {
            this.BindGrid();
            dataGridView1.RowValidated += dataGridView1_RowValidated;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz en az bir satır seçin.");
                return;
            }

            DialogResult sonuc = MessageBox.Show("Seçilen satır(lar) silinecek. Emin misiniz?", "Onayla", MessageBoxButtons.YesNo);
            if (sonuc != DialogResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(@"Data Source=HP\ZDB2100005899;Initial Catalog=KareBulmaca;Integrated Security=True"))
            {
                conn.Open();

                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (row.IsNewRow) continue;

                    var idHücre = row.Cells["KelimeID"].Value;
                    if (idHücre != null && idHücre != DBNull.Value)
                    {
                        int kelimeId = Convert.ToInt32(idHücre);
                        SqlCommand cmd = new SqlCommand("DELETE FROM Kelimeler WHERE KelimeID = @KelimeID", conn);
                        cmd.Parameters.AddWithValue("@KelimeID", kelimeId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Tabloyu güncelle
            BindGrid();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }
    }
}
