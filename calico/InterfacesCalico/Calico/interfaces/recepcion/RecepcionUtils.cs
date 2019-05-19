﻿using Calico.common;
using Calico.persistencia;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Calico.interfaces.recepcion
{
    class RecepcionUtils
    {
        public String BuildUrl(String urlParam, String fecha)
        {
            Dictionary<String, String> dictionary = new Dictionary<string, string>();
            dictionary.Add(Constants.PARAM_FECHA, fecha);
            return Utils.BuildUrl(urlParam, dictionary);
        }

        public List<ReceptionDTO> MappingJsonRecepcion(String myJsonString)
        {
            var jc = JsonConvert.DeserializeObject<JObject>(myJsonString);

            JArray rowset = jc.Value<JObject>(Constants.JSON_PREFIX + Constants.JSON_SUBFIX_RECEPTION)
                                 .Value<JObject>(Constants.JSON_TAG_DATA)
                                 .Value<JObject>(Constants.JSON_TAG_GRIDDATA)
                                 .Value<JArray>(Constants.JSON_TAG_ROWSET);

            if (rowset != null && rowset.Count > 0)
            {
                return rowset.ToObject<List<ReceptionDTO>>() as List<ReceptionDTO>;
            }

            return new List<ReceptionDTO>();
        }

        public void MappingReceptionDTORecepcion(List<ReceptionDTO> receptionDTOList, Dictionary<String, tblRecepcion> dictionary, String emplazamiento, String almacen, String tipo, String compania)
        {
            foreach(ReceptionDTO receptionDTO in receptionDTOList)
            {
                tblRecepcion recepcion = null;
                dictionary.TryGetValue(receptionDTO.F4201_DOCO, out recepcion);
                if (recepcion == null)
                {
                    /* CABEZERA */
                    recepcion = fillCabezera(receptionDTO, emplazamiento, almacen, tipo, compania);
                    /* DETALLE */
                    tblRecepcionDetalle detalle = fillDetalle(receptionDTO, compania);
                    recepcion.tblRecepcionDetalle.Add(detalle);
                    dictionary.Add(receptionDTO.F4201_DOCO, recepcion);
                }
                else
                {
                    /* DETALLE */
                    tblRecepcionDetalle detalle = fillDetalle(receptionDTO, compania);
                    recepcion.tblRecepcionDetalle.Add(detalle);
                }
            }
        }

        private tblRecepcion fillCabezera(ReceptionDTO receptionDTO, String emplazamiento, String almacen, String tipo, String compania)
        {
            tblRecepcion recepcion = new tblRecepcion();
            recepcion.recc_emplazamiento = emplazamiento;
            recepcion.recc_almacen = almacen;
            recepcion.recc_trec_codigo = tipo;
            recepcion.recc_numero = receptionDTO.F4201_DOCO;

            if (!String.IsNullOrWhiteSpace(receptionDTO.F4201_OPDJ))
            {
                string result = DateTime.ParseExact(receptionDTO.F4201_OPDJ, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
                recepcion.recc_fechaEntrega = Utils.ParseDate(result, "yyyy/MM/dd");
            }
            else
            {
                recepcion.recc_fechaEntrega = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
            }

            if(receptionDTO.F4211_MCU != null)
                recepcion.recc_proveedor = receptionDTO.F4211_MCU.Trim();

            // VERY HARDCODE
            recepcion.recc_fechaEmision = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");

            return recepcion;
        }

        private tblRecepcionDetalle fillDetalle(ReceptionDTO receptionDTO, String compania)
        {
            tblRecepcionDetalle detalle = new tblRecepcionDetalle();
            detalle.recd_compania = compania;
            detalle.recd_lineaPedido = Convert.ToInt64(Convert.ToDouble(receptionDTO.F4211_LNID));
            detalle.recd_lote = receptionDTO.F4211_LOTN;
            detalle.recd_cantidad = Convert.ToInt64(Convert.ToDouble(receptionDTO.F4211_UORG));

            // VERY HARDCODE
            if (!String.IsNullOrWhiteSpace(receptionDTO.F4211_LITM) && receptionDTO.F4211_LITM.Length > 15)
            {
                detalle.recd_producto = "99999999999999";
            }
            else
            {
                detalle.recd_producto = receptionDTO.F4211_LITM;
            }

            if (!String.IsNullOrWhiteSpace(receptionDTO.F4108_MMEJ))
            {
                string result = DateTime.ParseExact(receptionDTO.F4108_MMEJ, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
                detalle.recd_fechaVencimiento = Utils.ParseDate(result, "yyyy/MM/dd");
            }
            else
            {
                detalle.recd_fechaVencimiento = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
            }

            // VERY HARDCODE
            detalle.recd_numeroPedido = "1";

            return detalle;
        }

    }
}
