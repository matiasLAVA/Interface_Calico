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

        //public Dictionary<String, int> GetMapTotalCantidadLinea(List<tblInformePedido> informes)
        //{
        //    Dictionary<String, int> mapTotalCantidadLinea = new Dictionary<String, int>();
        //    int cantidadOut = 0;

        //    foreach (tblInformePedido informe in informes)
        //    {
        //        foreach (tblInformePedidoDetalle detalle in informe.tblInformePedidoDetalle)
        //        {
        //            mapTotalCantidadLinea.TryGetValue(informe.ipec_numero + "_" + detalle.iped_linea, out cantidadOut);

        //            if (cantidadOut == 0)
        //            {
        //                mapTotalCantidadLinea.Add(informe.ipec_numero + "_" + detalle.iped_linea, Decimal.ToInt32(detalle.iped_cantidad));
        //            }
        //            else
        //            {
        //                mapTotalCantidadLinea[informe.ipec_numero + "_" + detalle.iped_linea] = cantidadOut + Decimal.ToInt32(detalle.iped_cantidad);
        //            }
        //        }
        //    }
        //    return mapTotalCantidadLinea;
        //}

        public Dictionary<String, List<tblInformePedidoDetalle>> getMapDetalles(List<tblInformePedido> informes, out Dictionary<String, int> mapTotalCantidadLinea)
        {
            Dictionary<String, List<tblInformePedidoDetalle>> map = new Dictionary<String, List<tblInformePedidoDetalle>>();
            List<tblInformePedidoDetalle> listInformeDetailOut;
            mapTotalCantidadLinea = new Dictionary<String, int>();
            int cantidadOut = 0;

            foreach (tblInformePedido informe in informes)
            {
                foreach (tblInformePedidoDetalle detalle in informe.tblInformePedidoDetalle)
                {
                    map.TryGetValue(informe.ipec_numero + "_" + detalle.iped_linea, out listInformeDetailOut);

                    if (listInformeDetailOut == null)
                    {
                        List<tblInformePedidoDetalle> listInformeDetail = new List<tblInformePedidoDetalle>();
                        listInformeDetail.Add(detalle);
                        map.Add(informe.ipec_numero + "_" + detalle.iped_linea, listInformeDetail);
                        mapTotalCantidadLinea.Add(informe.ipec_numero + "_" + detalle.iped_linea, Decimal.ToInt32(detalle.iped_cantidad));
                    }
                    else
                    {
                        listInformeDetailOut.Add(detalle);
                        mapTotalCantidadLinea.TryGetValue(informe.ipec_numero + "_" + detalle.iped_linea, out cantidadOut);
                        mapTotalCantidadLinea[informe.ipec_numero + "_" + detalle.iped_linea] = cantidadOut + Decimal.ToInt32(detalle.iped_cantidad);
                    }
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

        public void fillMapKO(Dictionary<int, String> mapIdsKO, List<tblInformePedidoDetalle> detalles,String error)
        {
            foreach(tblInformePedidoDetalle detalle in detalles)
            {
                mapIdsKO.Add(detalle.tblInformePedido.ipec_proc_id, error);
            }
        }

        public List<int> getIdsPedidos(List<tblInformePedidoDetalle> detalles)
        {
            List<int> listIds = new List<int>();
            foreach (tblInformePedidoDetalle detalle in detalles)
            {
                listIds.Add(detalle.tblInformePedido.ipec_proc_id);
            }

            return listIds;
        }

        public List<InformePedidoJson> MappingInformeByMap(tblInformePedidoDetalle detalles, String orderCompany, String lastStatus, String nextStatus, String version, int cantidad)
        {
            List<InformePedidoJson> jsonList = new List<InformePedidoJson>();
            InformePedidoDTO informeDTO = new InformePedidoDTO();
            tblInformePedido lastPedido = detalles.tblInformePedido;

            informeDTO.OrderCompany = orderCompany;
            informeDTO.OrderNumber = lastPedido.ipec_numero.ToString();
            informeDTO.OrderType = lastPedido.ipec_referenciaB;
            informeDTO.OrderLineNumber = detalles.iped_linea.ToString();
            informeDTO.Lot = Utils.GetValueOrEmpty(detalles.iped_lote);
            informeDTO.ItemNumber = detalles.iped_producto.TrimStart(new Char[] { '0' }).Trim(); // sin CEROS a la izquierda;
            informeDTO.ChgLastStatus = lastStatus;
            informeDTO.ChgReference = Utils.GetValueOrEmpty(lastPedido.ipec_referenciaA);
            informeDTO.ChgNextStatus = nextStatus;
            informeDTO.ChgDispatchQuantity = cantidad.ToString();
            informeDTO.ChgLot = Utils.GetValueOrEmpty(detalles.iped_lote);
            informeDTO.ChgDispatchDate = lastPedido.ipec_fechaFinProceso.ToString("yyyy/MM/dd");
            InformePedidoJson json = GetObjectJsonFromDTO(informeDTO);
            json.P554211I_Version = version;
            jsonList.Add(json);


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
