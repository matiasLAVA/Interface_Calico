using Calico.common;
using Calico.persistencia;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Calico.interfaces.informePedido
{
    class InformePedidoUtils
    {

        public static String LAST_ERROR = String.Empty;

        public Dictionary<decimal, List<tblInformePedidoDetalle>> getMapDetalles(tblInformePedido informe)
        {
            Dictionary<decimal, List<tblInformePedidoDetalle>> map = new Dictionary<decimal, List<tblInformePedidoDetalle>>();
            List<tblInformePedidoDetalle> listInformeDetailOut;
            List<tblInformePedidoDetalle> listInformeDetail;

            foreach (tblInformePedidoDetalle detalle in informe.tblInformePedidoDetalle)
            {
                map.TryGetValue(detalle.iped_linea, out listInformeDetailOut);

                if(listInformeDetailOut == null)
                {
                    listInformeDetail = new List<tblInformePedidoDetalle>();
                    listInformeDetail.Add(detalle);
                    map.Add(detalle.iped_linea, listInformeDetail);
                }
                else
                {
                    listInformeDetailOut.Add(detalle);
                }
            }

            return map;
        }

        public string getCantidad(List<tblInformePedidoDetalle> listDetail)
        {
            int sum = 0;
            foreach(tblInformePedidoDetalle detail in listDetail)
            {
                sum += Decimal.ToInt32(detail.iped_cantidad);
            }

            return sum.ToString();
        }

        public List<InformePedidoJson> MappingInformeByMap(tblInformePedido informe, Dictionary<decimal, List<tblInformePedidoDetalle>> map, String orderCompany, String lastStatus, String nextStatus, String version)
        {
            List<InformePedidoJson> jsonList = new List<InformePedidoJson>();
            List<tblInformePedidoDetalle> listDetailOut;
            foreach (var key in map.Keys)
            {
                map.TryGetValue(key, out listDetailOut);
                tblInformePedidoDetalle first = listDetailOut[0];

                InformePedidoDTO informeDTO = new InformePedidoDTO();
                informeDTO.OrderCompany = orderCompany;
                informeDTO.OrderNumber = informe.ipec_numero.ToString();
                informeDTO.OrderType = informe.ipec_referenciaB;
                informeDTO.OrderLineNumber = first.iped_linea.ToString();
                informeDTO.Lot = Utils.GetValueOrEmpty(first.iped_lote);
                informeDTO.ItemNumber = first.iped_producto.TrimStart(new Char[] { '0' }).Trim(); // sin CEROS a la izquierda;
                informeDTO.ChgLastStatus = lastStatus;
                informeDTO.ChgReference = Utils.GetValueOrEmpty(informe.ipec_referenciaA);
                informeDTO.ChgNextStatus = nextStatus;
                informeDTO.ChgDispatchQuantity = getCantidad(listDetailOut);
                informeDTO.ChgLot = Utils.GetValueOrEmpty(first.iped_lote);
                informeDTO.ChgDispatchDate = informe.ipec_fechaFinProceso.ToString("yyyy/MM/dd");
                InformePedidoJson json = GetObjectJsonFromDTO(informeDTO);
                json.P554211I_Version = version;
                jsonList.Add(json);
            }

            return jsonList;
        }

        public List<InformePedidoJson> MappingInforme(tblInformePedido informe, String orderCompany, String lastStatus, String nextStatus, String version)
        { 
            List<InformePedidoJson> jsonList = new List<InformePedidoJson>();
            foreach (tblInformePedidoDetalle detalle in informe.tblInformePedidoDetalle)
            { 
                InformePedidoDTO informeDTO = new InformePedidoDTO();
                informeDTO.OrderCompany = orderCompany;
                informeDTO.OrderNumber = informe.ipec_numero.ToString();
                informeDTO.OrderType = informe.ipec_referenciaB;
                informeDTO.OrderLineNumber = detalle.iped_linea.ToString();
                informeDTO.Lot = Utils.GetValueOrEmpty(detalle.iped_lote);
                informeDTO.ItemNumber = detalle.iped_producto.TrimStart(new Char[] { '0' }).Trim(); // sin CEROS a la izquierda;
                informeDTO.ChgLastStatus = lastStatus;
                informeDTO.ChgReference = Utils.GetValueOrEmpty(informe.ipec_referenciaA);
                informeDTO.ChgNextStatus = nextStatus;
                informeDTO.ChgDispatchQuantity = Decimal.ToInt32(detalle.iped_cantidad).ToString();
                informeDTO.ChgLot = Utils.GetValueOrEmpty(detalle.iped_lote);
                informeDTO.ChgDispatchDate = informe.ipec_fechaFinProceso.ToString("yyyy/MM/dd");
                InformePedidoJson json = GetObjectJsonFromDTO(informeDTO);
                json.P554211I_Version = version;
                jsonList.Add(json);
            }

            return jsonList;
        }

        public String JsonToString(InformePedidoJson obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return json;
        }

        public InformePedidoJson GetObjectJsonFromDTO(InformePedidoDTO detalle)
        {
            List<InformePedidoDTO> list = new List<InformePedidoDTO>();
            list.Add(detalle);
            return new InformePedidoJson(list);
        }

        public bool ExistChildrenInJson(String jsonString, String father, String children)
        {
            return false;
        }

        public Boolean SendRequestPost(string url, String user, String pass, String json)
        {
            String myJsonString = String.Empty;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
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
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        myJsonString = reader.ReadToEnd();
                        return true;
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)e.Response;
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    HandleErrorRest(reader.ReadToEnd(), out LAST_ERROR);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return false;

        }

        public void HandleErrorRest(String myJsonString, out string error)
        {
            JObject json = JObject.Parse(myJsonString);

            Console.WriteLine("Servicio Rest KO");
            Console.WriteLine("----------------");
            Console.WriteLine("Detalle: ");
            Console.WriteLine(json.ToString());
            error = json.ToString();
        }

    }
}
