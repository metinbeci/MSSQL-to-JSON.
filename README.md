# MSSQL-to-JSON.
MSSQL to JSON is a Windows Service application designed to retrieve data from an MSSQL database using a pre-defined SQL query and convert the results into JSON format.
 

 
# Description:
MSSQL to JSON is a Windows Service application designed to retrieve data from an MSSQL database using a pre-defined SQL query and convert the results into JSON format. The application runs at regular intervals, updating the JSON file with the latest data. The generated JSON file and a detailed log of the process are made accessible through the Internet Information Services (IIS), ensuring secure access for authorized users.

# Example Usage
Sample configuration setup
'''
<?xml version="1.0" encoding="utf-8"?>
 
<configuration>
	<connectionStrings>
	 		<add name="MyConnectionString" connectionString="Data Source=IPADRESS;Initial Catalog=DATABASE_NAME;User ID=SQLUSER;Password=SQLPASSWORD" />
	</connectionStrings>
	<appSettings>
		<!-- Hangi saat aralıklarında çalışacak ise aşağıda saatleri belirtin.-->
		<add key="StartTime" value="08" />
		<add key="EndTime" value="23" />
		<add key="MerchantID" value="2" />
		<add key="MerchantName" value="KILICLAR" />
		
	</appSettings>
</configuration>
'''

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
