using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTranEsDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTranEsDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTranEsDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Traduce el estado interno de la transacción a una descripción legible.
        /// </summary>
        /// <param name="estado">Estado interno.</param>
        /// <returns>Descripción del estado.</returns>
        private static string ObtenerDescripcionEstado(string? estado)
        {
            return estado switch
            {
                "S" => "Solicitada",
                "A" => "Autorizada",
                "P" => "Procesada",
                "R" => "Rechazada",
                _ => estado ?? string.Empty
            };
        }

        /// <summary>
        /// Normaliza la descripción del estado de la transacción.
        /// </summary>
        /// <param name="data">Transacción a normalizar.</param>
        private static void NormalizarEstadoTransaccion(TranESData? data)
        {
            if (data is null)
            {
                return;
            }

            data.Estado = ObtenerDescripcionEstado(data.Estado);
        }

        /// <summary>
        /// Agrega filtros al listado de plantillas.
        /// </summary>
        /// <param name="tipoTran">Tipo de transacción.</param>
        /// <param name="codBoleta">Código de boleta.</param>
        /// <param name="generaUser">Usuario generador.</param>
        /// <param name="generaFecha">Fecha de generación.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltrosPlantilla(string tipoTran, string? codBoleta, string? generaUser, string? generaFecha, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            queryBuilder.Append(" WHERE plantilla = 1 AND tipo = @TipoTran ");
            parametros.Add("TipoTran", tipoTran);

            if (!string.IsNullOrWhiteSpace(codBoleta))
            {
                queryBuilder.Append(" AND boleta = @CodBoleta ");
                parametros.Add("CodBoleta", codBoleta);
            }

            if (!string.IsNullOrWhiteSpace(generaUser))
            {
                queryBuilder.Append(" AND genera_user LIKE @GeneraUser ");
                parametros.Add("GeneraUser", $"%{generaUser.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(generaFecha))
            {
                queryBuilder.Append(" AND genera_fecha BETWEEN @FechaInicio AND @FechaFin ");
                parametros.Add("FechaInicio", $"{generaFecha.Trim()} 00:00:00");
                parametros.Add("FechaFin", $"{generaFecha.Trim()} 23:59:59");
            }
        }

        /// <summary>
        /// Obtiene la consulta del detalle según el tipo de transacción.
        /// </summary>
        /// <param name="tipoTran">Tipo de transacción.</param>
        /// <returns>Consulta SQL.</returns>
        private static string ObtenerQueryLineas(string tipoTran)
        {
            string traslado = tipoTran == "T" ? ",D.COD_BODEGA_DESTINO" : string.Empty;

            return $@"select D.linea,
                              D.cod_producto,
                              P.descripcion,
                              D.cantidad,
                              B.cod_bodega,
                              B.descripcion as Bodega
                              {traslado},
                              D.precio,
                              (D.cantidad * D.precio) as Total,
                              isnull(D.despacho,0) as Despacho 
                       from PV_INVTRADET D
                       inner join pv_productos P on D.cod_producto = P.cod_producto
                       inner join PV_Bodegas B on D.cod_bodega = B.cod_bodega
                       where D.boleta = @CodBoleta and D.tipo = @TipoTran";
        }

        /// <summary>
        /// Obtiene la consulta para navegar entre transacciones.
        /// </summary>
        /// <param name="scrollValue">Dirección del desplazamiento.</param>
        /// <returns>Consulta SQL.</returns>
        private static string ObtenerQueryScroll(int scrollValue)
        {
            if (scrollValue == 1)
            {
                return "select Top 1 Boleta from pv_invTransac where tipo = @TipoTran and boleta > @CodBoleta order by boleta asc";
            }

            return "select Top 1 Boleta from pv_invTransac where tipo = @TipoTran and boleta < @CodBoleta order by boleta desc";
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo de boleta para el tipo indicado.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipoTran">Tipo de transacción.</param>
        /// <returns>Consecutivo formateado.</returns>
        private static string ObtenerSiguienteBoleta(IDbConnection connection, string tipoTran)
        {
            var consecutivo = connection.QueryFirstOrDefault<string>(
                "select isnull(max(Boleta),0)+1 as Ultimo from pv_InvTranSac where Tipo = @TipoTran",
                new { TipoTran = tipoTran });

            return (consecutivo ?? "0").PadLeft(10, '0');
        }

        /// <summary>
        /// Elimina todas las líneas de una boleta.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codBoleta">Código de boleta.</param>
        /// <param name="tipoTran">Tipo de transacción.</param>
        private static void EliminarLineasBoleta(IDbConnection connection, string codBoleta, string tipoTran)
        {
            connection.Execute(
                "delete pv_InvTraDet where boleta = @CodBoleta and tipo = @TipoTran",
                new { CodBoleta = codBoleta, TipoTran = tipoTran });
        }

        /// <summary>
        /// Inserta una línea de detalle de la transacción.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codBoleta">Código de boleta.</param>
        /// <param name="tipoTran">Tipo de transacción.</param>
        /// <param name="linea">Número de línea.</param>
        /// <param name="item">Detalle a insertar.</param>
        private static void InsertarLineaDetalle(IDbConnection connection, string codBoleta, string tipoTran, int linea, InvProducLineasInsert item)
        {
            string query = tipoTran == "T"
                ? @"insert pv_InvTraDet(linea,Boleta,tipo,cod_producto,cod_bodega,cantidad,despacho,precio,cod_bodega_destino)
                    values(@Linea, @CodBoleta, @TipoTran, @Cod_Producto, @Cod_Bodega, @Cantidad, @Despacho, @Precio, @Cod_Bodega_Destino)"
                : @"insert pv_InvTraDet(linea,Boleta,tipo,cod_producto,cod_bodega,cantidad,despacho,precio)
                    values(@Linea, @CodBoleta, @TipoTran, @Cod_Producto, @Cod_Bodega, @Cantidad, @Despacho, @Precio)";

            connection.Execute(
                query,
                new
                {
                    Linea = linea,
                    CodBoleta = codBoleta,
                    TipoTran = tipoTran,
                    item.Cod_Producto,
                    item.Cod_Bodega,
                    item.Cantidad,
                    item.Despacho,
                    item.Precio,
                    item.Cod_Bodega_Destino
                });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene los datos de una transacción de entrada/salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Datos de la transacción.</returns>
        public ErrorDto<TranESData> InvTranES_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            var result = DbHelper.ExecuteSingleQuery<TranESData>(
                CreatePortalDb(),
                CodEmpresa,
                @"select X.*, rtrim(C.descripcion) as Causa
                  from PV_INVTRANSAC X
                  inner join pv_entrada_salida C on X.cod_entsal = C.cod_entsal
                  where X.boleta = @CodBoleta and X.tipo = @TipoTran",
                new TranESData { Total = 0 },
                new { CodBoleta, TipoTran });

            if (result.Result is not null)
            {
                NormalizarEstadoTransaccion(result.Result);
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new TranESData { Total = 0 })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener la transacción.", result.Code.GetValueOrDefault(-1), new TranESData { Total = 0 });
        }

        /// <summary>
        /// Obtiene las líneas de productos asociadas a una transacción de entrada/salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Listado de líneas de productos.</returns>
        public ErrorDto<List<InvProducLineas>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            var result = DbHelper.ExecuteListQuery<InvProducLineas>(
                CreatePortalDb(),
                CodEmpresa,
                ObtenerQueryLineas(TipoTran),
                new { CodBoleta, TipoTran });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<InvProducLineas>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener líneas de la transacción.", result.Code.GetValueOrDefault(-1), new List<InvProducLineas>());
        }

        /// <summary>
        /// Desplaza la transacción de entrada/salida hacia adelante o hacia atrás en función del valor de desplazamiento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="scrollValue">Valor de desplazamiento.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Transacción encontrada por desplazamiento.</returns>
        public ErrorDto<TranESData> InvTranES_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            var result = DbHelper.ExecuteSingleQuery<TranESData>(
                CreatePortalDb(),
                CodEmpresa,
                ObtenerQueryScroll(scrollValue),
                new TranESData { Total = 0 },
                new
                {
                    TipoTran,
                    CodBoleta = CodBoleta ?? string.Empty
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new TranESData { Total = 0 })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al desplazar la transacción.", result.Code.GetValueOrDefault(-1), new TranESData { Total = 0 });
        }

        /// <summary>
        /// Obtiene las plantillas de transacciones de entrada/salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="GeneraUser">Usuario generador.</param>
        /// <param name="GeneraFecha">Fecha de generación.</param>
        /// <returns>Listado de plantillas.</returns>
        public ErrorDto<List<InvTranPlantilla>> InvTranPlantilla_Obtener(int CodEmpresa, string TipoTran, string? CodBoleta, string? GeneraUser, string? GeneraFecha)
        {
            var parametros = new DynamicParameters();
            var queryBuilder = new StringBuilder("select boleta, genera_user, genera_fecha, documento, notas from pv_InvTransac");
            AgregarFiltrosPlantilla(TipoTran, CodBoleta, GeneraUser, GeneraFecha, queryBuilder, parametros);

            var result = DbHelper.ExecuteListQuery<InvTranPlantilla>(
                CreatePortalDb(),
                CodEmpresa,
                queryBuilder.ToString(),
                parametros);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<InvTranPlantilla>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener plantillas de transacción.", result.Code.GetValueOrDefault(-1), new List<InvTranPlantilla>());
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Inserta una nueva transacción de entrada/salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="request">Datos de la transacción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTranES_Insertar(int CodEmpresa, string TipoTran, TranESData request)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                string ultimaBoleta = ObtenerSiguienteBoleta(connection, TipoTran);

                connection.Execute(
                    @"insert pv_InvTranSac(Boleta,Tipo,cod_entsal,genera_fecha,documento,notas,genera_user,estado,plantilla,fecha,fecha_sistema,total)
                      values(@Boleta,@Tipo,@Cod_Entsal,getdate(),@Documento,@Notas,@Genera_User,'S',@Plantilla,getdate(),getdate(),@Total)",
                    new
                    {
                        Boleta = ultimaBoleta,
                        Tipo = TipoTran,
                        request.Cod_Entsal,
                        request.Documento,
                        request.Notas,
                        request.Genera_User,
                        request.Total,
                        request.Plantilla
                    });

                return new ErrorDto
                {
                    Code = 0,
                    Description = ultimaBoleta
                };
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar la transacción.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza una transacción de entrada/salida existente para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la transacción a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTranES_Actualizar(int CodEmpresa, TranESUpdate request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"Update pv_InvTranSac
                  SET cod_Entsal = @Cod_Entsal,
                      fecha = getdate(),
                      documento = @Documento,
                      notas = @Notas,
                      Total = @Total,
                      plantilla = @Plantilla
                  WHERE Boleta = @Boleta and Tipo = @Tipo",
                new
                {
                    request.Boleta,
                    request.Tipo,
                    request.Cod_Entsal,
                    request.Documento,
                    request.Notas,
                    request.Total,
                    request.Plantilla
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar la transacción.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una transacción de entrada/salida existente para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTranES_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                EliminarLineasBoleta(connection, CodBoleta, TipoTran);
                connection.Execute(
                    "delete pv_InvTranSac where tipo = @TipoTran and Boleta = @CodBoleta",
                    new { TipoTran, CodBoleta });
                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar la transacción.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta las líneas de productos asociadas a una transacción de entrada/salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="producLineas">Líneas de productos.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvProducLineas_Insertar(int CodEmpresa, string CodBoleta, string TipoTran, List<InvProducLineasInsert> producLineas)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                EliminarLineasBoleta(connection, CodBoleta, TipoTran);

                int contador = 0;
                foreach (InvProducLineasInsert item in producLineas)
                {
                    contador++;
                    InsertarLineaDetalle(connection, CodBoleta, TipoTran, contador, item);
                }

                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Información guardada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar las líneas de la transacción.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una línea de producto asociada a una transacción de entrada/salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="Linea">Línea a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvProducLineas_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran, int Linea)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "delete pv_InvTraDet where tipo = @TipoTran and Boleta = @CodBoleta and linea = @Linea",
                new { TipoTran, CodBoleta, Linea });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar la línea de la transacción.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}