using Calico.common;
using Calico.persistencia;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Calico.DAOs
{
    class BianchiProcessDAO : Dao<BIANCHI_PROCESS>
    {
        private CalicoEntities entity;
        private DbContextTransaction scope;

        public void Delete(int id)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                BIANCHI_PROCESS obj = new BIANCHI_PROCESS { id = id };
                context.BIANCHI_PROCESS.Attach(obj);
                context.BIANCHI_PROCESS.Remove(obj);
                context.SaveChanges();
            }
        }

        public DbSet<BIANCHI_PROCESS> FindAll()
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                return context.Set<BIANCHI_PROCESS>();
            }
        }

        public BIANCHI_PROCESS FindById(int id)
        {
            using (CalicoEntities entities = new CalicoEntities())
            {
                return entities.BIANCHI_PROCESS.Find(id);
            }
        }

        public bool Save(BIANCHI_PROCESS obj)
        {
            try
            {
                using (CalicoEntities context = new CalicoEntities())
                {
                    context.BIANCHI_PROCESS.Add(obj);
                    context.SaveChanges();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public void Update(BIANCHI_PROCESS obj)
        {
            var result = this.entity.BIANCHI_PROCESS.Find(obj.id);
            if (result == null) return;
            this.entity.Entry(result).CurrentValues.SetValues(obj);
            this.entity.SaveChanges();
        }

        public bool updateEnCurso(string interfaz)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                var query = from BP in context.BIANCHI_PROCESS
                            where BP.interfaz == interfaz
                            select BP;
                var result = query.FirstOrDefault<BIANCHI_PROCESS>();
                if (result == null) return false;
                result.estado = Constants.ESTADO_EN_CURSO;
                context.Entry(result);
                context.SaveChanges();
                return true;
            }
        }

        public BIANCHI_PROCESS findByName(string interfaz)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                var query = from BP in context.BIANCHI_PROCESS
                            where BP.interfaz == interfaz
                            select BP;

                return query.FirstOrDefault<BIANCHI_PROCESS>();
            }
        }

        public bool validarSiPuedoProcesar(string interfaz)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                var result = context.BIANCHI_PROCESS.Where(bp => bp.interfaz == interfaz).FirstOrDefault<BIANCHI_PROCESS>();
                if (result == null) return true;
                return !Constants.ESTADO_EN_CURSO.Equals(result.estado);
            }
        }

        public DateTime? getProcessDate(string interfaz)
        {
            using (CalicoEntities context = new CalicoEntities())
            {
                var result = context.BIANCHI_PROCESS.Where(bp => bp.interfaz == interfaz).FirstOrDefault<BIANCHI_PROCESS>();
                return result.fecha_ultima;
            }
        }

        public void LockRow(int id)
        {
            this.entity = new CalicoEntities();
            this.scope = this.entity.Database.BeginTransaction();
            this.entity.Database.ExecuteSqlCommand("SELECT * FROM BIANCHI_PROCESS WITH (ROWLOCK, UPDLOCK) where id = " + id);
        }

        public void UnlockRow()
        {
            scope.Commit();
            entity.Dispose();
        }

    }
}
