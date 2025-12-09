using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosAdicionRetiroDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;

        public FrmActivosAdicionRetiroDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mActivos        = new MActivosFijos(config);
            _portalDB        = new PortalDB(config);
        }

        /// <summary>
        /// Helpers genéricos para reducir duplicación
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialResult"></param>
        /// <returns></returns>
        private static ErrorDto<T> CreateOkResponse<T>(T initialResult)
        {
            return new ErrorDto<T>
            {
                Code        = 0,
                Description = "Ok",
                Result      = initialResult
            };
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que no retornan datos
        /// </summary>
        /// <returns></returns>
        private static ErrorDto CreateOkResponse()
        {
            return new ErrorDto
            {
                Code        = 0,
                Description = "Ok"
            };
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que retornan listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto<List<T>> ExecuteListQuery<T>(
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse(new List<T>());

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que retornan un solo registro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="defaultValue"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto<T> ExecuteSingleQuery<T>(
            int codEmpresa,
            string sql,
            T defaultValue,
            object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).FirstOrDefault()!;
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = defaultValue;
            }

            return result;
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que no retornan datos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto ExecuteNonQuery(
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                connection.Execute(sql, parameters);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }



        /// <summary>
        /// Método para consultar lista de justificaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Justificaciones_Obtener(int CodEmpresa, string tipo)
        {
            const string sql = @"
                SELECT RTRIM(cod_justificacion) AS item,
                       RTRIM(descripcion)       AS descripcion
                FROM   Activos_justificaciones
                WHERE  tipo = @tipo";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql, new { tipo });
        }


        /// <summary>
        /// Método para obtener los proveedores de un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Proveedores_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT cod_proveedor AS item,
                       descripcion   AS descripcion
                FROM   Activos_proveedores";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql);
        }



        /// <summary>
        /// Método para consultar listado de activos disponibles para Adición o Retiro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosData>> Activos_AdicionRetiro_Activos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT num_placa,
                       Placa_Alterna,
                       Nombre
                FROM   Activos_Principal";

            return ExecuteListQuery<ActivosData>(CodEmpresa, sql);
        }


        /// <summary>
        /// Método para consultar retiros/adiciones por número de placa e id
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Id_AddRet"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosRetiroAdicionData> Activos_AdicionRetiro_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            var result = CreateOkResponse(new ActivosRetiroAdicionData());

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT X.*,
                           RTRIM(J.cod_justificacion) AS Motivo_Id,
                           RTRIM(J.descripcion)       AS Motivo_Desc,
                           A.nombre,
                           P.cod_proveedor,
                           P.descripcion              AS Proveedor
                    FROM   Activos_retiro_adicion  X
                           INNER JOIN Activos_Principal      A ON X.num_placa = A.num_placa
                           INNER JOIN Activos_justificaciones J ON X.cod_justificacion = J.cod_justificacion
                           LEFT JOIN  Activos_proveedores     P ON X.compra_proveedor = P.cod_proveedor
                    WHERE  X.Id_AddRet  = @Id_AddRet
                    AND    X.num_placa  = @placa";

                result.Result = connection.Query<ActivosRetiroAdicionData>(sql, new { Id_AddRet, placa })
                                          .FirstOrDefault()
                                ?? new ActivosRetiroAdicionData();

                if (result.Result.tipo_vidautil != "R")
                {
                    result.Result.tipo_vidautil = "S";
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

        
        /// <summary>
        /// Método para validar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_AdicionRetiro_Validar(int CodEmpresa, string placa, DateTime fecha)
        {
            var result = new ErrorDto<string>
            {
                Code        = 0,
                Description = string.Empty,
                Result      = string.Empty
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlFecha = @"
                    SELECT fecha_adquisicion
                    FROM   Activos_Principal
                    WHERE  num_placa = @placa
                    AND    estado   <> 'R'";

                result.Result = connection.Query<string>(sqlFecha, new { placa }).FirstOrDefault();

                if (result.Result == null)
                {
                    result.Code        = -2;
                    result.Description = "El Activo no existe, o ya fue retirado ...";
                }
                else
                {
                    var fechaAdquisicion = DateTime.Parse(result.Result, System.Globalization.CultureInfo.InvariantCulture);

                    if ((fecha - fechaAdquisicion).Days < 1)
                    {
                        result.Code        = -2;
                        result.Description = "La fecha del Movimiento no es válida, ya que es menor a la del activo ...";
                    }
                }

                const string sqlPeriodo = @"
                    SELECT estado,
                           dbo.fxActivos_PeriodoActual() AS PeriodoActual
                    FROM   Activos_periodos
                    WHERE  anio = @anno
                    AND    mes  = @mes";

                var periodo = connection.Query<ActivosPeriodosData>(
                    sqlPeriodo,
                    new { anno = fecha.Year, mes = fecha.Month })
                    .FirstOrDefault();

                if (periodo != null)
                {
                    if (periodo.estado.Trim() != "P")
                    {
                        result.Code        = -2;
                        result.Description = $"{result.Description} - El Periodo del Movimiento ya fue cerrado ... ";
                    }

                    if (fecha.Year != periodo.periodoactual.Year ||
                        fecha.Month != periodo.periodoactual.Month)
                    {
                        result.Code        = -2;
                        result.Description = $"{result.Description} - La fecha de aplicación del movimiento no corresponde al periodo abierto!";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

        
        /// <summary>
        /// Método para consultar los meses de un registro de Adición/Retiro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="tipo"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<int> Activos_AdicionRetiro_Meses_Consulta(int CodEmpresa, string placa, string tipo, DateTime fecha)
        {
            const string sql = @"SELECT dbo.fxActivos_VidaUtilPendiente(@placa, @tipo, @fecha)";

            return ExecuteSingleQuery(
                CodEmpresa,
                sql,
                0,
                new { placa, tipo, fecha });
        }


        /// <summary>
        /// Método para consultar los datos de Depreciación del Activo al corte
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPrincipalData> Activos_AdicionRetiro_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            var result = CreateOkResponse(new ActivosPrincipalData());

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"EXEC spActivos_InfoDepreciacion @placa";

                var data = connection.Query<ActivosPrincipalData>(sql, new { placa }).FirstOrDefault();

                if (data == null)
                {
                    data = new ActivosPrincipalData
                    {
                        depreciacionPeriodo = "????",
                        depreciacion_acum   = 0,
                        valor_historico     = 0,
                        valor_desecho       = 0,
                        valor_libros        = 0
                    };
                }
                else
                {
                    data.depreciacionPeriodo = data.depreciacion_periodo.ToString("dd/MM/yyyy");
                }

                result.Result = data;
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

       
       /// <summary>
       /// Método para guardar un registro de Adiciones, Mejoras o Retiros del Activo
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="usuario"></param>
       /// <param name="data"></param>
       /// <returns></returns>
        public ErrorDto Activos_AdicionRetiro_Guardar(int CodEmpresa, string usuario, ActivosRetiroAdicionData data)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    EXEC spActivos_AdicionRetiro
                         @Placa,
                         @Tipo,
                         @Justificacion,
                         @Descripcion,
                         @Fecha,
                         @Monto,
                         @Meses,
                         @Usuario,
                         @CompraDoc,
                         @CompraProv,
                         @VentaDoc,
                         @VentaCliente";

                var linea = connection.Query<int>(sql, new
                {
                    Placa        = data.num_placa,
                    Tipo         = data.tipo,
                    Justificacion= data.cod_justificacion,
                    Descripcion  = data.descripcion,
                    Fecha        = data.fecha,
                    Monto        = data.monto,
                    Meses        = data.meses_calculo,
                    Usuario      = usuario,
                    CompraDoc    = data.compra_documento,
                    CompraProv   = data.proveedor,
                    VentaDoc     = data.venta_documento,
                    VentaCliente = data.venta_cliente
                }).FirstOrDefault();

                result.Code = linea;

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento =
                        $"{data.tipoDescripcion} (Placa: {data.num_placa}) Id: {data.tipoDescripcion}:{data.id_addret}_ {data.justificacion} ",
                    Movimiento        = "Registra - WEB",
                    Modulo            = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        
        /// <summary>
        /// Método para consultar el histórico de retiros y adiciones de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosHistoricoData>> Activos_AdicionRetiro_Historico_Consultar(int CodEmpresa, string placa)
        {
            const string sql = @"
                SELECT X.id_AddRet,
                       X.fecha,
                       X.MONTO,
                       X.DESCRIPCION,
                       RTRIM(J.cod_justificacion) + ' - ' + J.descripcion AS Justifica,
                       CASE 
                           WHEN X.Tipo = 'A' THEN 'Adicion/Mejora'
                           WHEN X.Tipo = 'M' THEN 'Mantenimiento'
                           WHEN X.Tipo = 'R' THEN 'Retiro'
                           ELSE ''
                       END AS TipoMov
                FROM   Activos_retiro_adicion X
                       INNER JOIN Activos_Principal      A ON X.num_placa = A.num_placa
                       INNER JOIN Activos_justificaciones J ON X.cod_justificacion = J.cod_justificacion
                       LEFT JOIN  Activos_proveedores     P ON X.compra_proveedor = P.cod_proveedor
                WHERE  X.num_placa = @placa
                AND    X.Tipo IN ('A','R','M')";

            return ExecuteListQuery<ActivosHistoricoData>(CodEmpresa, sql, new { placa });
        }


        /// <summary>
        /// Método para consultar los cierres de un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="Id_AddRet"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosRetiroAdicionCierreData>> Activos_AdicionRetiro_Cierres_Consultar(int CodEmpresa, string placa, int Id_AddRet)
        {
            const string sql = @"
                SELECT A.*
                FROM   Activos_Auxiliar_Adiciones A
                       INNER JOIN Activos_Periodos P
                           ON A.anio = P.Anio AND A.mes = P.mes
                WHERE  A.num_placa  = @placa
                AND    A.ID_AddRet  = @Id_AddRet
                AND    P.Estado     = 'C'
                ORDER BY A.anio DESC, A.mes DESC";

            return ExecuteListQuery<ActivosRetiroAdicionCierreData>(CodEmpresa, sql, new { placa, Id_AddRet });
        }


        /// <summary>
        /// Método para obtener el nombre del activo por número de placa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_AdicionRetiro_ActivosNombre_Consultar(int CodEmpresa, string placa)
        {
            const string sql = @"
                SELECT nombre
                FROM   Activos_Principal
                WHERE  num_placa = @placa";

            return ExecuteSingleQuery(CodEmpresa, sql, string.Empty, new { placa });
        }


        /// <summary>
        /// Método para eliminar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="Id_AddRet"></param>
        /// <returns></returns>
        public ErrorDto Activos_AdicionRetiro_Eliminar(int CodEmpresa, string placa, int Id_AddRet)
        {
            const string sql = @"
                DELETE Activos_retiro_adicion
                WHERE  num_placa = @placa
                AND    Id_AddRet = @Id_AddRet";

            return ExecuteNonQuery(CodEmpresa, sql, new { placa, Id_AddRet });
        }


        /// <summary>
        /// Consulta el periodo pendiente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            var result = CreateOkResponse(DateTime.Now);

            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}