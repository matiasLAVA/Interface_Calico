﻿using Calico.common;
using Calico.interfaces.pedido;
using Calico.persistencia;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nini.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace Calico.interfaces.pedidos
{
    class PedidoUtils
    {
        public String BuildUrl(String urlParam, String param, String value)
        {
            Dictionary<String, String> dictionary = new Dictionary<string, string>();
            dictionary.Add(param, value);
            return Utils.BuildUrl(urlParam, dictionary);
        }

        public String GetValueOrEmpty(String[] tipos,int size)
        {
            if(tipos.Length >= size)
            {
                return tipos[size - 1];
            }

            return String.Empty;
        }

        public PedidoJson getJson(String fromStatus,String toStatus,String[] tipos)
        {
            PedidoJson json = new PedidoJson();
            json.fromStatus = fromStatus;
            json.toStatus = toStatus;
      
            json.OrTy1 = GetValueOrEmpty(tipos,1);
            json.OrTy2 = GetValueOrEmpty(tipos, 2);
            json.OrTy3 = GetValueOrEmpty(tipos, 3);
            json.OrTy4 = GetValueOrEmpty(tipos, 4);
            json.OrTy5 = GetValueOrEmpty(tipos, 5);
            json.OrTy6 = GetValueOrEmpty(tipos, 6);
            json.OrTy7 = GetValueOrEmpty(tipos, 7);
            json.OrTy8 = GetValueOrEmpty(tipos, 8);
            json.OrTy9 = GetValueOrEmpty(tipos, 9);
            json.OrTy10 = GetValueOrEmpty(tipos, 10);
            json.OrTy11 = GetValueOrEmpty(tipos, 11);
            json.OrTy12 = GetValueOrEmpty(tipos, 12);
            json.OrTy13 = GetValueOrEmpty(tipos, 13);
            json.OrTy14 = GetValueOrEmpty(tipos, 14);

            return json;

        }

        public String JsonToString(PedidoJson obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return json;
        }

        public List<PedidoDTO> SendRequestPost(string url, String user, String pass, String json)
        {
            String myJsonString = String.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(user + ":" + pass));
            request.Headers.Add("Authorization", "Basic " + encoded);
            request.ContentType = "application/json";
            request.Method = Constants.METHOD_POST;
            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("El servicio rest retorno HTTP 200 OK");
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        myJsonString = reader.ReadToEnd();
                        if(!String.Empty.Equals(myJsonString))
                            return MappingJsonPedido(myJsonString);
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)e.Response;
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    handleErrorRest(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return new List<PedidoDTO>();

        }

        public List<PedidoDTO> MappingJsonPedido(String myJsonString)
        {
            var jc = JsonConvert.DeserializeObject<JObject>(myJsonString);

            JArray rowset = jc.Value<JObject>("ServiceRequest1")
                                 .Value<JObject>("fs_DATABROWSE_V554211")
                                 .Value<JObject>("data")
                                 .Value<JObject>("gridData")
                                 .Value<JArray>("rowset");

            if (rowset != null && rowset.Count > 0)
            {
                return rowset.ToObject<List<PedidoDTO>>() as List<PedidoDTO>;
            }

            return new List<PedidoDTO>();
        }

        public static void handleErrorRest(String myJsonString)
        {
            JObject json = JObject.Parse(myJsonString);

            Console.WriteLine("Servicio Rest KO");
            Console.WriteLine("Detalle: ");
            Console.WriteLine(json["message"]);
        }


        public void MappingPedidoDTOPedido(List<PedidoDTO> pedidoDTOList, Dictionary<string, tblPedido> dictionary, String emplazamiento, String compania, String cliente, IConfigSource source)
        {
            foreach(PedidoDTO pedidoDTO in pedidoDTOList)
            {
                tblPedido pedido = null;
                dictionary.TryGetValue(pedidoDTO.F4201_DOCO, out pedido);
                if (pedido == null)
                {
                    String tipoPedido = source.Configs[Constants.INTERFACE_PEDIDOS + "." + Constants.INTERFACE_PEDIDOS_TIPO_PEDIDO].GetString(pedidoDTO.F4201_DCTO);
                    String letra = source.Configs[Constants.INTERFACE_PEDIDOS + "." + Constants.INTERFACE_PEDIDOS_LETRA].GetString(pedidoDTO.F4201_DCTO);

                    /* CABEZERA */
                    pedido = fillCabezera(pedidoDTO, emplazamiento, letra, cliente, tipoPedido);
                    /* DETALLE */
                    tblPedidoDetalle detalle = fillDetalle(pedidoDTO, compania);
                    pedido.tblPedidoDetalle.Add(detalle);
                    dictionary.Add(pedidoDTO.F4201_DOCO, pedido);
                }
                else
                {
                    /* DETALLE */
                    tblPedidoDetalle detalle = fillDetalle(pedidoDTO, compania);
                    pedido.tblPedidoDetalle.Add(detalle);
                }
            }
        }

        private tblPedido fillCabezera(PedidoDTO pedidoDTO, String emplazamiento, String letra, String cliente, String tipoPedido)
        {
            tblPedido pedido = new tblPedido();
            pedido.pedc_emplazamiento = emplazamiento;
            pedido.pedc_tped_codigo = tipoPedido;
            pedido.pedc_letra = letra;
            pedido.pedc_numero = Convert.ToDecimal(pedidoDTO.F4201_DOCO);

            if (!String.IsNullOrWhiteSpace(pedidoDTO.F4201_OPDJ))
            {
                string result = DateTime.ParseExact(pedidoDTO.F4201_OPDJ, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
                pedido.pedc_fechaEntrega = Utils.ParseDate(result, "yyyy/MM/dd");
            }
            else
            {
                pedido.pedc_fechaEntrega = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
            }

            pedido.pedc_cliente = cliente;
            pedido.pedc_destinatario = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_MCU) ? pedidoDTO.F4201_MCU.Trim() : String.Empty;
            pedido.pedc_referenciaA = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_VR01) ? pedidoDTO.F4201_VR01.Trim() : String.Empty;
            pedido.pedc_referenciaB = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_VR02) ? pedidoDTO.F4201_VR02.Trim() : String.Empty;
            pedido.pedc_pais =  !String.IsNullOrWhiteSpace(pedidoDTO.F4006_COUN) ? pedidoDTO.F4006_COUN.Trim() : String.Empty;
            pedido.pedc_provincia = !String.IsNullOrWhiteSpace(pedidoDTO.F4006_ADDS) ? pedidoDTO.F4006_ADDS.Trim() : String.Empty;
            pedido.pedc_codigoPostal = !String.IsNullOrWhiteSpace(pedidoDTO.F4006_ADDZ) ? pedidoDTO.F4006_ADDZ.Trim() : String.Empty;
            pedido.pedc_localidad =  !String.IsNullOrWhiteSpace(pedidoDTO.F4006_CTY1) ? pedidoDTO.F4006_CTY1.Trim() : String.Empty;
            pedido.pedc_domicilio = pedidoDTO.F4006_ADD1 + " " + pedidoDTO.F4006_ADD2 + " " + pedidoDTO.F4006_ADD3 + " " + pedidoDTO.F4006_ADD4;

            pedido.pedc_areaMuelle = String.Empty;
            pedido.pedc_centroCosto = String.Empty;
            pedido.pedc_contraRembolso = 0;
            pedido.pedc_entregaParcial = false;
            pedido.pedc_fechaEmision = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
            pedido.pedc_importeFactura = 0;
            pedido.pedc_numeroRuteo = 0;
            pedido.pedc_observaciones = String.Empty;
            pedido.pedc_prioridad = 0;
            pedido.pedc_razonSocial = String.Empty;

            return pedido;
        }

        private tblPedidoDetalle fillDetalle(PedidoDTO pedidoDTO, String compania)
        {
            tblPedidoDetalle detalle = new tblPedidoDetalle();
            detalle.pedd_linea = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_LNID) ? Convert.ToDecimal(pedidoDTO.F4211_LNID) : 0;
            detalle.pedd_compania = compania;
            detalle.pedd_producto = String.Empty;
            detalle.pedd_lote = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_LOTN) ? pedidoDTO.F4211_LOTN : String.Empty;
            detalle.pedd_cantidad = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_UORG) ? Convert.ToDecimal(pedidoDTO.F4211_UORG) : 0;

            detalle.pedd_despachoParcial = false;
            detalle.pedd_epro_codigo = String.Empty;
            detalle.pedd_loteUnico = false;
            detalle.pedd_serie = String.Empty;
          
            return detalle;
        }

    }
}
