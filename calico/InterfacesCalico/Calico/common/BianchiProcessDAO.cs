﻿using Calico.Persistencia;
using System.Data.Entity;
using System.Linq;

namespace Calico.common
{
    class BianchiProcessDAO : Dao<BIANCHI_PROCESS>
    {
        public void delete(int id)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                BIANCHI_PROCESS obj = new BIANCHI_PROCESS { id = id };
                context.BIANCHI_PROCESS.Attach(obj);
                context.BIANCHI_PROCESS.Remove(obj);
                context.SaveChanges();
            }
        }

        public DbSet<BIANCHI_PROCESS> findAll()
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                /* Obtengo todos los registros de la tabla de esta manera */
                var rows = context.Set<BIANCHI_PROCESS>();
                return rows;
            }
        }

        public BIANCHI_PROCESS findById(int id)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                return context.BIANCHI_PROCESS.Find(id);
            }
        }

        public void save(BIANCHI_PROCESS obj)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                context.BIANCHI_PROCESS.Add(obj);
                context.SaveChanges();
            }
        }

        public void update(BIANCHI_PROCESS obj)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                var result = context.BIANCHI_PROCESS.Find(obj.id);
                if (result == null) return;
                context.Entry(result).CurrentValues.SetValues(obj);
                context.SaveChanges();
            }
        }

        public bool updateEnCurso(string interfaz)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                var query = from BP in context.BIANCHI_PROCESS
                            where BP.@interface == interfaz
                            select BP;
                var result = query.FirstOrDefault<BIANCHI_PROCESS>();
                if (result == null) return false;
                result.estado = Constants.ESTADO_EN_CURSO;
                context.Entry(result);
                context.SaveChanges();
                return true;
            }
        }

        public bool validarSiPuedoProcesar(string interfaz)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                var result = context.BIANCHI_PROCESS.Where(bp => bp.@interface == interfaz).FirstOrDefault<BIANCHI_PROCESS>();
                if (result == null) return true;
                return !Constants.ESTADO_EN_CURSO.Equals(result.estado);
            }
        }

    }
}
