using Calico.common;
using Calico.persistencia;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.interfaces.clientes
{
    class ClientesUtils
    {
        public String BuildUrl(String urlParam, String key, String fecha)
        {
            String url = String.Empty;
            Dictionary<String, String> dictionary = new Dictionary<string, string>();
            dictionary.Add(Constants.PARAM_FECHA, fecha);
            url = Utils.BuildUrl(urlParam, dictionary);
            return url;
        }

        private String GetHeaderJson(String key)
        {
            String header = String.Empty;

            if (Constants.MLNM.Equals(key))
            {
                header = Constants.JSON_PREFIX + Constants.JSON_SUBFIX_MLNM;
            }
            else if (Constants.TAX.Equals(key))
            {
                header = Constants.JSON_PREFIX + Constants.JSON_SUBFIX_TAX;
            }
            else
            {
                header = Constants.JSON_PREFIX + Constants.JSON_SUBFIX_F0116;
            }
            return header;
        }

        public void MappingCliente(String myJsonString, String key, Dictionary<String, tblSubCliente> diccionary)
        {
            var json = JObject.Parse(myJsonString);
            var root = json[GetHeaderJson(key)];
            var data = root[Constants.JSON_TAG_DATA];
            var gridData = data[Constants.JSON_TAG_GRIDDATA];
            var rowset = gridData[Constants.JSON_TAG_ROWSET];

            String AN8 = String.Empty;
            String value = String.Empty;

            if (Constants.MLNM.Equals(key))
            {
                String columnId = Constants.JSON_SUBFIX_MLNM + "_" + Constants.COLUMN_AN8;
                String columnValue = Constants.JSON_SUBFIX_MLNM + "_" + Constants.COLUMN_MLNM;
                SetValues(rowset, key, diccionary, columnId, columnValue);
            }
            else if (Constants.TAX.Equals(key))
            {
                String columnId = Constants.JSON_SUBFIX_TAX + "_" + Constants.COLUMN_AN8;
                String columnValue = Constants.JSON_SUBFIX_TAX + "_" + Constants.COLUMN_TAX;
                SetValues(rowset, key, diccionary, columnId, columnValue);
            }
            else if (Constants.ADDZ.Equals(key))
            {
                String columnId = Constants.JSON_SUBFIX_F0116 + "_" + Constants.COLUMN_AN8;
                String columnValue = Constants.JSON_SUBFIX_F0116 + "_" + Constants.ADDZ;
                SetValues(rowset, key, diccionary, columnId, columnValue);
            }
            else if (Constants.ADD1.Equals(key))
            {
                String columnId = Constants.JSON_SUBFIX_F0116 + "_" + Constants.COLUMN_AN8;
                String columnValue = Constants.ADD1ADD2ADD3;
                SetValues(rowset, key, diccionary, columnId, columnValue);
            }
            else if (Constants.CTY1.Equals(key))
            {
                String columnId = Constants.JSON_SUBFIX_F0116 + "_" + Constants.COLUMN_AN8;
                String columnValue = Constants.JSON_SUBFIX_F0116 + "_" + Constants.CTY1;
                SetValues(rowset, key, diccionary, columnId, columnValue);
            }
        }

        private void SetValues(JToken rowset, String key, Dictionary<String, tblSubCliente> diccionary, String columnId, String columnValue)
        {
            if (columnValue.Equals(Constants.ADD1ADD2ADD3))
            {
                String ADD1 = Constants.JSON_SUBFIX_F0116 + "_" + Constants.ADD1;
                String ADD2 = Constants.JSON_SUBFIX_F0116 + "_" + Constants.ADD2;
                String ADD3 = Constants.JSON_SUBFIX_F0116 + "_" + Constants.ADD3;
                while (rowset.First != null)
                {
                    String id = rowset.First[columnId].ToString();
                    String value1 = rowset.First[ADD1].ToString();
                    String value2 = rowset.First[ADD2].ToString();
                    String value3 = rowset.First[ADD3].ToString();
                    String value = value1 + " " + value2 + " " + value3;
                    AddDataToDictionary(diccionary, id, value, key);
                    rowset.First.Remove();
                }
            }
            while (rowset.First != null)
            {
                String id = rowset.First[columnId].ToString();
                String value = rowset.First[columnValue].ToString();
                AddDataToDictionary(diccionary, id, value, key);
                rowset.First.Remove();
            }
        }

        private void AddDataToDictionary(Dictionary<String, tblSubCliente> dictionary, String id, String data, String key)
        {
            tblSubCliente cliente = null;
            dictionary.TryGetValue(id, out cliente);
            if (cliente == null)
            {
                cliente = new tblSubCliente();
                cliente.subc_codigoCliente = id;
                dictionary.Add(id, cliente);
            }
            if (Constants.MLNM.Equals(key))
            {
                cliente.subc_razonSocial = data;
            }
            else if (Constants.TAX.Equals(key))
            {
                String cuit = String.IsNullOrEmpty(data) ? String.Empty : data;
                cliente.subc_cuit = cuit.Length > 13 ? "999999999999" : cuit;
            }
            else if (Constants.ADDZ.Equals(key))
            {
                cliente.subc_codigoPostal = data;
            }
            else if (Constants.ADD1.Equals(key))
            {
                cliente.subc_domicilio = data;
            }
            else if (Constants.CTY1.Equals(key))
            {
                cliente.subc_localidad = data;
            }
        }

    }
}
