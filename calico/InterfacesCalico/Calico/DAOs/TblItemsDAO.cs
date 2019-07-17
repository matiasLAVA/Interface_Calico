using Calico.persistencia;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.DAOs
{
    class TblRecepcionDAO : CommonDAO, Dao<tblRecepcion>
    {
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public DbSet<tblRecepcion> FindAll()
        {
            throw new NotImplementedException();
        }

        public tblRecepcion FindById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(tblRecepcion obj)
        {
            throw new NotImplementedException();
        }

        public bool Save(tblRecepcion obj)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                try
                {
                    context.tblRecepcion.Add(obj);
                    context.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.Error.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    return false;
                }
                catch (DbUpdateException dbe)
                {
                    Console.WriteLine("Error insertando la recepcion:" + dbe.Message);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Error desconocido insertando la recepcion:" + ee.Message);
                }
            }
            return true;
        }

        /// <summary>
        /// Verifica que no se haya procesado esta recepcion con anterioridad
        /// </summary>
        /// <param name="emplaz"></param>
        /// <param name="alm"></param>
        /// <param name="cod"></param>
        /// <param name="numero"></param>
        /// <returns>Retorna TRUE si ya fue procesada esta Recepcion</returns>
        public bool IsAlreadyProcess(String emplaz, String alm, String cod, String numero)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                int count = context.tblRecepcion
                   .Where(x => x.recc_emplazamiento == emplaz &&
                          x.recc_almacen == alm &&
                          x.recc_trec_codigo == cod &&
                          x.recc_numero == numero)
                   .Count<tblRecepcion>();

                if (count == 0)
                {
                    count += context.tblHistoricoRecepcion
                       .Where(x => x.hrec_emplazamiento == emplaz &&
                              x.hrec_almacen == alm &&
                              x.hrec_trec_codigo == cod &&
                              x.hrec_numero == numero)
                       .Count<tblHistoricoRecepcion>();
                }
                else
                {
                    return true;
                }

                return count > 0;
            }
        }

    }
}
