//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Calico
{
    using System;
    using System.Collections.Generic;
    
    public partial class BIANCHI_PROCESS
    {
        public int id { get; set; }
        public Nullable<System.DateTime> inicio { get; set; }
        public Nullable<System.DateTime> fin { get; set; }
        public Nullable<decimal> cant_lineas { get; set; }
        public string estado { get; set; }
        public Nullable<decimal> process_id { get; set; }
        public string maquina { get; set; }
        public string @interface { get; set; }
        public Nullable<System.DateTime> fecha_ultima { get; set; }
    }
}
