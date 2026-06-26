using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvTranReversionDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTranReversionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTranReversionDB(IConfiguration config)
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
        /// Normaliza el estado de la transacción.
        /// </summary>
        /// <param name="data">Transacción a normalizar.</param>
        private static void NormalizarEstadoTransaccion(TranReversionData? data)
        {
            if (data is null)
            {
                return;
            }

            data.Estado = ObtenerDescripcionEstado(data.Estado);
        }

        /// <summary>
        /// Query de líneas de reversión según el tipo de transacción.
        /// </summary>
        private const string QueryLineas = @"select D.Linea,
                D.cod_producto,
                P.descripcion,
                D.cantidad,
                B.cod_bodega,
                B.descripcion as Bodega,
                D.precio,
                (D.cantidad * D.precio) as Total,
                isnull(D.despacho,0) as Despacho,
                D.cod_bodega_destino,
                X.descripcion as BodegaD
            from PV_INVTRADET D
            inner join pv_productos P on D.cod_producto = P.cod_producto
            inner join PV_Bodegas B on D.cod_bodega = B.cod_bodega
            left join PV_Bodegas X on D.cod_bodega_destino = X.cod_Bodega
            where D.boleta = @CodBoleta and D.tipo = @TipoTran";

        /// <summary>
        /// Obtiene la consulta para navegar entre transacciones.
        /// </summary>
        /// <param name="scrollValue">Dirección del desplazamiento.</param>
        /// <returns>Consulta SQL.</returns>
        private static string ObtenerQueryScroll(int scrollValue)
        {
            return scrollValue == 1
                ? "select Top 1 Boleta from pv_invTransac where tipo = @TipoTran and boleta > @CodBoleta order by boleta asc"
                : "select Top 1 Boleta from pv_invTransac where tipo = @TipoTran and boleta < @CodBoleta order by boleta desc";
        }

        /// <summary>
        /// Obtiene el tipo inverso de la transacción y su descripción destino.
        /// </summary>
        /// <param name="tipo">Tipo original.</param>
        /// <returns>Tupla con tipo inverso y descripción destino.</returns>
        private static (string TipoInverso, string Destino) ObtenerDatosReversion(string tipo)
        {
            return tipo switch
            {
                "E" => ("S", "Salida"),
                "S" => ("E", "Entrada"),
                "T" => ("T", "Traslado"),
                "R" => ("R", "Requisicion"),
                _ => (string.Empty, string.Empty)
            };
        }

        /// <summary>
        /// Obtiene el estado actual de una boleta.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipo">Tipo de transacción.</param>
        /// <param name="boleta">Boleta a validar.</param>
        /// <returns>Estado encontrado o cadena vacía.</returns>
        private static string ObtenerEstadoBoleta(IDbConnection connection, string tipo, string boleta)
        {
            return connection.ExecuteScalar<string>(
                "select estado from pv_InvTranSac where Tipo = @Tipo and Boleta = @Boleta",
                new { Tipo = tipo, Boleta = boleta }) ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo para el tipo inverso.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <returns>Boleta generada con padding.</returns>
        private static string ObtenerSiguienteBoleta(IDbConnection connection, string tipoInverso)
        {
            var consecutivo = connection.QueryFirstOrDefault<string>(
                "select isnull(max(Boleta),0)+1 as Ultimo from pv_InvTranSac where Tipo = @Tipo",
                new { Tipo = tipoInverso });

            return (consecutivo ?? "1").PadLeft(10, '0');
        }

        /// <summary>
        /// Inserta el encabezado de la transacción reversada.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="boletaInverso">Nueva boleta.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <param name="request">Datos de la reversión.</param>
        private static void InsertarEncabezadoReversion(IDbConnection connection, string boletaInverso, string tipoInverso, TranReversionInsert request)
        {
            connection.Execute(
                @"insert pv_InvTranSac(Boleta,Tipo,cod_entsal,genera_fecha,documento,notas,genera_user,
                                      estado,plantilla,fecha,fecha_sistema,autoriza_fecha,autoriza_user,procesa_fecha,procesa_user)
                  values(@Boleta,@Tipo,@Cod_Entsal,getdate(),@Documento,@Notas,@User,
                         'P',0,@Fecha,getdate(),getdate(),@User,getdate(),@User)",
                new
                {
                    Boleta = boletaInverso,
                    Tipo = tipoInverso,
                    request.Cod_Entsal,
                    Documento = $"Rev.{request.Boleta}",
                    request.Notas,
                    request.User,
                    request.Fecha
                });
        }

        /// <summary>
        /// Inserta el detalle reversado para entradas y salidas.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="boletaInverso">Nueva boleta.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <param name="request">Datos de la reversión.</param>
        private static void InsertarDetalleEntradaSalida(IDbConnection connection, string boletaInverso, string tipoInverso, TranReversionInsert request)
        {
            connection.Execute(
                @"insert into pv_invTraDet(Linea,Boleta,Tipo,Cod_Bodega,cod_Producto,Cod_Bodega_destino,cantidad,Precio,despacho)
                  select Linea,
                         @BoletaInverso,
                         @TipoInverso,
                         Cod_Bodega,
                         cod_Producto,
                         Cod_Bodega_destino,
                         cantidad,
                         Precio,
                         cantidad as Desp
                  From pv_invTraDet
                  Where Tipo = @TipoOriginal And Boleta = @BoletaOriginal",
                new
                {
                    BoletaInverso = boletaInverso,
                    TipoInverso = tipoInverso,
                    TipoOriginal = request.Tipo,
                    BoletaOriginal = request.Boleta
                });
        }

        /// <summary>
        /// Inserta el detalle reversado para traslados invirtiendo bodegas.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="boletaInverso">Nueva boleta.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <param name="request">Datos de la reversión.</param>
        private static void InsertarDetalleTraslado(IDbConnection connection, string boletaInverso, string tipoInverso, TranReversionInsert request)
        {
            connection.Execute(
                @"insert into pv_invTraDet(Linea,Boleta,Tipo,Cod_Bodega,cod_Producto,Cod_Bodega_destino,cantidad,Precio,despacho)
                  select Linea,
                         @BoletaInverso,
                         @TipoInverso,
                         Cod_Bodega_destino,
                         cod_Producto,
                         Cod_Bodega,
                         cantidad,
                         Precio,
                         cantidad as Desp
                  From pv_invTraDet
                  Where Tipo = @TipoOriginal And Boleta = @BoletaOriginal",
                new
                {
                    BoletaInverso = boletaInverso,
                    TipoInverso = tipoInverso,
                    TipoOriginal = request.Tipo,
                    BoletaOriginal = request.Boleta
                });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene una transacción para reversión.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Datos de la transacción.</returns>
        public ErrorDto<TranReversionData> InvTranReversion_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            var result = DbHelper.ExecuteSingleQuery<TranReversionData>(
                CreatePortalDb(),
                CodEmpresa,
                @"select X.*,(rtrim(C.cod_entsal) + ' - ' + C.descripcion) as Causa
                  from PV_INVTRANSAC X
                  inner join pv_entrada_salida C on X.cod_entsal = C.cod_entsal
                  where X.boleta = @CodBoleta and X.tipo = @TipoTran",
                new TranReversionData(),
                new { CodBoleta, TipoTran });

            if (result.Result is not null)
            {
                NormalizarEstadoTransaccion(result.Result);
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new TranReversionData())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener la transacción de reversión.", result.Code.GetValueOrDefault(-1), new TranReversionData());
        }

        /// <summary>
        /// Obtiene las líneas de productos asociadas a una reversión.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Listado de líneas.</returns>
        public ErrorDto<List<InvProducReversion>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            var result = DbHelper.ExecuteListQuery<InvProducReversion>(
                CreatePortalDb(),
                CodEmpresa,
                QueryLineas,
                new { CodBoleta, TipoTran });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<InvProducReversion>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener líneas de reversión.", result.Code.GetValueOrDefault(-1), new List<InvProducReversion>());
        }

        /// <summary>
        /// Desplaza la consulta de reversión hacia adelante o atrás.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="scrollValue">Dirección del desplazamiento.</param>
        /// <param name="CodBoleta">Boleta actual.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Boleta encontrada.</returns>
        public ErrorDto<TranReversionData> InvTranReversion_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            var result = DbHelper.ExecuteSingleQuery<TranReversionData>(
                CreatePortalDb(),
                CodEmpresa,
                ObtenerQueryScroll(scrollValue),
                new TranReversionData(),
                new
                {
                    TipoTran,
                    CodBoleta = CodBoleta ?? string.Empty
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new TranReversionData())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al desplazar la reversión.", result.Code.GetValueOrDefault(-1), new TranReversionData());
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Inserta una reversión de transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la reversión.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTranReversion_Insertar(int CodEmpresa, TranReversionInsert request)
        {
            var datosReversion = ObtenerDatosReversion(request.Tipo);
            if (string.IsNullOrWhiteSpace(datosReversion.TipoInverso))
            {
                return DbHelper.ErrorResponse("Tipo de transacción no válido para reversión.", -1);
            }

            string vFecha = request.Fecha.ToString();
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                string verificaEstado = ObtenerEstadoBoleta(connection, request.Tipo, request.Boleta);

                if (string.IsNullOrEmpty(verificaEstado))
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = $"No se encontró la boleta '{request.Boleta}', verifique..."
                    };
                }

                if (verificaEstado != "P")
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "La Boleta consultada no se encuentra procesada..."
                    };
                }

                var periodo = new MProGrXAuxiliarDB(_config).fxInvPeriodos(CodEmpresa, vFecha);
                if (!periodo)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "El periodo en el que desea realizar el movimiento se encuentra cerrado ..."
                    };
                }

                string boletaInverso = ObtenerSiguienteBoleta(connection, datosReversion.TipoInverso);
                InsertarEncabezadoReversion(connection, boletaInverso, datosReversion.TipoInverso, request);

                if (request.Tipo == "E" || request.Tipo == "S")
                {
                    InsertarDetalleEntradaSalida(connection, boletaInverso, datosReversion.TipoInverso, request);
                }
                else if (request.Tipo == "T")
                {
                    InsertarDetalleTraslado(connection, boletaInverso, datosReversion.TipoInverso, request);
                }

                return new ErrorDto
                {
                    Code = 0,
                    Description = $"Reversion de '{request.Tipo}', Boleta : '{request.Boleta}' realizada con '{datosReversion.Destino}' boleta : '{boletaInverso}'"
                };
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar la reversión.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}