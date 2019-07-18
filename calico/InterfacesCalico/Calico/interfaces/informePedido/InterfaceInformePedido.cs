using Calico.common;
using Calico.persistencia;
using Calico.service;
using InterfacesCalico.generic;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace Calico.interfaces.informePedido
{
    class InterfaceInformePedido : InterfaceGeneric
    {

        private const String INTERFACE = Constants.INTERFACE_INFORME_PEDIDOS;

        private BianchiService service = new BianchiService();
        private TblInformePedidoService serviceInformePedido = new TblInformePedidoService();
        private InformePedidoUtils informePedidoUtils = new InformePedidoUtils();

        public bool ValidateDate() => false;

        public bool Process(DateTime? dateTime)
        {
            Console.WriteLine("Comienzo del proceso para la interfaz " + INTERFACE);

            BIANCHI_PROCESS process = service.FindByName(INTERFACE);
            if (process == null)
            {
                Console.WriteLine("No hay configuracion en BIANCHI_PROCESS para la interface: " + INTERFACE);
                Console.WriteLine("Finalizamos la ejecucion de la interface: " + INTERFACE);
                return false;
            }

            /* Inicializamos los datos del proceso */
            Console.WriteLine("Inicializando los datos del proceso");
            process.inicio = DateTime.Now;
            process.maquina = Environment.MachineName;
            process.process_id = System.Diagnostics.Process.GetCurrentProcess().Id;
            Console.WriteLine("Inicio: " + process.inicio);
            Console.WriteLine("Maquina: " + process.maquina);
            Console.WriteLine("Process_id: " + process.process_id);

            /* Bloquea la row, para que no pueda ser actualizada por otra ejecucion de la misma interface */
            Console.WriteLine("Si hay otro proceso ejecutandose para la interface " + INTERFACE + " esperamos a que termine");
            Console.WriteLine("Bloqueando la row de BIANCHI_PROCESS, para la interfaz " + INTERFACE);
            if (!service.LockRow(process.id))
            {
                Console.WriteLine("No se pudo lockear la row para la interface " + INTERFACE + " se cancela la ejecucion");
                return false;
            }

            /* Cargamos archivo con parametros propios para cada interface */
            Console.WriteLine("Cargamos archivo de configuracion");
            if (!FilePropertyUtils.Instance.ReadFile(Constants.PROPERTY_FILE_NAME))
            {
                service.finishProcessByError(process, Constants.FAILED_LOAD_FILE, INTERFACE);
                return false;
            }

            // INICIO BUSQUEDA DE DATOS
            String emplazamiento = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.EMPLAZAMIENTO);
            String orderCompany = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.ORDER_COMPANY);
            String lastStatus = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.INTERFACE_INFORME_PEDIDO_LAST_STATUS);
            String nextStatus = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.INTERFACE_INFORME_PEDIDO_NEXT_STATUS);
            String version = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.INTERFACE_INFORME_PEDIDO_P554211I_VERSION);
            int tipoProceso = FilePropertyUtils.Instance.GetValueInt(INTERFACE, Constants.TIPO_PROCESO);

            var almacenes = FilePropertyUtils.Instance.GetValueArrayString(INTERFACE + "." + Constants.ALMACEN);
            var tipos = FilePropertyUtils.Instance.GetValueArrayString(INTERFACE + "." + Constants.TIPO);

            List<tblInformePedido> informes = serviceInformePedido.FindInformes(emplazamiento, almacenes, tipos, tipoProceso);
            List<InformePedidoJson> jsonList = null;

            /* Obtenemos usuario y contraseña del archivo para el servicio Rest */
            String urlPath = String.Empty;
            String user = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.USER);
            String pass = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.PASS);
            Console.WriteLine("Usuario del Servicio Rest: " + user);

            /* Obtenemos la URL del archivo */
            String url = FilePropertyUtils.Instance.GetValueString(INTERFACE + "." + Constants.URLS, Constants.INTERFACE_INFORME_PEDIDO_URL);

            int count = 0;
            int countError = 0;
            Boolean callArchivar;

            foreach (tblInformePedido informe in informes)
            {
                callArchivar = true;
                jsonList = informePedidoUtils.MappingInforme(informe, orderCompany, lastStatus, nextStatus, version);

                if (jsonList.Any())
                {
                    Console.WriteLine("Se llevara a cabo el envio al servicio REST de los detalles de la cabecera: " + informe.ipec_proc_id);
                    foreach (InformePedidoJson json in jsonList)
                    {
                        var jsonString = informePedidoUtils.JsonToString(json);
                        Console.WriteLine("Se enviara el siguiente Json al servicio REST: ");
                        Console.WriteLine(jsonString);
                        /* Send request */
                        if (!(informePedidoUtils.SendRequestPost(url, user, pass, jsonString)))
                        {
                            Console.WriteLine("Se llamara al procedure para informar el error");
                            serviceInformePedido.CallProcedureInformarEjecucion(informe.ipec_proc_id, InformePedidoUtils.LAST_ERROR, new ObjectParameter("error", typeof(String)));
                            callArchivar = false;
                            countError++;
                        }
                        else
                        {
                            Console.WriteLine("El servicio REST retorno OK: " + jsonString);
                            count++;
                        }
                    }

                    if (callArchivar)
                    {
                        Console.WriteLine("Se llamara al procedure para archivar el informe");
                        serviceInformePedido.CallProcedureArchivarInformePedido(informe.ipec_proc_id, new ObjectParameter("error", typeof(String)));
                    }

                }
                else
                {
                    Console.WriteLine("No se encontraron detalles para la cabecera: " + informe.ipec_proc_id);
                }

            }

            Console.WriteLine("Finalizó el proceso de actualización de Recepciones");

            /* Agregamos datos faltantes de la tabla de procesos */
            Console.WriteLine("Preparamos los datos a actualizar en BIANCHI_PROCESS");
            process.fin = DateTime.Now;
            process.fecha_ultima = DateTime.Now;
            process.cant_lineas = count;
            process.estado = Constants.ESTADO_OK;
            Console.WriteLine("Fecha_fin: " + process.fin);
            Console.WriteLine("Cantidad de Recepciones procesadas OK: " + process.cant_lineas);
            Console.WriteLine("Cantidad de Recepciones procesadas con ERROR: " + countError);
            Console.WriteLine("Estado: " + process.estado);

            /* Actualizamos la tabla BIANCHI_PROCESS */
            Console.WriteLine("Actualizamos BIANCHI_PROCESS");
            service.Update(process);

            /* Liberamos la row, para que la tome otra interface */
            Console.WriteLine("Se libera la row de BIANCHI_PROCESS");
            service.UnlockRow();

            Console.WriteLine("Fin del proceso, para la interfaz " + INTERFACE);
            Console.WriteLine("Proceso Finalizado correctamente");

            return true;


        }

    }
}
