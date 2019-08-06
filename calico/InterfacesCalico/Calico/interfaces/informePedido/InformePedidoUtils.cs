using Calico.common;
using Calico.persistencia;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Calico.interfaces.informePedido
{
    class InformePedidoUtils
    {

        public static String LAST_ERROR = String.Empty;

        public List<InformePedidoJson> MappingInforme(tblInformePedido informe, String orderCompany, String lastStatus,String nextStatus, String version)
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

        public bool ExistErrorInJson(String jsonString)
        {
            try
            {
                JObject jsonObj = JObject.Parse(jsonString);
                String data = jsonObj[Constants.SERVICE_REQUEST_1][Constants.VALID_JSON_PREFIX][Constants.JSON_TAG_DATA].ToString();
                return false;
            }
            catch (Exception)
            {
                HandleErrorRest(jsonString, out LAST_ERROR);
                return true;
            }
        }

        public String SendRequestPost(string url, String user, String pass, String json)
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
                        return myJsonString;
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

            return String.Empty;

        }



        public void HandleErrorRest(String myJsonString, out string error)
        {
            JObject json = JObject.Parse(myJsonString);
            error = json.ToString();
        }

    }
}
