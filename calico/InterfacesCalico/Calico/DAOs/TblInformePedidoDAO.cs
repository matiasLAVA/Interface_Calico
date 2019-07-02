using Calico.persistencia;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.DAOs
{
    class TblInformePedidoDAO : CommonDAO, Dao<tblInformePedido>
    {
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public DbSet<tblInformePedido> FindAll()
        {
            throw new NotImplementedException();
        }

        public tblInformePedido FindById(int id)
        {
            throw new NotImplementedException();
        }

        public bool Save(tblInformePedido obj)
        {
            throw new NotImplementedException();
        }

        public void Update(tblInformePedido obj)
        {
            throw new NotImplementedException();
        }

        public int CallProcedureInformarEjecucion(int? id, string mensaje, ObjectParameter error)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                return context.INTERFAZ_InformarEjecucion(id, mensaje, error);
            }
        }

        public int CallProcedureArchivarInformePedido(int? id, ObjectParameter error)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                return context.INTERFAZ_ArchivarInformePedido(id, error);
            }
        }


        public List<tblInformePedido> FindInformes(String emplazamiento, String[] almacenes, String[] tipos, int tipoProceso)
        {
            try
            {
                using (CalicoEntities context = new CalicoEntities())
                {
                    var query = (from R in context.tblInformePedido
                                 join P in context.tblProceso on R.ipec_proc_id equals P.proc_id
                                 //where R.ipec_proc_id == tipoProceso              JHV 
                                 where P.proc_tipoProceso == tipoProceso
                                    && R.ipec_emplazamiento == emplazamiento
                                    && almacenes.Contains(R.ipec_almacen)
                                    && tipos.Contains(R.ipec_tipo)
                                 select R).Include(D => D.tblInformePedidoDetalle);

                    Console.WriteLine("Query: " + @"(from R in context.tblInformePedido
                                 join P in context.tblProceso on R.ipec_proc_id equals P.proc_id
                                 where P.proc_tipoProceso == tipoProceso
                                    && R.ipec_emplazamiento == emplazamiento
                                    && almacenes.Contains(R.ipec_almacen)
                                    && tipos.Contains(R.ipec_tipo)
                                 select R).Include(D => D.tblInformePedidoDetalle)");

                    return query.ToList<tblInformePedido>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

    }
}
