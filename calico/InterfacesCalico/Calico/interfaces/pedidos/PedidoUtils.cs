using Calico.common;
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
using System.Text;

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

        public String GetValueOrEmpty(String index)
        {
            try {
                String value = FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_PEDIDOS + "." + Constants.MCU, Constants.MCU + index);
                return "0".Equals(value) ? String.Empty : value;
            }
            catch (Exception)
            {
                return String.Empty;
            }
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
            
            json.OrTy01 = GetValueOrEmpty(tipos, 1);
            json.OrTy02 = GetValueOrEmpty(tipos, 2);
            json.OrTy03 = GetValueOrEmpty(tipos, 3);
            json.OrTy04 = GetValueOrEmpty(tipos, 4);
            json.OrTy05 = GetValueOrEmpty(tipos, 5);
            json.OrTy06 = GetValueOrEmpty(tipos, 6);
            json.OrTy07 = GetValueOrEmpty(tipos, 7);
            json.OrTy08 = GetValueOrEmpty(tipos, 8);
            json.OrTy09 = GetValueOrEmpty(tipos, 9);
            json.OrTy10 = GetValueOrEmpty(tipos, 10);
            json.OrTy11 = GetValueOrEmpty(tipos, 11);
            json.OrTy12 = GetValueOrEmpty(tipos, 12);
            json.OrTy13 = GetValueOrEmpty(tipos, 13);
            json.OrTy14 = GetValueOrEmpty(tipos, 14);

            json.MCU01 = GetValueOrEmpty("01");
            json.MCU02 = GetValueOrEmpty("02");
            json.MCU03 = GetValueOrEmpty("03");
            json.MCU04 = GetValueOrEmpty("04");
            json.MCU05 = GetValueOrEmpty("05");
            json.MCU06 = GetValueOrEmpty("06");
            json.MCU07 = GetValueOrEmpty("07");
            json.MCU08 = GetValueOrEmpty("08");
            json.MCU09 = GetValueOrEmpty("09");
            json.MCU10 = GetValueOrEmpty("10");
            json.MCU11 = GetValueOrEmpty("11");
            json.MCU12 = GetValueOrEmpty("12");
            json.MCU13 = GetValueOrEmpty("13");
            json.MCU14 = GetValueOrEmpty("14");
            json.MCU15 = GetValueOrEmpty("15");
            json.MCU16 = GetValueOrEmpty("16");

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
            request.Proxy = null;  //12/07/2019 
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


        public void MappingPedidoDTOPedido(List<PedidoDTO> pedidoDTOList, Dictionary<string, tblPedido> dictionary, String emplazamiento, String cliente,Boolean fromRecepcionOR)
        {
            foreach(PedidoDTO pedidoDTO in pedidoDTOList)
            {
                tblPedido pedido = null;
                String tipoPedido;
                String letra;
                String compania = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_PEDIDOS + "." + Constants.COMPANIA, pedidoDTO.F4211_SRP1));

                dictionary.TryGetValue(pedidoDTO.F4201_DOCO, out pedido);
                if (pedido == null)
                {
                    if (fromRecepcionOR)
                    {
                        tipoPedido = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_RECEPCION_OR + "." + Constants.TIPO_PEDIDO, pedidoDTO.F4201_DCTO));
                        letra = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_RECEPCION_OR + "." + Constants.INTERFACE_PEDIDOS_LETRA, pedidoDTO.F4201_DCTO));
                    }
                    else
                    {
                        tipoPedido = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_PEDIDOS + "." + Constants.TIPO_PEDIDO, pedidoDTO.F4201_DCTO));
                        letra = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_PEDIDOS + "." + Constants.INTERFACE_PEDIDOS_LETRA, pedidoDTO.F4201_DCTO));
                    }
                    
                    String almacen = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.ALMACEN, pedidoDTO.F4201_MCU));
                    String sucursal = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.SUCURSAL, pedidoDTO.F4201_MCU));
                    String areaMuelle = Utils.GetValueOrEmpty(FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_PEDIDOS, Constants.INTERFACE_PEDIDOS_AREA_MUELLE));
                    /* CABEZERA */
                    pedido = fillCabezera(pedidoDTO, emplazamiento, letra, cliente, tipoPedido, almacen, sucursal,areaMuelle);
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

        private tblPedido fillCabezera(PedidoDTO pedidoDTO, String emplazamiento, String letra, String cliente, String tipoPedido,String almacen,String sucursal,String areaMuelle)
        {
            tblPedido pedido = new tblPedido();
            pedido.pedc_emplazamiento = emplazamiento;
            pedido.pedc_tped_codigo = tipoPedido;
            pedido.pedc_letra = letra;
            pedido.pedc_almacen = almacen;
            pedido.pedc_sucursal = sucursal;
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
            pedido.pedc_destinatario = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_SHAN) ? pedidoDTO.F4201_SHAN.Trim() : String.Empty;
            pedido.pedc_referenciaA = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_VR01) ? pedidoDTO.F4201_VR01.Trim() : String.Empty;
            pedido.pedc_referenciaB = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_VR02) ? pedidoDTO.F4201_VR02.Trim() : String.Empty;
            pedido.pedc_pais =  !String.IsNullOrWhiteSpace(pedidoDTO.F4006_COUN) ? pedidoDTO.F4006_COUN.Trim() : String.Empty;
            pedido.pedc_provincia = !String.IsNullOrWhiteSpace(pedidoDTO.F4006_ADDS) ? pedidoDTO.F4006_ADDS.Trim() : String.Empty;
            pedido.pedc_codigoPostal = !String.IsNullOrWhiteSpace(pedidoDTO.F4006_ADDZ) ? pedidoDTO.F4006_ADDZ.Trim() : String.Empty;
            pedido.pedc_localidad =  !String.IsNullOrWhiteSpace(pedidoDTO.F4006_CTY1) ? pedidoDTO.F4006_CTY1.Trim() : String.Empty;
            pedido.pedc_domicilio = pedidoDTO.F4006_ADD1 + " " + pedidoDTO.F4006_ADD2 + " " + pedidoDTO.F4006_ADD3 + " " + pedidoDTO.F4006_ADD4;

            pedido.pedc_areaMuelle = areaMuelle;
            pedido.pedc_centroCosto = String.Empty;
            pedido.pedc_contraRembolso = 0;
            pedido.pedc_entregaParcial = false;
            pedido.pedc_fechaEmision = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
            pedido.pedc_importeFactura = 0;
            pedido.pedc_numeroRuteo = 0;
            pedido.pedc_observaciones = String.Empty;
            pedido.pedc_prioridad = 0;
            pedido.pedc_razonSocial = String.Empty;
            pedido.pedc_referenciaA = String.Empty;
            pedido.pedc_referenciaB = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_DCTO) ? pedidoDTO.F4201_DCTO.Trim() : String.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(!String.IsNullOrWhiteSpace(pedidoDTO.F4201_VR01) ? pedidoDTO.F4201_VR01.Trim() : String.Empty).Append(" ");
            sb.Append(!String.IsNullOrWhiteSpace(pedidoDTO.F4201_VR02) ? pedidoDTO.F4201_VR02.Trim() : String.Empty).Append(" "); ;
            sb.Append(!String.IsNullOrWhiteSpace(pedidoDTO.F4201_DEL1) ? pedidoDTO.F4201_DEL1.Trim() : String.Empty);
            pedido.pedc_observaciones = sb.ToString();

            return pedido;
        }

        private tblPedidoDetalle fillDetalle(PedidoDTO pedidoDTO, String compania)
        {
            tblPedidoDetalle detalle = new tblPedidoDetalle();
            detalle.pedd_linea = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_LNID) ? Convert.ToDecimal(pedidoDTO.F4211_LNID) : 0;
            detalle.pedd_compania = !String.IsNullOrWhiteSpace(compania) ? compania.Trim() : String.Empty;
            detalle.pedd_producto = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_LITM) ? pedidoDTO.F4211_LITM : String.Empty;
            detalle.pedd_lote = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_LOTN) ? pedidoDTO.F4211_LOTN : String.Empty;
            detalle.pedd_cantidad = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_UORG) ? Math.Abs(Convert.ToDecimal(pedidoDTO.F4211_UORG)) : 0;

            detalle.pedd_despachoParcial = false;
            detalle.pedd_epro_codigo = String.Empty;
            detalle.pedd_loteUnico = false;
            detalle.pedd_serie = String.Empty;
          
            return detalle;
        }

    }
}
