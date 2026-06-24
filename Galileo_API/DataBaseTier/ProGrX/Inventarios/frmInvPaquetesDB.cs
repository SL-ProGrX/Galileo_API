using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvPaquetesDB
    {
        private readonly IConfiguration _config;
        private const string DateFormatYMD = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm:ss";
        private const string MensajeOk = "Ok";
        private const string ErrorObtenerPaquetes = "Error al obtener paquetes.";
        private const string ErrorObtenerPaquete = "Error al obtener el paquete.";
        private const string ErrorObtenerDetalle = "Error al obtener el detalle del paquete.";
        private const string ErrorActualizarPaquete = "Error al actualizar el paquete.";
        private const string ErrorInsertarPaquete = "Error al insertar el paquete.";
        private const string ErrorInsertarDetalle = "Error al insertar el detalle del paquete.";
        private const string ErrorActualizarDetalle = "Error al actualizar el detalle del paquete.";
        private const string ErrorEliminarDetalle = "Error al eliminar el detalle del paquete.";
        private const string ErrorEliminarDetalles = "Error al eliminar los detalles del paquete.";
        private const string ErrorCodigoPaquete = "Código de paquete inválido.";
        private const string QueryPaquetes = "SELECT * FROM pv_paquetes";
        private const string QueryTotalPaquetes = "SELECT COUNT(*) FROM pv_paquetes";
        private const string QueryObtenerPaquete = "SELECT * FROM pv_paquetes WHERE cod_paquete = @Cod_Paquete";
        private const string QueryEliminarDetalle = "DELETE pv_paquetes_detalle WHERE linea = @Linea";
        private const string QueryEliminarDetalles = "DELETE pv_paquetes_detalle WHERE cod_paquete = @Cod_Paquete";
        private const string QueryUnidadProducto = "SELECT COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO = @Cod_Producto";
        private const string ProcedurePaqueteAgregar = "[spINV_W_Paquete_Agregar]";
        private const string ProcedureDetalleAgregar = "[spINV_W_PaqueteDetalle_Agregar]";
        private const string ProcedureDetalleActualizar = "[spINV_W_PaqueteDetalle_Actualizar]";


        public FrmInvPaquetesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private PortalDB CreatePortalDb() => new(_config);

        private static PaqueteDataLista CrearListaVacia() => new()
        {
            Total = 0,
            Lista = new List<PaqueteDto>()
        };



        private static object CrearParametrosPaquete(int codPaquete) => new
        {
            Cod_Paquete = codPaquete,
            cod_paquete = codPaquete
        };

        private static object CrearParametrosLinea(int linea) => new
        {
            Linea = linea,
            linea
        };

        private static object CrearParametrosDetalle(PaqueteDetalleDto request) => new
        {
            request.Cod_Producto,
            request.Cod_Paquete,
            request.Cantidad,
            request.Porc_Utilidad,
            request.Precio,
            request.Imp_Ventas,
            request.Imp_Consumo
        };

        private static string ObtenerUnidadProducto(IDbConnection connection, string codProducto)
        {
            return connection.QuerySingleOrDefault<string>(
                QueryUnidadProducto,
                new { Cod_Producto = codProducto }) ?? string.Empty;
        }

        private static void CompletarUnidadDetalles(IDbConnection connection, IEnumerable<PaqueteDetalleDto> detalles)
        {
            foreach (PaqueteDetalleDto item in detalles)
            {
                item.unidad = ObtenerUnidadProducto(connection, item.Cod_Producto);
            }
        }

        private static DynamicParameters CrearParametrosInsertarPaquete(PaqueteDto request)
        {
            var parameters = new DynamicParameters();

            parameters.Add(nameof(PaqueteDto.Descripcion), request.Descripcion);
            parameters.Add(nameof(PaqueteDto.Notas), request.Notas);
            parameters.Add(nameof(PaqueteDto.User_Crea), request.User_Crea);
            parameters.Add(nameof(PaqueteDto.Fecha_Inicio), MProGrXAuxiliarDB.validaFechaGlobal(request.Fecha_Inicio, DateFormatYMD));
            parameters.Add(nameof(PaqueteDto.Fecha_Corte), MProGrXAuxiliarDB.validaFechaGlobal(request.Fecha_Corte, DateFormatYMD));
            parameters.Add(nameof(PaqueteDto.Frecuencia_Horai), MProGrXAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horai, TimeFormat));
            parameters.Add(nameof(PaqueteDto.Frecuencia_Horac), MProGrXAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horac, TimeFormat));
            parameters.Add(nameof(PaqueteDto.Frecuencia_Lunes), request.Frecuencia_Lunes);
            parameters.Add(nameof(PaqueteDto.Frecuencia_Martes), request.Frecuencia_Martes);
            parameters.Add(nameof(PaqueteDto.Frecuencia_Miercoles), request.Frecuencia_Miercoles);
            parameters.Add(nameof(PaqueteDto.Frecuencia_Jueves), request.Frecuencia_Jueves);
            parameters.Add(nameof(PaqueteDto.Frecuencia_Viernes), request.Frecuencia_Viernes);
            parameters.Add(nameof(PaqueteDto.Frecuencia_Sabado), request.Frecuencia_Sabado);
            parameters.Add(nameof(PaqueteDto.Frecuencia_Domingo), request.Frecuencia_Domingo);
            parameters.Add("NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            return parameters;
        }


        private ErrorDto EjecutarProcedimientoConCodigo(int CodEmpresa, string procedure, object values, string errorMessage)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(procedure, values, commandType: CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(errorMessage, result.Result);
        }

        public ErrorDto<PaqueteDataLista> Paquetes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>(QueryTotalPaquetes);

                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder(QueryPaquetes);
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    queryBuilder.Append(" WHERE cod_paquete LIKE @Filtro OR DESCRIPCION LIKE @Filtro ");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                queryBuilder.Append(" ORDER BY cod_paquete ");

                if (pagina.HasValue && paginacion.HasValue)
                {
                    queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY ");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Lista = connection.Query<PaqueteDto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? ErrorObtenerPaquetes, result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        public ErrorDto<List<PaqueteDto>> Paquetes_ObtenerTodos(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<PaqueteDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryPaquetes);
        }

        public ErrorDto<PaqueteDto> Paquete_Obtener(int CodEmpresa, int Cod_Paquete)
        {
            var result = DbHelper.ExecuteSingleQuery<PaqueteDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryObtenerPaquete,
                null,
                CrearParametrosPaquete(Cod_Paquete));

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? default(PaqueteDto)!)
                : DbHelper.CreateErrorResponse(result.Description ?? ErrorObtenerPaquete, result.Code.GetValueOrDefault(-1), default(PaqueteDto)!);
        }

        public ErrorDto<List<PaqueteDetalleDto>> Paquete_ObtenerDetalles(int CodEmpresa, int Cod_Paquete)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var detalles = connection.Query<PaqueteDetalleDto>(
                    @"SELECT D.linea,
                             D.cod_producto,
                             P.descripcion,
                             D.cantidad,
                             D.porc_utilidad,
                             D.precio,
                             D.imp_ventas,
                             (D.cantidad * (D.precio + D.precio * D.porc_utilidad / 100))
                             + ((D.cantidad * (D.precio + D.precio * D.porc_utilidad / 100)) * (D.imp_ventas / 100)) AS Total
                      FROM pv_paquetes_detalle D
                      INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                      WHERE D.cod_paquete = @Cod_Paquete
                      ORDER BY D.Linea",
                    CrearParametrosPaquete(Cod_Paquete)).ToList();

                CompletarUnidadDetalles(connection, detalles);
                return detalles;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<PaqueteDetalleDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? ErrorObtenerDetalle, result.Code.GetValueOrDefault(-1), new List<PaqueteDetalleDto>());
        }

        public ErrorDto Paquete_Actualizar(int CodEmpresa, PaqueteDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE pv_paquetes
                  SET descripcion = @Descripcion,
                      notas = @Notas,
                      user_modifica = @User_Modifica,
                      fecha_modifica = @Fecha_Modifica,
                      fecha_inicio = @Fecha_Inicio,
                      fecha_corte = @Fecha_Corte,
                      frecuencia_horai = @Frecuencia_Horai,
                      frecuencia_horac = @Frecuencia_Horac,
                      frecuencia_lunes = @Frecuencia_Lunes,
                      frecuencia_martes = @Frecuencia_Martes,
                      frecuencia_miercoles = @Frecuencia_Miercoles,
                      frecuencia_jueves = @Frecuencia_Jueves,
                      frecuencia_viernes = @Frecuencia_Viernes,
                      frecuencia_sabado = @Frecuencia_Sabado,
                      frecuencia_domingo = @Frecuencia_Domingo
                  WHERE cod_paquete = @Cod_Paquete",
                new
                {
                    request.Cod_Paquete,
                    request.Descripcion,
                    request.Notas,
                    request.User_Modifica,
                    Fecha_Modifica = DateTime.Now,
                    Fecha_Inicio = request.Fecha_Inicio?.ToLocalTime(),
                    Fecha_Corte = request.Fecha_Corte?.ToLocalTime(),
                    Frecuencia_Horai = request.Frecuencia_Horai?.ToLocalTime(),
                    Frecuencia_Horac = request.Frecuencia_Horac?.ToLocalTime(),
                    request.Frecuencia_Lunes,
                    request.Frecuencia_Martes,
                    request.Frecuencia_Miercoles,
                    request.Frecuencia_Jueves,
                    request.Frecuencia_Viernes,
                    request.Frecuencia_Sabado,
                    request.Frecuencia_Domingo
                });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(result.Description ?? ErrorActualizarPaquete, result.Code.GetValueOrDefault(-1));
        }

        public ErrorDto Paquete_Insertar(int CodEmpresa, PaqueteDto request)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parameters = CrearParametrosInsertarPaquete(request);
                connection.Execute(ProcedurePaqueteAgregar, parameters, commandType: CommandType.StoredProcedure);
                return parameters.Get<int>("NewID");
            });

            return result.Code == 0
                ? new ErrorDto { Code = result.Result, Description = MensajeOk }
                : DbHelper.ErrorResponse(result.Description ?? ErrorInsertarPaquete, result.Code.GetValueOrDefault(-1));
        }

        public ErrorDto Paquete_Insertar2(int CodEmpresa, PaqueteDto request)
        {
            return Paquete_Insertar(CodEmpresa, request);
        }

        public ErrorDto PaqueteDetalle_Insertar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                ProcedureDetalleAgregar,
                CrearParametrosDetalle(request),
                ErrorInsertarDetalle);
        }

        public ErrorDto PaqueteDetalle_Actualizar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                ProcedureDetalleActualizar,
                CrearParametrosDetalle(request),
                ErrorActualizarDetalle);
        }

        public ErrorDto PaqueteDetalle_Eliminar(int CodEmpresa, PaqueteDetalleDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryEliminarDetalle,
                CrearParametrosLinea(request.Linea));

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(result.Description ?? ErrorEliminarDetalle, result.Code.GetValueOrDefault(-1));
        }

        public ErrorDto Paquete_Eliminar(int CodEmpresa, PaqueteDto request)
        {
            return Paquete_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Paquete_EliminarDetalles(int CodEmpresa, PaqueteDto request)
        {
            if (!request.Cod_Paquete.HasValue)
            {
                return DbHelper.ErrorResponse(ErrorCodigoPaquete, -1);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryEliminarDetalles,
                CrearParametrosPaquete(request.Cod_Paquete.Value));

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(result.Description ?? ErrorEliminarDetalles, result.Code.GetValueOrDefault(-1));
        }
    }
}