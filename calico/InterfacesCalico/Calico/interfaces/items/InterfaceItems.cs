using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.interfaces.items
{
    class InterfaceItems
    {
//        Hola Damián, hola Matías,

//La info de la interfaz 8 Items(similar a clientes) así pueden trabajar en el desarrollo.

//Ejemplo url: 
//http://200.61.42.186:8091/jderest/v2/dataservice/table/F4101?$filter=F4101.UPMJ%20GT%2001012019&%24field=F4101.SRP1&%24field=F4101.LITM&%24field=F4101.DSC1&%24field=F4101.DSC2&%24limit=999999
//Nombre interfaz : Items

//Tablas : 
//tblProceso
//+ con
//tblProducto prod_proc_id Id Correlativo.Se recupera con Stored Procedure.Debe ser igual al id de tblProceso.
//tblProducto prod_compania              Compañía REST: F4101_SRP1 (tipo producto) + se busca en configuración Items.Compania
//tblProducto prod_codigo Código                      REST: F4101_LITM
//tblProducto prod_descripcion Descripción              REST: F4101_DSC1
//tblPedido prod_descripcionFantasia Descripción fantasía REST: F4101_DSC2

//La tabla de items (producto) tiene muchos campos.
//Los no indicados, como en las otras interfaces, grabarlos en 0 o "" según corresponda.
//Hay un tema abierto con los campos unidades de medida y volumen que tiene que veré con Matías.
//Apenas tenga la definición se las paso.

//Luego comparto un doc, que estoy armando a medida que hago las pruebas con la información concisa de como quedaron todas las interfaces.


//* Los bugs o modificaciones que surjan en la semana tendrán siempre prioridad sobre items.

//Cualquier duda lo vemos.


//Saludos,
//Jorge
    }
}
