using Calico.persistencia;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace Calico.DAOs
{
    class TblProductoDAO : CommonDAO, Dao<tblProducto>
    {
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public DbSet<tblProducto> FindAll() => throw new NotImplementedException();

        public tblProducto FindById(int id) => throw new NotImplementedException();

        public void Update(tblProducto obj) => throw new NotImplementedException();

        public bool Save(tblProducto obj)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                try
                {
                    context.tblProducto.Add(obj);
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
                    Console.WriteLine("Error insertando el producto: " + dbe.Message);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Error desconocido insertando el producto: " + ee.Message);
                }
                return true;
            }
        }

    }

}
