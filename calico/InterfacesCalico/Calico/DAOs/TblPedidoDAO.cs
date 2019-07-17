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
    class TblPedidoDAO : CommonDAO, Dao<tblPedido>
    {
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public DbSet<tblPedido> FindAll()
        {
            throw new NotImplementedException();
        }

        public tblPedido FindById(int id)
        {
            throw new NotImplementedException();
        }

        public bool Save(tblPedido obj)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                try
                {
                    context.tblPedido.Add(obj);
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
                    Console.WriteLine("Error insertando el pedido: " + dbe.Message);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Error desconocido insertando el pedido: " + ee.Message);
                }
                return true;
            }
        }

        public void Update(tblPedido obj)
        {
            throw new NotImplementedException();
        }

        public bool IsAlreadyProcess(String alm, String tipo, String letra, String sucursal, decimal numero)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                bool existTblPedido = context.tblPedido.Any((x => x.pedc_almacen == alm &&
                          x.pedc_tped_codigo == tipo &&
                          x.pedc_letra == letra &&
                          x.pedc_sucursal == sucursal &&
                          x.pedc_numero == numero));

                bool existTblHistPedido = context.tblHistoricoPedido.Any((x => x.hpec_almacen == alm &&
                          x.hpec_tped_codigo == tipo &&
                          x.hpec_letra == letra &&
                          x.hpec_sucursal == sucursal &&
                          x.hpec_numero == numero));

                return existTblPedido || existTblHistPedido;
            }
        }

    }
}
