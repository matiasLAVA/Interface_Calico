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
    class TblInformeRecepcionDAO : CommonDAO, Dao<tblInformeRecepcion>
    {
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public DbSet<tblInformeRecepcion> FindAll()
        {
            throw new NotImplementedException();
        }

        public tblInformeRecepcion FindById(int id)
        {
            throw new NotImplementedException();
        }

        public bool Save(tblInformeRecepcion obj)
        {
            throw new NotImplementedException();
        }

        public void Update(tblInformeRecepcion obj)
        {
            throw new NotImplementedException();
        }

        public int CallProcedureArchivarInformeRecepcion(Nullable<int> id, ObjectParameter error)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                return context.INTERFAZ_ArchivarInformeRecepcion(id, error);
            }
        }

        public int CallProcedureInformarEjecucion(int? id, string mensaje, ObjectParameter error)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                return context.INTERFAZ_InformarEjecucion(id, mensaje, error);
            }
        }

        public List<tblInformeRecepcion> FindInformes(String emplazamiento, String[] almacenes, String[] tipos)
        {
            try
            {
                using (CalicoEntities context = new CalicoEntities())
                {
                    var query = (from R in context.tblInformeRecepcion
                                where R.irec_emplazamiento == emplazamiento
                                   && almacenes.Contains(R.irec_almacen)
                                   && tipos.Contains(R.irec_tipo)
                                select R).Include(D => D.tblInformeRecepcionDetalle);

                    Console.WriteLine("Query: (from R in context.tblInformeRecepcion" +
                                                  "where R.irec_emplazamiento == emplazamiento " +
                                                  "&& almacenes.Contains(R.irec_almacen) " +
                                                  "&& tipos.Contains(R.irec_tipo) " +
                                                  "select R).Include(D => D.tblInformeRecepcionDetalle);");

                    return query.ToList<tblInformeRecepcion>();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

    }
}
