﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using System.IO;
using ReadWriteCsv;
using System.Windows.Forms;
using Newtonsoft.Json;
using AxesoFeng.Classes;

namespace AxesoFeng
{
    public class FolioOrder
    {
        private RestClient client;
        string bu;
        public FolioOrder(String BaseUrl)
        {
            client = new RestClient(BaseUrl);
            bu = BaseUrl;
        }

        public List<RespFolio.Assets> GetProductsFile(string fileName)
        {
            List<RespFolio.Assets> productsFile = new List<RespFolio.Assets>();
            using (CsvFileReader reader = new CsvFileReader(fileName))
            {
                CsvRow rowcsv = new CsvRow();
                while (reader.ReadRow(rowcsv))
                {
                    productsFile.Add(new RespFolio.Assets(rowcsv[0], rowcsv[1], rowcsv[2], int.Parse(rowcsv[3])));
                    //table.addRow(rowcsv[0],rowcsv[1],rowcsv[2]);
                }
            }
            return productsFile;
        }

        public List<string> CompareTo(String path)
        {
            RespFolio respFolio;
            string folio = GetFolio(path);
            string fileName = Path.GetFileName(path);
            respFolio = GETInventoryFile();
            List<string> messages = new List<string>();
            if (respFolio != null)
            {
                List<RespFolio.Assets> productsFile = GetProductsFile(fileName);
                //messages = CompareTo(productsFile, respFolio);
            }
            return messages;
        }

        public List<string> CompareTo(List<RespFolio.Assets> productsRead, String valueWarehouse)
        {
            RespFolio respFolio;
            respFolio = GETInventoryFile();
            List<string> messages = new List<string>();
            if (respFolio != null)
            {
                messages = CompareTo(productsRead, respFolio,valueWarehouse);
            }
            return messages;
        }

        private List<string> CompareTo(List<RespFolio.Assets> productsRead, RespFolio respInv, String valueWarehouse)
        {
            //esta mal esta comparasion
            List<string> Inequalities = new List<string>();
            bool firstMessage = true;
            foreach (RespFolio.Assets prodRead in productsRead)
            {
                foreach (RespFolio.Assets prodInv in respInv.assets)
                {
                    if (prodRead.upc == prodInv.upc)
                    {
                        if (prodInv.place_id != int.Parse(valueWarehouse))
                        {
                            if (firstMessage)
                            {
                                Inequalities.Add("**No deberian estar aqui:");
                                firstMessage = false;
                            }
                            Inequalities.Add(" " + prodRead.name);
                            break;
                        }
                    }
                }
            }
            bool find= false;
            firstMessage = true;
            foreach (RespFolio.Assets prodInv in respInv.assets)
            {
                if (prodInv.place_id == int.Parse(valueWarehouse))
                {
                    find = false;
                    foreach (RespFolio.Assets prodRead in productsRead)
                    {
                        if (prodInv.name == prodRead.name)
                        {
                            find = true; 
                            break;
                        }
                    }
                    if (find == false)
                    {
                        if (firstMessage)
                        {
                            Inequalities.Add("");
                            Inequalities.Add("**Deberian estar aqui:");
                            firstMessage = false;
                        }
                        Inequalities.Add(" " + prodInv.name);
                    }
                }
            }
            return Inequalities;
        }

        private string GetFolio(string fileName)
        {
            string folio; 
            string[] comp = fileName.Split(new Char[] { '_' });
            try{
                folio = comp[3];
            }
            catch (Exception exc) {
                folio = "";
            }
            return folio;
        }

        private RespFolio GETInventoryFile()
        {
            try {
                System.IO.StreamReader objReader;
                objReader = File.OpenText(@"\rfiddata\inventario.json");
                String text = objReader.ReadToEnd();
                RespFolio data = JsonConvert.DeserializeObject<RespFolio>(text);
                return data;
            }
            catch (Exception exc) { }
            return null;
        }

        private bool requestError(String StatusCode)
        {
            switch (StatusCode)
            {
                case "0":
                case "NotFound":
                    MessageBox.Show("Error de Red. No se pudieron sincronizar los datos con el servidor. Verifique que tiene su wifi encendida, que tiene acceso a la red y al servidor.", "Error");
                    return false;
                case "Forbidden":
                case "InternalServerError":
                    MessageBox.Show("Error contacte a su administrador.", "Error");
                    return false;
                case "OK":
                    return true;
                default:
                    MessageBox.Show(StatusCode, "Error");
                    return false;
            }
        }

        public static void DeleteFiles()
        {
            Cursor.Current = Cursors.WaitCursor;
            string[] filePaths = Directory.GetFiles(@"\rfiddata");

            foreach (String path in filePaths)
            {
                if (path.Contains("iupc") || path.Contains("oupc") || path.Contains("iepc")
                    || path.Contains("oepc") || path.Contains("message"))
                {
                    File.Delete(path);
                }
            }
            Cursor.Current = Cursors.Default;
        }

    }
}
