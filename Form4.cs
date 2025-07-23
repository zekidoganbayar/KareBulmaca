using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace KareBulmaca
{
    public partial class Form4 : Form
    {
        Random rnd = new Random();
        List<KelimeYerlestirmeBilgisi> yerlestirilenKelimeler = new List<KelimeYerlestirmeBilgisi>();
        List<Point> secilenHarfler = new List<Point>();
        Dictionary<string, Color> kelimeRenkleri = new Dictionary<string, Color>();
        private bool isMouseDown = false;
        private Point lastSelectedCell = Point.Empty;
        private Point? oncekiHucre = null;

        public Form4()
        {
            InitializeComponent();
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += listBox1_DrawItem;

            // Add mouse event handlers for drag selection
            dataGridView1.CellMouseDown += DataGridView1_CellMouseDown;
            dataGridView1.CellMouseEnter += DataGridView1_CellMouseEnter;
            dataGridView1.MouseUp += DataGridView1_MouseUp;
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;

        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKelime = listBox1.SelectedItem.ToString();

            // gridde bul
            var bilgi = yerlestirilenKelimeler.FirstOrDefault(k => k.Kelime == secilenKelime);

            // önceden vurgulanmış hücre varsa
            if (oncekiHucre.HasValue)
            {
                var eski = oncekiHucre.Value;
                var eskiHucre = dataGridView1[eski.X, eski.Y];

                // sarıysa sıfırla
                if (eskiHucre.Style.BackColor == Color.Yellow)
                {
                    eskiHucre.Style.BackColor = Color.White;
                }
            }

            // yeni harf seç
            var rastgeleHarf = bilgi.Koordinatlar[rnd.Next(bilgi.Koordinatlar.Count)];

            // boya
            dataGridView1[rastgeleHarf.X, rastgeleHarf.Y].Style.BackColor = Color.Yellow;

            oncekiHucre = rastgeleHarf;
        }



        private async void Form4_Load(object sender, EventArgs e)
        {
            await Task.Delay(500);
            KelimeleriYerlestirVeGoster();
        }

        private async Task<List<string>> TurkceKelimeleriGetirAsync()
        {
            List<string> kelimeler = new List<string>();
            using (HttpClient client = new HttpClient())
            {
                string url = "https://raw.githubusercontent.com/ncarkaci/TDKDictionaryCrawler/refs/heads/master/TDK_S%C3%B6zl%C3%BCk_Kelime_Listesi.txt";
                string response = await client.GetStringAsync(url);
                string[] lines = response.Split(new[] { "\r\n", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    string kelime = line.Trim().ToUpper();
                    if (kelime.Length <= 15) // Kare boyutundan uzun kelimeleri alma
                        kelimeler.Add(kelime);
                }
            }

            // Rastgele 20 kelime seç
            return kelimeler.OrderBy(x => Guid.NewGuid()).Take(20).ToList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public class KelimeYerlestirmeBilgisi
        {
            public string Kelime { get; set; }
            public List<Point> Koordinatlar { get; set; }
        }

        private void GridOlustur(int boyut)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            for (int i = 0; i < boyut; i++)
            {
                dataGridView1.Columns.Add($"col{i}", "");
                dataGridView1.Columns[i].Width = 41;
            }

            for (int i = 0; i < boyut; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Height = 41;
            }

            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private List<string> RastgeleKelimeleriGetir()
        {
            List<string> kelimeler = new List<string>();
            string connStr = @"Data Source=HP\ZDB2100005899;Initial Catalog=KareBulmaca;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT TOP 20 Kelime FROM Kelimeler ORDER BY NEWID()";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            kelimeler.Add(reader.GetString(0).ToUpper());
                        }
                    }
                }
            }
            return kelimeler;
        }

        private KelimeYerlestirmeBilgisi KelimeYerlestir(string kelime, char[,] tablo)
        {
            int boyut = tablo.GetLength(0);
            List<(int x, int y, bool yatay, int skor)> adaylar = new List<(int, int, bool, int)>();

            for (int y = 0; y < boyut; y++)
            {
                for (int x = 0; x < boyut; x++)
                {
                    for (int yatayMi = 0; yatayMi < 2; yatayMi++)
                    {
                        bool yatay = yatayMi == 0;
                        bool uygun = true;
                        int skor = 0; // Kesişim puanı

                        if (yatay && x + kelime.Length <= boyut)
                        {
                            for (int i = 0; i < kelime.Length; i++)
                            {
                                char mevcut = tablo[y, x + i];
                                if (mevcut != '\0' && mevcut != kelime[i])
                                {
                                    uygun = false;
                                    break;
                                }
                                if (mevcut == kelime[i]) skor++;
                            }

                            if (uygun) adaylar.Add((x, y, true, skor));
                        }
                        else if (!yatay && y + kelime.Length <= boyut)
                        {
                            for (int i = 0; i < kelime.Length; i++)
                            {
                                char mevcut = tablo[y + i, x];
                                if (mevcut != '\0' && mevcut != kelime[i])
                                {
                                    uygun = false;
                                    break;
                                }
                                if (mevcut == kelime[i]) skor++;
                            }

                            if (uygun) adaylar.Add((x, y, false, skor));
                        }
                    }
                }
            }

            // Kesişim puanı en yüksek olanlardan birini seç
            if (adaylar.Count > 0)
            {
                int maxSkor = -1;
                foreach (var a in adaylar)
                    if (a.skor > maxSkor) maxSkor = a.skor;

                var enIyiAdaylar = adaylar.FindAll(a => a.skor == maxSkor);
                var secilen = enIyiAdaylar[rnd.Next(enIyiAdaylar.Count)];
                List<Point> koordinatlar = new List<Point>();

                if (secilen.yatay)
                {
                    for (int i = 0; i < kelime.Length; i++)
                    {
                        tablo[secilen.y, secilen.x + i] = kelime[i];
                        koordinatlar.Add(new Point(secilen.x + i, secilen.y));
                    }
                }
                else
                {
                    for (int i = 0; i < kelime.Length; i++)
                    {
                        tablo[secilen.y + i, secilen.x] = kelime[i];
                        koordinatlar.Add(new Point(secilen.x, secilen.y + i));
                    }
                }

                return new KelimeYerlestirmeBilgisi { Kelime = kelime, Koordinatlar = koordinatlar };
            }

            return null;
        }

        private async void KelimeleriYerlestirVeGoster()
        {
            int boyut = 15;
            GridOlustur(boyut);
            List<string> kelimeler = await TurkceKelimeleriGetirAsync();

            listBox1.Items.Clear();
            yerlestirilenKelimeler.Clear();
            kelimeRenkleri.Clear();
            secilenHarfler.Clear();

            char[,] tablo = new char[boyut, boyut];

            foreach (string kelime in kelimeler)
            {
                var bilgi = KelimeYerlestir(kelime, tablo);
                if (bilgi != null)
                {
                    yerlestirilenKelimeler.Add(bilgi);
                    listBox1.Items.Add(kelime);
                }
            }

            for (int y = 0; y < boyut; y++)
            {
                for (int x = 0; x < boyut; x++)
                {
                    if (tablo[y, x] == '\0')
                        tablo[y, x] = (char)('A' + rnd.Next(26));
                    dataGridView1[x, y].Value = tablo[y, x];
                    dataGridView1[x, y].Style.BackColor = Color.White;
                }
            }
        }

        private void DataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            isMouseDown = true;
            lastSelectedCell = new Point(e.ColumnIndex, e.RowIndex);

            // Clear previous selection
            ClearSelection();

            // Process the clicked cell
            ProcessCellSelection(e.ColumnIndex, e.RowIndex);
        }

        private void DataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (!isMouseDown || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Select all cells between the starting point and current point
            SelectCellsBetween(lastSelectedCell, new Point(e.ColumnIndex, e.RowIndex));
        }

        private void DataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            CheckForWordMatch();
        }

        private void SelectCellsBetween(Point start, Point end)
        {
            ClearSelection();

            // Determine if we're selecting horizontally or vertically
            bool horizontal = Math.Abs(start.X - end.X) > Math.Abs(start.Y - end.Y);

            if (horizontal)
            {
                // Horizontal selection
                int minX = Math.Min(start.X, end.X);
                int maxX = Math.Max(start.X, end.X);
                int y = start.Y;

                for (int x = minX; x <= maxX; x++)
                {
                    ProcessCellSelection(x, y);
                }
            }
            else
            {
                // Vertical selection
                int minY = Math.Min(start.Y, end.Y);
                int maxY = Math.Max(start.Y, end.Y);
                int x = start.X;

                for (int y = minY; y <= maxY; y++)
                {
                    ProcessCellSelection(x, y);
                }
            }
        }

        private void ProcessCellSelection(int x, int y)
        {
            Point cell = new Point(x, y);

            if (!secilenHarfler.Contains(cell))
            {
                secilenHarfler.Add(cell);
                dataGridView1[x, y].Style.BackColor = Color.LightBlue;
            }
        }

        private void ClearSelection()
        {
            foreach (var p in secilenHarfler)
            {
                dataGridView1[p.X, p.Y].Style.BackColor = Color.White;
            }
            secilenHarfler.Clear();
        }

        private void CheckForWordMatch()
        {
            if (secilenHarfler.Count == 0) return;

            // Yeni kelimeyi oluştur
            string secilenKelime = "";
            foreach (var p in secilenHarfler.OrderBy(p => p.Y).ThenBy(p => p.X)) // Sort by row then column
            {
                secilenKelime += dataGridView1[p.X, p.Y].Value.ToString();
            }

            foreach (var kelimeInfo in yerlestirilenKelimeler)
            {
                if (kelimeInfo.Kelime == secilenKelime)
                {
                    Color renk = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    kelimeRenkleri[kelimeInfo.Kelime] = renk;

                    listBox1.Refresh();

                    foreach (var p in kelimeInfo.Koordinatlar)
                    {
                        dataGridView1[p.X, p.Y].Style.BackColor = renk;
                    }

                    secilenHarfler.Clear();
                    return;
                }
            }

            // If no match found, keep the selection highlighted
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string kelime = listBox1.Items[e.Index].ToString();
            Color renk = kelimeRenkleri.ContainsKey(kelime) ? kelimeRenkleri[kelime] : e.ForeColor;

            e.DrawBackground();
            using (Brush b = new SolidBrush(renk))
            {
                e.Graphics.DrawString(kelime, e.Font, b, e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Task.Delay(500);
            KelimeleriYerlestirVeGoster();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            Point secilen = new Point(e.ColumnIndex, e.RowIndex);

            // Zaten seçilmişse iptal et (kaldır ve rengi eski haline getir)
            if (secilenHarfler.Contains(secilen))
            {
                secilenHarfler.Remove(secilen);
                dataGridView1[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.White; // veya varsayılan renk
            }
            else
            {
                secilenHarfler.Add(secilen);
                dataGridView1[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.LightBlue;
            }

            // Yeni kelimeyi oluştur
            string secilenKelime = "";
            foreach (var p in secilenHarfler)
            {
                secilenKelime += dataGridView1[p.X, p.Y].Value.ToString();
            }

            foreach (var kelimeInfo in yerlestirilenKelimeler)
            {
                if (kelimeInfo.Kelime == secilenKelime)
                {
                    Color renk = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    kelimeRenkleri[kelimeInfo.Kelime] = renk;

                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        if (listBox1.Items[i].ToString() == kelimeInfo.Kelime)
                        {
                            listBox1.Refresh();
                            break;
                        }
                    }

                    foreach (var p in kelimeInfo.Koordinatlar)
                    {
                        dataGridView1[p.X, p.Y].Style.BackColor = renk;
                    }

                    secilenHarfler.Clear();
                    return;
                }
            }
        }
    }
}