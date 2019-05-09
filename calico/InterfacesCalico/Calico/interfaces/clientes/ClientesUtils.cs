﻿using Calico.common;
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
            if (Constants.MLNM.Equals(key))
            {
                dictionary.Add(Constants.PARAM_FECHA, fecha);
                url = Utils.BuildUrl(urlParam, dictionary);
            }
            else if (Constants.TAX.Equals(key))
            {
                dictionary.Add(Constants.PARAM_FECHA, fecha);
                url = Utils.BuildUrl(urlParam, dictionary);
            }

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

            return header;
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
                cliente.subc_cuit = data;
            }
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
                while (rowset.First != null)
                {
                    AN8 = rowset.First[Constants.JSON_SUBFIX_MLNM + "_" + Constants.COLUMN_AN8].ToString();
                    value = rowset.First[Constants.JSON_SUBFIX_MLNM + "_" + Constants.COLUMN_MLNM].ToString();
                    AddDataToDictionary(diccionary, AN8, value, key);
                    rowset.First.Remove();
                }
            }
            else if (Constants.TAX.Equals(key))
            {
                while (rowset.First != null)
                {
                    AN8 = rowset.First[Constants.JSON_SUBFIX_TAX + "_" + Constants.COLUMN_AN8].ToString();
                    value = rowset.First[Constants.JSON_SUBFIX_TAX + "_" + Constants.COLUMN_TAX].ToString();
                    AddDataToDictionary(diccionary, AN8, value, key);
                    rowset.First.Remove();
                }
            }
        }

    }
}
