using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using SW.ServicioWeb;
using System.Timers;
using System.Net;
using System.IO;
using System.Configuration;

namespace SW
{
    public partial class SW : ServiceBase
    {
        private System.Timers.Timer tim;
        private string userName1;
        private string userName2;
        private string pass;
        private string host1; 
        private string host2; 
        private string localFile = @"C:\BK\";
        private FtpWebResponse ftpResponse = null;
        private FtpWebRequest ftpRequest = null;
        private Stream stream = null;

        public SW()
        {
            InitializeComponent();

            userName1 = "ssqlserver";
            userName2 = "swebsoap";
            pass = "Ingenieria123.";
            host1 = "ftp://ciudadcontradelincuencia.backup.somee.com/ciudadcontradelincuencia_MSSql_Database_Backup".ToString();
            host2 = "ftp://ciudadcontradelincuencia.somee.com/www.ciudadcontradelincuencia.somee.com".ToString();
            localFile = @"C:\BK\";
            ftpResponse = null;
            ftpRequest = null;
            stream = null;

            ServicioWeb.ServiciosTareaProgramada sw = new ServicioWeb.ServiciosTareaProgramada();
            int tbhoras = sw.obtenerTiempoBackup();
            tim = new System.Timers.Timer(tbhoras * 3600000);
            tim.Elapsed += new ElapsedEventHandler(ejecucionTimer);
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";


        }

        void ejecucionTimer(object sender, ElapsedEventArgs e)
        {
            DescargarDeS1();
            DescargarDeS2();
        }

        void DescargarDeS1()
        {
            string[] str = getFilesOnServer("", host1, userName1);
            foreach (string s in str)
            {
                if (s != "")
                {
                    ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host1 + "/" + s);
                    ftpRequest.Credentials = new NetworkCredential(userName1, pass);
                    ftpRequest.UseBinary = true;
                    ftpRequest.UsePassive = true;
                    ftpRequest.KeepAlive = true;
                    ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                    ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                    stream = ftpResponse.GetResponseStream();
                    FileStream fs = new FileStream(localFile + s, FileMode.OpenOrCreate);
                    byte[] byteBuffer = new byte[Convert.ToInt32(getFilesize(s, host1, userName1))];
                    int bytesRead = stream.Read(byteBuffer, 0, Convert.ToInt32(getFilesize(s, host1, userName1)));

                    try
                    {
                        while (bytesRead > 0)
                        {
                            fs.Write(byteBuffer, 0, bytesRead);
                            bytesRead = stream.Read(byteBuffer, 0, Convert.ToInt32(getFilesize(s, host1, userName1)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    fs.Close();
                    stream.Close();
                    ftpResponse.Close();
                    ftpRequest = null;
                }
            }
        }

        void DescargarDeS2()
        {
            string[] str = getFilesOnServer("/archivos/", host2, userName2);
            foreach (string s in str)
            {
                if (s != "")
                {
                    ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host2 + "/" + s);
                    ftpRequest.Credentials = new NetworkCredential(userName2, pass);
                    ftpRequest.UseBinary = true;
                    ftpRequest.UsePassive = true;
                    ftpRequest.KeepAlive = true;
                    ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                    ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                    stream = ftpResponse.GetResponseStream();
                    FileStream fs = new FileStream(localFile + s, FileMode.OpenOrCreate);
                    byte[] byteBuffer = new byte[Convert.ToInt32(getFilesize(s, host2, userName2))];
                    int bytesRead = stream.Read(byteBuffer, 0, Convert.ToInt32(getFilesize(s, host2, userName2)));

                    try
                    {
                        while (bytesRead > 0)
                        {
                            fs.Write(byteBuffer, 0, bytesRead);
                            bytesRead = stream.Read(byteBuffer, 0, Convert.ToInt32(getFilesize(s, host2, userName2)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    fs.Close();
                    stream.Close();
                    ftpResponse.Close();
                    ftpRequest = null;
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Iniciar");
            DescargarDeS1();
            DescargarDeS2();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Terminar");
        }

        private long getFilesize(string fileName, string host, string user)
        {
            long size;
            FtpWebRequest sizeRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
            sizeRequest.Credentials = new NetworkCredential(user, pass);
            sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            sizeRequest.UseBinary = true;
            FtpWebResponse serverResponse = (FtpWebResponse)sizeRequest.GetResponse();
            FtpWebResponse respSize = (FtpWebResponse)sizeRequest.GetResponse();
            size = respSize.ContentLength;
            return size;
        }

        public string[] getFilesOnServer(string dir, string host, string user)
        {
            string[] filesInDir = new string[50];
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host);
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                stream = ftpResponse.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string dirRam = null;
                try
                {
                    while (sr.Peek() != -1)
                    {
                        dirRam += sr.ReadLine() + "|";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                ftpResponse.Close();
                sr.Dispose();
                stream.Close();
                ftpRequest = null;
                try
                {
                    filesInDir = dirRam.Split("|".ToCharArray());
                    return filesInDir;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return filesInDir;
        }

    }
}
