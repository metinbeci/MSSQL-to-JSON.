# MSSQL-to-JSON.
MSSQL to JSON is a Windows Service application designed to retrieve data from an MSSQL database using a pre-defined SQL query and convert the results into JSON format.
 

 
# Description:
MSSQL to JSON is a Windows Service application designed to retrieve data from an MSSQL database using a pre-defined SQL query and convert the results into JSON format. The application runs at regular intervals, updating the JSON file with the latest data. The generated JSON file and a detailed log of the process are made accessible through the Internet Information Services (IIS), ensuring secure access for authorized users.

# Example Usage
Sample configuration setup
```
<?xml version="1.0" encoding="utf-8"?>
 
<configuration>
	<connectionStrings>
	 		<add name="MyConnectionString" connectionString="Data Source=IPADRESS;Initial Catalog=DATABASE_NAME;User ID=SQLUSER;Password=SQLPASSWORD" />
	</connectionStrings>
	<appSettings>
			<!-- Specify the hours below in which hours it will work. (Hangi saat aralıklarında çalışacak ise aşağıda saatleri belirtin.)-->
		<add key="StartTime" value="08" />
		<add key="EndTime" value="23" />
		<add key="MerchantID" value="2" />
		<add key="MerchantName" value="KILICLAR" />
		
	</appSettings>
</configuration>
```

 
Summary

```Shell
 Sure, here's a translation of the summarized steps into English:

1. Time Check and Determining the Operating Time
2. Time Check and Log Message
3. Database Connection and Data Retrieval
4. Writing Data to JSON File
5. Duration Calculations and Log Messages
6. Copying Files to the Target Directory
7. Error Handling

 
private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Let's check the clock (Saat kontrolü yapalım)
                DateTime now = DateTime.Now;
                int hour = now.Hour;
                int startTime = int.Parse(ConfigurationManager.AppSettings["StartTime"]);
                int endTime = int.Parse(ConfigurationManager.AppSettings["EndTime"]);


                // Specify the hours you do not want to run in certain time intervals here (Belirli saat aralıklarında çalışmasını istemediğiniz saatleri burada belirleyin )
                if (hour < startTime || hour >= endTime)
                {
                    // The application will not run, it will go to standby (Uygulama çalıştırılmayacak, beklemeye geçecek)
                    if (!logMessageDisplayed)
                    {
                        logMessageDisplayed = true;
                        LogMessage("The time period that will not work at certain time intervals. Current Time Waiting...(" + startTime +  endTime + ")");
                    }
                    return;
                }
                else
                {
                 
                    logMessageDisplayed = false;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
                int merchantID = int.Parse(ConfigurationManager.AppSettings["MerchantID"]);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    LogMessage("SQL Connection OPENED.");
                

                    string query = "select * from [_IDEPEX_PAZARYERI_MIKRODATA_fark]";

                    using (SqlCommand command = new SqlCommand(query, connection))
             
                    {
                        command.CommandTimeout = 100;
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(dataTable);
                        int rowCount = dataTable.Rows.Count;
                        LogMessage("SQL Queries DONE.");
                        connection.Close();
                        LogMessage("SQL Connection CLOSED.");

                        // Let's write the data to JSON file (Veriyi JSON dosyasına yazalım)
                         
 
                        string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output_"+ merchantID + ".json");
                        WriteDataTableToJson(dataTable, rowCount, outputPath, stopwatch.Elapsed.TotalSeconds);
                    }
                }
                stopwatch.Stop();

                // Let's write the time to the JSON file (Süreyi JSON dosyasına yazalım)
                string timeTakenComment = "Islem Suresi: " + stopwatch.Elapsed.TotalSeconds ;

                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output_"+ merchantID +".json");
                string LogFilePath  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServiceLog_" + merchantID + ".txt");


                LogMessage("JSON File CREATED.");
                LogMessage(timeTakenComment);

                //inetpub copy below C:\inetpub\wwwroot\TrendyolServis
                // Create the destination folder(Hedef klasörü oluşturun) (eğer yoksa)
                string targetDirectory = "c:\\inetpub\\wwwroot\\TrendyolServis";
                Directory.CreateDirectory(targetDirectory);



                // Check if JSON file exists in destination (JSON dosyasının hedefte var olup olmadığını kontrol edin)
                string targetPath = Path.Combine(targetDirectory, "output_" + merchantID + ".json");
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath); // If the file exists, delete it (Dosya varsa silin)
                }
                File.Copy(jsonFilePath, targetPath); // copy file (Dosyayı kopyala)

                // ServiceLog.txt Check if the file exists in the destination (dosyasının hedefte var olup olmadığını kontrol edin)
                string targetServiceLogFilePath = Path.Combine(targetDirectory, "ServiceLog_" + merchantID + ".txt");
                if (File.Exists(targetServiceLogFilePath))
                {
                    File.Delete(targetServiceLogFilePath); // If the file exists, delete it(Dosya varsa silin)
                }
                File.Copy(LogFilePath, targetServiceLogFilePath); // copy file (Dosyayı kopyala)


            }
            catch (Exception ex)
            {
                // Error handling (Hata yönetimi)
                LogMessage("An error occurred: " + ex.Message);
            }
        }
```
# Key Features:

Automatically fetches data from MSSQL database and converts it to JSON format.
Scheduled to run every 5 minutes, providing up-to-date information.
Logs each step of the process for easy monitoring and troubleshooting.
JSON file and log file are securely accessible through IIS.
SQL Connection Configuration: The application reads the SQL connection string from the configuration file to establish a connection with the MSSQL database. To configure the SQL connection, open the application's configuration file (App.config) and locate the connectionStrings section. Replace the placeholder values (your_server_name, your_database_name, your_username, your_password) with the appropriate information for your MSSQL database.

# Development 
IDE: Visual Studio 2022 Language: C#

# Usage Guidelines:

Install the application as a Windows Service on your server.
Configure the SQL connection in the application's configuration file.
Obtain authorization from the project administrator to access the application's data.
Provide your external IP address to the administrator for inclusion in the whitelist.
Access the JSON file and log file through the designated URL and port.
With the implementation of the whitelist and secure SQL connection configuration, MSSQL to JSON guarantees a controlled and secure data sharing environment, protecting sensitive information from unauthorized access.

For more information and usage instructions, please refer to the project documentation. Additional Security: To ensure the security of the data and prevent unauthorized access, external access to the application's JSON file and log file has been restricted using a whitelist mechanism. A specific port has been configured on the source server, and only users with authorized IP addresses are allowed access to this port. The IP addresses of authorized users have been added to the whitelist, granting them exclusive access to the data.
