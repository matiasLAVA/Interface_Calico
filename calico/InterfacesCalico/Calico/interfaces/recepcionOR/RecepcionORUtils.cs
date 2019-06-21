﻿using Calico.common;
using Calico.interfaces.pedido;
using Calico.interfaces.pedidos;
using Calico.interfaces.recepcion;
using Calico.persistencia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.interfaces.recepcionOR
{
    class RecepcionORUtils : PedidoUtils
    {
        public void MappingPedidoDTORecepcion(List<PedidoDTO> pedidoDTOList, Dictionary<String, tblRecepcion> dictionary, String emplazamiento)
        {
            foreach (PedidoDTO pedidoDTO in pedidoDTOList)
            {
                tblRecepcion recepcion = null;
                dictionary.TryGetValue(pedidoDTO.F4201_DOCO, out recepcion);
                if (recepcion == null)
                {
                    /* CABEZERA */
                    recepcion = fillCabezera(pedidoDTO, emplazamiento);
                    /* DETALLE */
                    tblRecepcionDetalle detalle = fillDetalle(pedidoDTO);
                    recepcion.tblRecepcionDetalle.Add(detalle);
                    dictionary.Add(pedidoDTO.F4201_DOCO, recepcion);
                }
                else
                {
                    /* DETALLE */
                    tblRecepcionDetalle detalle = fillDetalle(pedidoDTO);
                    recepcion.tblRecepcionDetalle.Add(detalle);
                }
            }
        }

        private tblRecepcion fillCabezera(PedidoDTO pedidoDTO, String emplazamiento)
        {
            tblRecepcion recepcion = new tblRecepcion();
            recepcion.recc_contacto = String.Empty;
            recepcion.recc_despacho = String.Empty;
            recepcion.recc_ordenCompra = String.Empty;
            recepcion.recc_motivoDevolucion = String.Empty;
            recepcion.recc_observaciones = String.Empty;
            recepcion.recc_emplazamiento = emplazamiento;
            recepcion.recc_trec_codigo = pedidoDTO.F4201_DCTO;
            recepcion.recc_numero = pedidoDTO.F4201_DOCO;

            if (!String.IsNullOrWhiteSpace(pedidoDTO.F4201_OPDJ))
            {
                string result = DateTime.ParseExact(pedidoDTO.F4201_OPDJ, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
                recepcion.recc_fechaEntrega = Utils.ParseDate(result, "yyyy/MM/dd");
            }
            else
            {
                recepcion.recc_fechaEntrega = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
            }

            recepcion.recc_proveedor = !String.IsNullOrWhiteSpace(pedidoDTO.F4201_MCU) ? pedidoDTO.F4201_MCU.Trim() : String.Empty;

            // VERY HARDCODE
            recepcion.recc_fechaEmision = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");

            return recepcion;
        }

        private tblRecepcionDetalle fillDetalle(PedidoDTO pedidoDTO)
        {
            tblRecepcionDetalle detalle = new tblRecepcionDetalle();
            detalle.recd_serie = String.Empty;
            detalle.recd_linea = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_LNID) ? Convert.ToInt64(Convert.ToDouble(pedidoDTO.F4211_LNID)) * 1000 : 0;
            detalle.recd_lineaPedido = 0;
            detalle.recd_lote = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_LOTN) ? pedidoDTO.F4211_LOTN.Trim() : String.Empty;
            detalle.recd_cantidad = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_UORG) ? Convert.ToInt64(Convert.ToDouble(pedidoDTO.F4211_UORG)) : 0;
            detalle.recd_compania = !String.IsNullOrWhiteSpace(pedidoDTO.F4211_SRP1) ? pedidoDTO.F4211_SRP1.Trim() : String.Empty;

            if (!String.IsNullOrWhiteSpace(pedidoDTO.F4211_LITM) && pedidoDTO.F4211_LITM.Length > 15)
            {
                // VERY HARDCODE
                detalle.recd_producto = "99999999999999";
            }
            else
            {
                detalle.recd_producto = pedidoDTO.F4211_LITM;
            }


           detalle.recd_fechaVencimiento = Utils.ParseDate(Constants.FECHA_DEFAULT, "yyyy/MM/dd");
            

            // VERY HARDCODE
            detalle.recd_numeroPedido = "0";

            return detalle;
        }


    }
}
