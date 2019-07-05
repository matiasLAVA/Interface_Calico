using Calico.DAOs;
using Calico.persistencia;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Calico.service
{
    class TblRecepcionService
    {
        TblRecepcionDAO dao = new TblRecepcionDAO();

        public void Delete(int id)
        {
            dao.Delete(id);
        }

        public DbSet<tblRecepcion> FindAll()
        {
            return dao.FindAll();
        }

        public tblRecepcion FindById(int id)
        {
            return dao.FindById(id);
        }

        public bool Save(tblRecepcion obj) 
        {
            return dao.Save(obj);
        }

        public void Update(tblRecepcion obj)
        {
            dao.Update(obj);
        }

        public int CallProcedure(int? tipoProceso, int? tipoMensaje)
        {
            return dao.CallProcedure(tipoProceso, tipoMensaje);
        }

        public Boolean IsAlreadyProcess(String emplaz, String alm, String cod, String numero)
        {
            return dao.IsAlreadyProcess(emplaz, alm, cod, numero);
        }

    }
}
