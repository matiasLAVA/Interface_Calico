using Calico.DAOs;
using Calico.persistencia;
using System.Data.Entity;

namespace Calico.service
{
    class TblProductoService
    {

        TblProductoDAO dao = new TblProductoDAO();

        public int CallProcedure(int? tipoProceso, int? tipoMensaje)
        {
            return dao.CallProcedure(tipoProceso, tipoMensaje);
        }

        public void Delete(int id)
        {
            dao.Delete(id);
        }

        public DbSet<tblProducto> FindAll()
        {
            return dao.FindAll();
        }

        public tblProducto FindById(int id)
        {
            return dao.FindById(id);
        }

        public bool Save(tblProducto obj)
        {
            return dao.Save(obj);
        }

        public void Update(tblProducto obj)
        {
            dao.Update(obj);
        }


    }
}
