using Calico.common;
using System;
using Nini.Config;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using InterfacesCalico.generic;
using Calico.service;
using Calico.persistencia;

namespace Calico.interfaces.clientes
{
    public class InterfaceCliente : InterfaceGeneric
    {
        private const String INTERFACE = Constants.INTERFACE_CLIENTES;

        private BianchiService service = new BianchiService();
        private TblSubClienteService serviceCliente = new TblSubClienteService();
        private ClientesUtils clientesUtils = new ClientesUtils();

        public bool ValidateDate() => true;

        public bool Process(DateTime? dateTime)
        {
            Console.WriteLine("Comienzo del proceso para la interfaz " + INTERFACE);
            DateTime lastTime;
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
            service.LockRow(process.id);

            /* Obtenemos la fecha */
            if (Utils.IsInvalidateDates(dateTime, process.fecha_ultima))
            {
                service.UnlockRow();
                return false;
            }
            lastTime = Utils.GetDateToProcess(dateTime, process.fecha_ultima);

            if (DateTime.Now > lastTime)
            {
                lastTime = DateTime.Now;
            }

            /* Convierto DateTime a String */
            String lastStringTime = Utils.ConvertDateTimeInString(lastTime);

            /* Cargamos archivo con parametros propios para cada interface */
            Console.WriteLine("Cargamos archivo de configuracion");
            if (!FilePropertyUtils.Instance.ReadFile(Constants.PROPERTY_FILE_NAME))
            {
                service.finishProcessByError(process, Constants.FAILED_LOAD_FILE, INTERFACE);
                return false;
            }

            /* Obtenemos las keys de las URLs del archivo externo */
            String[] URLkeys = FilePropertyUtils.Instance.GetKeysArrayString(INTERFACE + "." + Constants.URLS);

            /* Preparamos la URL con sus parametros y llamamos al servicio */
            String urlPath = String.Empty;
            String user = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.USER);
            String pass = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.PASS);
            Console.WriteLine("Usuario del Servicio Rest: " + user);

            /* Obtenemos las URLs, las armamos con sus parametros, obtenemos los datos y armamos los objetos Clientes */
            Dictionary<String, tblSubCliente> diccionary = new Dictionary<string, tblSubCliente>();
            foreach (String key in URLkeys)
            {
                // Obtenemos las URLs
                String url = FilePropertyUtils.Instance.GetValueString(INTERFACE + "." + Constants.URLS, key);
                // Armamos la URL
                urlPath = clientesUtils.BuildUrl(url, key, lastStringTime);
                Console.WriteLine("Url: " + urlPath);
                // Obtenemos los datos
                String myJsonString = Utils.SendRequest(urlPath, user, pass);
                // Armamos los objetos Clientes
                if (!String.Empty.Equals(myJsonString))
                {
                    clientesUtils.MappingCliente(myJsonString, key, diccionary);
                }
                else
                {
                    Console.WriteLine("Fallo el llamado al Rest Service");
                    Console.WriteLine("Finalizamos la ejecucion de la interface: " + INTERFACE);
                    service.UnlockRow();
                    return false;
                }
            }

            // LLamando al SP por cada cliente
            int? tipoProceso = FilePropertyUtils.Instance.GetValueInt(INTERFACE, Constants.NUMERO_INTERFACE);

            int? tipoMensaje = 0;
            int codigoCliente = FilePropertyUtils.Instance.GetValueInt(INTERFACE, Constants.NUMERO_CLIENTE);
            int count = 0;
            int countError = 0;
            Console.WriteLine("Codigo de interface: " + tipoProceso);
            Console.WriteLine("Llamando al SP por cada cliente");
            foreach (KeyValuePair<string, tblSubCliente> entry in diccionary)
            {
                Console.WriteLine("Procesando cliente: " + entry.Value.subc_codigoCliente);
                int sub_proc_id = serviceCliente.CallProcedure(tipoProceso, tipoMensaje);
                entry.Value.subc_proc_id = sub_proc_id;
                entry.Value.subc_codigo = "CODIGO";
                entry.Value.subc_areaMuelle = "AREA";
                entry.Value.subc_telefono = "TEL";

                try
                {
                    serviceCliente.Save(entry.Value);
                }
                catch (DbEntityValidationException ex)
                {
                    Console.Error.WriteLine("Error al agregar cliente: " + entry.Value.subc_codigoCliente);
                    foreach (var errors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in errors.ValidationErrors)
                        {
                            string errorMessage = validationError.ErrorMessage;
                            Console.Error.WriteLine(errorMessage);
                        }
                    }
                    countError++;
                } catch (Exception ex) {
                    Console.Error.WriteLine("Error desconocido al agregar cliente: " + entry.Value.subc_codigoCliente);
                    Console.Error.WriteLine(ex.Message);
                    countError++;
                }
                count++;
            }

            Console.WriteLine("Finalizó el proceso de actualización de clientes");
            Console.WriteLine(countError + " Clientes no pudieron ser procesados");

            /* Agregamos datos faltantes de la tabla de procesos */
            Console.WriteLine("Preparamos la actualizamos de BIANCHI_PROCESS");
            process.fin = DateTime.Now;
            process.fecha_ultima = lastTime;
            process.cant_lineas = count;
            process.estado = Constants.ESTADO_OK;
            Console.WriteLine("Fecha_fin: " + process.fin);
            Console.WriteLine("Cantidad de clientes procesados: " + process.cant_lineas);
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
