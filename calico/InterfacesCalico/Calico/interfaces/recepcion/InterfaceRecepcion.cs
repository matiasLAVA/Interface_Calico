using Calico.common;
using Calico.persistencia;
using Calico.service;
using InterfacesCalico.generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calico.interfaces.recepcion
{
    class InterfaceRecepcion : InterfaceGeneric
    {
        private const String INTERFACE = Constants.INTERFACE_RECEPCION;

        private BianchiService service = new BianchiService();
        private TblRecepcionService serviceRecepcion = new TblRecepcionService();
        private RecepcionUtils recepcionUtils = new RecepcionUtils();

        public bool ValidateDate() => true;

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

            /* Obtenemos la fecha */
            if (Utils.IsInvalidateDates(dateTime, process.fecha_ultima))
            {
                service.finishProcessByError(process, Constants.FAILED_LOAD_DATES, INTERFACE);
                return false;
            }
            DateTime lastTime = Utils.GetDateToProcess(dateTime, process.fecha_ultima);

            /* Convierto DateTime a String formato YYYYMMDD */
            String lastStringTime = lastStringTime = Utils.ConvertDateTimeInString(lastTime);

            /* Cargamos archivo con parametros propios para cada interface */
            Console.WriteLine("Cargamos archivo de configuracion");
            if (!FilePropertyUtils.Instance.ReadFile(Constants.PROPERTY_FILE_NAME))
            {
                service.finishProcessByError(process, Constants.FAILED_LOAD_FILE, INTERFACE);
                return false;
            }

            /* Obtenemos usuario y contraseña del archivo para el servicio Rest */
            String urlPath = String.Empty;
            String user = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.USER);
            String pass = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.PASS);
            Console.WriteLine("Usuario del Servicio Rest: " + user);

            /* Obtenemos la URL del archivo */
            String url = FilePropertyUtils.Instance.GetValueString(INTERFACE + "." + Constants.URLS, Constants.INTERFACE_RECEPCION_URL);

            /* Armamos la URL con parametros */
            String tipoOrden = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.TIPO_ORDER);
            Dictionary<String, String> URLdictionary = new Dictionary<string, string>();
            URLdictionary.Add(Constants.PARAM_FECHA, lastStringTime);
            URLdictionary.Add(Constants.PARAM_TIPO_ORDER, tipoOrden);
            urlPath = Utils.BuildUrl(url, URLdictionary);

            Console.WriteLine("URL: " + urlPath);

            /* Obtenemos los datos */
            String myJsonString = Utils.SendRequest(urlPath, user, pass);

            /* Mapping */
            List<ReceptionDTO> receptionDTO = null;
            Dictionary<String, tblRecepcion> dictionary = new Dictionary<string, tblRecepcion>();
            String emplazamiento = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.EMPLAZAMIENTO);

            if (!String.Empty.Equals(myJsonString))
            {
                receptionDTO = recepcionUtils.MappingJsonRecepcion(myJsonString);
                if (receptionDTO.Any())
                {
                    recepcionUtils.MappingReceptionDTORecepcion(receptionDTO, dictionary, emplazamiento);
                }
                else
                {
                    service.finishProcessByError(process, Constants.NOT_DATA_FOUND, INTERFACE);
                    return false;
                }
            }
            else
            {
                service.finishProcessByError(process, Constants.FAILED_CALL_REST, INTERFACE);
                return false;
            }

            int count = 0;
            int countError = 0;
            int countAlreadyProcess = 0;
            int? tipoMensaje = 0;
            int tipoProceso = FilePropertyUtils.Instance.GetValueInt(INTERFACE, Constants.NUMERO_INTERFACE);
            int codigoCliente = FilePropertyUtils.Instance.GetValueInt(INTERFACE, Constants.NUMERO_CLIENTE);
            Console.WriteLine("Codigo de interface: " + tipoProceso);

            // Validamos si hay que insertar o descartar la recepcion
            foreach (KeyValuePair<string, tblRecepcion> entry in dictionary)
            {
                // ¿Ya está procesada?
                if (serviceRecepcion.IsAlreadyProcess(entry.Value.recc_emplazamiento, entry.Value.recc_almacen, entry.Value.recc_trec_codigo, entry.Value.recc_numero))
                {
                    Console.WriteLine("La recepcion " + entry.Value.recc_numero + " ya fue tratada, no se procesara");
                    countAlreadyProcess++;
                }
                // No está procesada! la voy a guardar
                else
                {
                    // LLamo al SP y seteo su valor a la cabecera y sus detalles
                    int recc_proc_id = serviceRecepcion.CallProcedure(tipoProceso, tipoMensaje);
                    entry.Value.recc_proc_id = recc_proc_id;

                    foreach (tblRecepcionDetalle detalle in entry.Value.tblRecepcionDetalle)
                    {
                        detalle.recd_compania = FilePropertyUtils.Instance.GetValueString(INTERFACE + "." + Constants.COMPANIA, detalle.recd_compania);
                        detalle.recd_proc_id = recc_proc_id;
                    }
                    // ¿La pude guardar?
                    Console.WriteLine("Procesando recepcion nro. de OT: " + entry.Value.recc_numero);
                    if (serviceRecepcion.Save(entry.Value))
                        count++;
                    else
                        countError++;
                }
            }

            Console.WriteLine("Finalizó el proceso de actualización de Recepciones");

            /* Agregamos datos faltantes de la tabla de procesos */
            Console.WriteLine("Preparamos los datos a actualizar en BIANCHI_PROCESS");
            process.fin = DateTime.Now;
            process.fecha_ultima = lastTime;
            process.cant_lineas = count;
            process.estado = Constants.ESTADO_OK;
            Console.WriteLine("Fecha_fin: " + process.fin);
            Console.WriteLine("Cantidad de Recepciones procesadas OK: " + process.cant_lineas);
            Console.WriteLine("Cantidad de Recepciones procesadas con ERROR: " + countError);
            Console.WriteLine("Cantidad de Recepciones evitadas: " + countAlreadyProcess);
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
