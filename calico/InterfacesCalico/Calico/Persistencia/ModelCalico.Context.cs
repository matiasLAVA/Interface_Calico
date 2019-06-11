﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Calico.persistencia
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Data.SqlClient;
    using System.Data;

    public partial class CalicoEntities : DbContext
    {
        public CalicoEntities()
            : base("name=CalicoEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BIANCHI_PROCESS> BIANCHI_PROCESS { get; set; }
        public virtual DbSet<tblSubCliente> tblSubCliente { get; set; }
        public virtual DbSet<tblRecepcion> tblRecepcion { get; set; }
        public virtual DbSet<tblRecepcionDetalle> tblRecepcionDetalle { get; set; }
        public virtual DbSet<tblHistoricoRecepcion> tblHistoricoRecepcion { get; set; }
        public virtual DbSet<tblPedido> tblPedido { get; set; }
        public virtual DbSet<tblPedidoDetalle> tblPedidoDetalle { get; set; }
        public virtual DbSet<tblInformeRecepcion> tblInformeRecepcion { get; set; }
        public virtual DbSet<tblInformeRecepcionDetalle> tblInformeRecepcionDetalle { get; set; }
        public virtual DbSet<tblHistoricoPedido> tblHistoricoPedido { get; set; }
        public virtual DbSet<tblInformePedido> tblInformePedido { get; set; }
        public virtual DbSet<tblInformePedidoDetalle> tblInformePedidoDetalle { get; set; }
        public virtual DbSet<tblProceso> tblProceso { get; set; }

        public virtual int INTERFAZ_CrearProceso(Nullable<int> tipoProceso, Nullable<int> tipoMensaje)
        {
            using (SqlConnection con = (SqlConnection)this.Database.Connection)
            {
                using (SqlCommand cmd = new SqlCommand("INTERFAZ_CrearProceso", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@tipoProceso", SqlDbType.Int).Value = tipoProceso;
                    cmd.Parameters.Add("@tipoMensaje", SqlDbType.Int).Value = tipoMensaje;

                    var returnParameter = cmd.Parameters.Add("@id", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    return Convert.ToInt32(id);
                }
            }
        }

        public virtual int INTERFAZ_ArchivarInformeRecepcion(Nullable<int> id, ObjectParameter error)
        {
            var idParameter = id.HasValue ?
                new ObjectParameter("id", id) :
                new ObjectParameter("id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("INTERFAZ_ArchivarInformeRecepcion", idParameter, error);
        }
    
        public virtual int INTERFAZ_InformarEjecucion(Nullable<int> id, string mensaje, ObjectParameter error)
        {
            var idParameter = id.HasValue ?
                new ObjectParameter("id", id) :
                new ObjectParameter("id", typeof(int));
    
            var mensajeParameter = mensaje != null ?
                new ObjectParameter("mensaje", mensaje) :
                new ObjectParameter("mensaje", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("INTERFAZ_InformarEjecucion", idParameter, mensajeParameter, error);
        }
    
        public virtual int INTERFAZ_ArchivarInformePedido(Nullable<int> id, ObjectParameter error)
        {
            var idParameter = id.HasValue ?
                new ObjectParameter("id", id) :
                new ObjectParameter("id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("INTERFAZ_ArchivarInformePedido", idParameter, error);
        }
    }
}
