﻿using Calico.DAOs;
using Calico.persistencia;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.service
{
    class TblInformePedidoService
    {
        TblInformePedidoDAO dao = new TblInformePedidoDAO();

        public int CallProcedureArchivarInformeRecepcion(int? id, ObjectParameter error)
        {
            // return dao.CallProcedureArchivarInformeRecepcion(id, error);
            return 0;
        }

        public int CallProcedureInformarEjecucion(int? id, string mensaje, ObjectParameter error)
        {
            return dao.CallProcedureInformarEjecucion(id, mensaje, error);
        }

        public int CallProcedureArchivarInformePedido(int? id,ObjectParameter error)
        {
            return dao.CallProcedureArchivarInformePedido(id, error);
        }

        public List<tblInformePedido> FindInformes(String emplazamiento, String almacen, String tipo)
        {
            return dao.FindInformes(emplazamiento, almacen, tipo);
        }

    }
}
