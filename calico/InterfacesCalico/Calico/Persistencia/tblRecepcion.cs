//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class tblRecepcion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblRecepcion()
        {
            this.tblRecepcionDetalle = new HashSet<tblRecepcionDetalle>();
        }
    
        public int recc_proc_id { get; set; }
        public string recc_emplazamiento { get; set; }
        public string recc_almacen { get; set; }
        public string recc_trec_codigo { get; set; }
        public string recc_numero { get; set; }
        public System.DateTime recc_fechaEmision { get; set; }
        public System.DateTime recc_fechaEntrega { get; set; }
        public string recc_proveedor { get; set; }
        public string recc_contacto { get; set; }
        public string recc_despacho { get; set; }
        public string recc_ordenCompra { get; set; }
        public string recc_motivoDevolucion { get; set; }
        public string recc_observaciones { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblRecepcionDetalle> tblRecepcionDetalle { get; set; }
        public virtual tblProceso tblProceso { get; set; }
    }
}
