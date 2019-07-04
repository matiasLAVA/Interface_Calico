using Calico.common;
using Calico.interfaces.recepcion;
using Calico.persistencia;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Calico.interfaces.informeRecepcion
{
    class InformeRecepcionUtils
    {
        public static String LAST_ERROR = String.Empty;

        public String JsonToString(InformeRecepcionJson obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return json;
        }

        public InformeRecepcionJson GetObjectJsonFromDTO(InformeRecepcionDTO detalle, String receiptsVersion)
        {
            List<InformeRecepcionDTO> list = new List<InformeRecepcionDTO>();
            list.Add(detalle);
            return new InformeRecepcionJson(list, receiptsVersion);
        }

        public bool ExistChildrenInJson(String jsonString, String father, String children)
        {
            JObject jsonObj = JObject.Parse(jsonString);
            if (jsonObj[father] != null)
            {
                return jsonObj[father].Children().Where(child => child[children] != null).Any();
            }
            else
            {
                return false;
            }
        }

        public Boolean SendRequestPost(string url, String user, String pass, String json)
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
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        myJsonString = reader.ReadToEnd();

                        if (ExistChildrenInJson(myJsonString, Constants.INTERFACE_REPEATING_REQUEST, Constants.INTERFACE_RECEIPT_DOCUMENT))
                        {
                            return true;
                        }
                        else
                        {
                            handleErrorRest(myJsonString, out LAST_ERROR);
                            return false;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)e.Response;
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    handleErrorRest(reader.ReadToEnd(), out LAST_ERROR);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return false;

        }

        public void handleErrorRest(String myJsonString, out string error)
        {
                JObject json = JObject.Parse(myJsonString);

                Console.WriteLine("Servicio Rest KO");
                Console.WriteLine();
                Console.WriteLine("Detalle: ");
                Console.WriteLine(json["message"]);
                error = json["message"].ToString();
        }

        internal List<InformeRecepcionJson> MappingInforme(tblInformeRecepcion informe, String OrderCompany, String OrderType, String receiptsVersion)
        {
            List<InformeRecepcionJson> jsonList = new List<InformeRecepcionJson>();

            foreach (tblInformeRecepcionDetalle detalle in informe.tblInformeRecepcionDetalle)
            {
                InformeRecepcionDTO informeDTO = new InformeRecepcionDTO();

                informeDTO.OrderCompany = OrderCompany;
                informeDTO.OrderType = OrderType;
                int order;
                informeDTO.OrderNumber = Int32.TryParse(informe.irec_numero, out order) ? order.ToString() : String.Empty;
                String orderLine = detalle.ired_linea > 0 ? (detalle.ired_linea / 1000).ToString() : String.Empty;
                informeDTO.OrderLine = orderLine.Replace(",",".");
                informeDTO.QuantityToRecieve = (detalle.ired_cantidadRecibida.ToString()).Replace(",", ".");
                DateTime receiptDate = informe.irec_fecha ?? Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
                informeDTO.ReceiptDate = receiptDate.ToString("yyyy/MM/dd");
                informeDTO.Lot = detalle.ired_lote != null ? detalle.ired_lote.Trim() : String.Empty;

                jsonList.Add(GetObjectJsonFromDTO(informeDTO, receiptsVersion));
            }

            return jsonList;
        }
    }
}
