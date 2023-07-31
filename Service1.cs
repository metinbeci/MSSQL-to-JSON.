using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using Newtonsoft.Json;
using System.Configuration; // System.Configuration eklendi
using System.Collections.Generic;


namespace TrendyolServis
{


                  public class DescriptionInfo
                  {
                          public string IslemSuresi { get; set; }
                          public string BuDosyaOlusturulmaTarihi { get; set; }
                          public int    ToplamUrunSayisi { get; set; }
                          public int    merchant_id { get; set; }
                          public string merchantname { get; set; }
                  }
                  public class Product
                      {
                          public string sto_kod { get; set; }
                          public string barkod { get; set; }
                          public string sto_isim { get; set; }
                          public string PazarYeri { get; set; }
                          public string PiyasaFiyati { get; set; }
                          public string PazarYeriSatisFiyati { get; set; }
                          public double StokMiktari { get; set; }
                          public string MinimumStok { get; set; }
                          public int    dep_no { get; set; }
                          public string dep_adi { get; set; }
                          public bool   SubeSatisAK { get; set; }
                          public bool   GenelSatisAK { get; set; }
                  }
        public class Output
        {
            public DescriptionInfo Description { get; set; }
            public List<Product> Products { get; set; }
        }
    
        public partial class idepexTrendyolService : ServiceBase
    {
        private Timer timer;
 
        public idepexTrendyolService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
         
            timer = new Timer(300000);
            timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            timer.AutoReset = true;
            timer.Start();
            LogMessage("Servis BASLATILDI.");   
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();
            LogMessage("Servis DURDURULDU.");
        }
        private bool logMessageDisplayed = false;

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Saat kontrolü yapalım
                DateTime now = DateTime.Now;
                int hour = now.Hour;
                int startTime = int.Parse(ConfigurationManager.AppSettings["StartTime"]);
                int endTime = int.Parse(ConfigurationManager.AppSettings["EndTime"]);


                // Belirli saat aralıklarında çalışmasını istemediğiniz saatleri burada belirleyin
                if (hour < startTime || hour >= endTime)
                {
                    // Uygulama çalıştırılmayacak, beklemeye geçecek
                    if (!logMessageDisplayed)
                    {
                        logMessageDisplayed = true;
                        LogMessage("Belirli saat araliklarinda calismayacak zaman dilimi. Gecerli Saat Bekleniyor...(" + startTime +  endTime + ")");
                    }
                    return;
                }
                else
                {
                    // If the current hour is within the allowed range, reset the log message flag.
                    logMessageDisplayed = false;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
                int merchantID = int.Parse(ConfigurationManager.AppSettings["MerchantID"]);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    LogMessage("SQL Baglantisi ACILDI.");
                

                    string query = "select * from [_IDEPEX_PAZARYERI_MIKRODATA_fark]";

                    using (SqlCommand command = new SqlCommand(query, connection))
             
                    {
                        command.CommandTimeout = 100;
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(dataTable);
                        int rowCount = dataTable.Rows.Count;
                        LogMessage("SQL Sorgular TAMAMLANDI.");
                        connection.Close();
                        LogMessage("SQL Baglantisi KAPATILDI.");

                        // Veriyi JSON dosyasına yazma
                         
 
                        string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output_"+ merchantID + ".json");
                        WriteDataTableToJson(dataTable, rowCount, outputPath, stopwatch.Elapsed.TotalSeconds);
                    }
                }
                stopwatch.Stop();

                // Süreyi JSON dosyasına yazma
                string timeTakenComment = "Islem Suresi: " + stopwatch.Elapsed.TotalSeconds ;

                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output_"+ merchantID +".json");
                string LogFilePath  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServiceLog_" + merchantID + ".txt");


                LogMessage("JSON Dosyasi OLUSTURULDU.");
                LogMessage(timeTakenComment);

                //inetpub altına kopyala C:\inetpub\wwwroot\TrendyolServis
                // Hedef klasörü oluşturun (eğer yoksa)
                string targetDirectory = "c:\\inetpub\\wwwroot\\TrendyolServis";
                Directory.CreateDirectory(targetDirectory);



                // JSON dosyasının hedefte var olup olmadığını kontrol edin
                string targetPath = Path.Combine(targetDirectory, "output_" + merchantID + ".json");
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath); // Dosya varsa silin
                }
                File.Copy(jsonFilePath, targetPath); // Dosyayı taşı

                // ServiceLog.txt dosyasının hedefte var olup olmadığını kontrol edin
                string targetServiceLogFilePath = Path.Combine(targetDirectory, "ServiceLog_" + merchantID + ".txt");
                if (File.Exists(targetServiceLogFilePath))
                {
                    File.Delete(targetServiceLogFilePath); // Dosya varsa silin
                }
                File.Copy(LogFilePath, targetServiceLogFilePath); // Dosyayı taşı


            }
            catch (Exception ex)
            {
                // Hata yönetimi  
                LogMessage("HATA OLUSTU: " + ex.Message);
            }
        }
        private void WriteDataTableToJson(DataTable dataTable, int rowCount, string outputPath, double islemSuresi)
        {
            int merchantID = int.Parse(ConfigurationManager.AppSettings["MerchantID"]);
            string merchantName = ConfigurationManager.AppSettings["MerchantName"];



            // Description bilgilerini oluştur
            var descriptionInfo = new DescriptionInfo
            {
                IslemSuresi = islemSuresi + "",
                BuDosyaOlusturulmaTarihi = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ToplamUrunSayisi = rowCount,
                merchant_id = merchantID,
                merchantname = merchantName
            };

            // JSON çıktısını oluştur
            var output = new Output
            {
                Description = descriptionInfo,
                Products = new List<Product>()
            };

            // Verileri JSON formatına dönüştür ve "products" listesine ekle
            foreach (DataRow row in dataTable.Rows)
            {
                output.Products.Add(new Product
                {
                    sto_kod = row["sto_kod"].ToString(),
                    barkod = row["barkod"].ToString(),
                    sto_isim = row["sto_isim"].ToString(),
                    PazarYeri = row["PazarYeri"].ToString(),
                    PiyasaFiyati = row["Piyasa Fiyatı"].ToString(),
                    PazarYeriSatisFiyati = row["PazarYeri Satış Fiyatı"].ToString(),
                    StokMiktari = Convert.ToDouble(row["StokMiktari"]),
                    MinimumStok = row["MinimumStok"].ToString(),
                    dep_no = Convert.ToInt32(row["dep_no"]),
                    dep_adi = row["dep_adi"].ToString(),
                    SubeSatisAK = Convert.ToBoolean(row["Şube Satış A/K"]),
                    GenelSatisAK = Convert.ToBoolean(row["Genel Satış A/K"]),
                });
            }

            // JSON çıktısını dosyaya yaz
            string json = JsonConvert.SerializeObject(output, Formatting.Indented);
            File.WriteAllText(outputPath, json);
        }
   

        private void LogMessage(string message)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            int merchantID = int.Parse(ConfigurationManager.AppSettings["MerchantID"]);

            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServiceLog_" + merchantID + ".txt");
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +"-["+ message+"]");
            }
        }
    }
}
