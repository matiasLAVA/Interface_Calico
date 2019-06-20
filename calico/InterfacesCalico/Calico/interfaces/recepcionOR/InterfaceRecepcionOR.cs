﻿using Calico.common;
using Calico.interfaces.pedido;
using Calico.interfaces.pedidos;
using Calico.persistencia;
using Calico.service;
using InterfacesCalico.generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calico.interfaces.recepcionOR
{
    class InterfaceRecepcionOR : InterfaceGeneric
    {

        private const String INTERFACE = Constants.INTERFACE_PEDIDOS;

        private BianchiService service = new BianchiService();
        private TblPedidoService servicePedido = new TblPedidoService();
        private RecepcionORUtils recepcionORUtils = new RecepcionORUtils();

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
            String numeroInterfaz = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.NUMERO_INTERFACE);
            String fromStatus = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.FROM_STATUS);
            String toStatus = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.TO_STATUS);
            String emplazamiento = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.EMPLAZAMIENTO);
            String cliente = FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.INTERFACE_CLIENTE);
            // .....

            /* Obtenemos usuario y contraseña del archivo para el servicio Rest */
            String urlPath = String.Empty;
            String user = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.USER);
            String pass = FilePropertyUtils.Instance.GetValueString(Constants.BASIC_AUTH, Constants.PASS);
            Console.WriteLine("Usuario del Servicio Rest: " + user);

            /* Obtenemos la URL del archivo */
            String urlPost = FilePropertyUtils.Instance.GetValueString(INTERFACE + "." + Constants.URLS, Constants.RECEPCION_OR_URL_POST);

            /* Obtenemos el tipo de pedido del archivo externo */
            String[] tiposPedido = {
                FilePropertyUtils.Instance.GetValueString(INTERFACE, Constants.TIPO_PEDIDO)
            };

            int countOKPedido = 0;
            int countErrorPedido = 0;
            int countAlreadyProcessPedido = 0;
            int? tipoMensaje = 0;
            int tipoProceso = FilePropertyUtils.Instance.GetValueInt(INTERFACE, Constants.TIPO_PROCESO);
            int codigoCliente = FilePropertyUtils.Instance.GetValueInt(INTERFACE, Constants.NUMERO_CLIENTE);
            Console.WriteLine("Codigo de interface: " + tipoProceso);

            /* Mapping */
            List<PedidoDTO> pedidosDTO = null;
            Dictionary<string, tblPedido> dictionary = new Dictionary<string, tblPedido>();

            /* Preparamos y enviamos la URL */
            PedidoJson json = recepcionORUtils.getJson(fromStatus, toStatus, tiposPedido);
            var jsonString = recepcionORUtils.JsonToString(json);
            Console.WriteLine("Se enviara el siguiente Json al servicio REST: ");
            Console.WriteLine(jsonString);
            Console.WriteLine("Se realiza el envio al servicio REST : " + urlPost);
            pedidosDTO = recepcionORUtils.SendRequestPost(urlPost, user, pass, jsonString);

            if (pedidosDTO.Any())
            {
                recepcionORUtils.MappingPedidoDTOPedido(pedidosDTO, dictionary, emplazamiento, cliente);
                // Validamos si hay que insertar o descartar el pedido
                foreach (KeyValuePair<string, tblPedido> entry in dictionary)
                {

                    if (servicePedido.IsAlreadyProcess(entry.Value.pedc_almacen, entry.Value.pedc_tped_codigo, entry.Value.pedc_letra, entry.Value.pedc_sucursal, entry.Value.pedc_numero))
                    {
                        Console.WriteLine("El pedido " + entry.Value.pedc_numero + " ya fue tratado, no se procesara");
                        countAlreadyProcessPedido++;
                    }
                    else // No está procesada! la voy a guardar
                    {
                        // LLamo al SP y seteo su valor a la cabecera y sus detalles
                        int recc_proc_id = servicePedido.CallProcedure(tipoProceso, tipoMensaje);
                        entry.Value.pedc_proc_id = recc_proc_id;
                        foreach (tblPedidoDetalle detalle in entry.Value.tblPedidoDetalle)
                        {
                            detalle.pedd_proc_id = recc_proc_id;
                        }

                        Console.WriteLine("Procesando pedido: " + entry.Value.pedc_numero);
                        if (servicePedido.Save(entry.Value)) countOKPedido++;
                        else countErrorPedido++;
                    }
                }
            }
            else
            {
                Console.WriteLine(Constants.FAILED_GETTING_DATA);
            }

            Console.WriteLine("Finalizó el proceso de actualización de Pedidos");

            /* Agregamos datos faltantes de la tabla de procesos */
            Console.WriteLine("Preparamos los datos a actualizar en BIANCHI_PROCESS");
            process.fin = DateTime.Now;
            process.fecha_ultima = process.inicio;
            process.cant_lineas = countOKPedido;
            process.estado = Constants.ESTADO_OK;
            Console.WriteLine("Fecha_fin: " + process.fin);
            Console.WriteLine("Cantidad de pedidos procesados OK: " + process.cant_lineas);
            Console.WriteLine("Cantidad de pedidos procesados con ERROR: " + countErrorPedido);
            Console.WriteLine("Cantidad de pedidos evitados: " + countAlreadyProcessPedido);
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
