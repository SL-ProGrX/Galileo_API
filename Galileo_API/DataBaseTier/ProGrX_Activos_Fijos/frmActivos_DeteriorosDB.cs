using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosDeteriorosDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;


        public FrmActivosDeteriorosDb(IConfiguration config)
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
        /// Ejecuta una consulta que devuelve una lista de elementos
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
        /// Ejecuta una consulta que devuelve un solo elemento
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
        /// Método para obtener las justificaciones de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Deterioros_Justificaciones_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(cod_justificacion) AS item,
                       RTRIM(descripcion)       AS descripcion
                FROM   Activos_justificaciones
                WHERE  tipo = 'D'";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql);
        }


        /// <summary>
        /// Método para obtener los activos disponibles para deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosData>> Activos_Deterioros_Activos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT num_placa,
                       Placa_Alterna,
                       Nombre
                FROM   Activos_Principal";

            return ExecuteListQuery<ActivosData>(CodEmpresa, sql);
        }


        /// <summary>
        /// Método para consultar los datos de un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Id_AddRet"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosDeterioroData> Activos_Deterioros_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            var result = CreateOkResponse(new ActivosDeterioroData());

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
                    FROM   Activos_retiro_adicion X
                           INNER JOIN Activos_Principal      A ON X.num_placa = A.num_placa
                           INNER JOIN Activos_justificaciones J ON X.cod_justificacion = J.cod_justificacion
                           LEFT JOIN  Activos_proveedores     P ON X.compra_proveedor = P.cod_proveedor
                    WHERE  X.Id_AddRet = @Id_AddRet
                    AND    X.num_placa = @placa";

                result.Result = connection.Query<ActivosDeterioroData>(sql, new { Id_AddRet, placa })
                                          .FirstOrDefault()
                                ?? new ActivosDeterioroData();
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
        /// Método para validar un activo antes de pasarlo a estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Deterioros_Validar(int CodEmpresa, string placa, DateTime fecha)
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
        /// Método para consultar los detalles de un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosDeterioroDetallaData> Activos_DeteriorosDetalle_Consultar(int CodEmpresa, string placa)
        {
            var result = CreateOkResponse(new ActivosDeterioroDetallaData());

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"EXEC spActivos_InfoDepreciacion @placa";

                var data = connection.Query<ActivosDeterioroDetallaData>(sql, new { placa }).FirstOrDefault();

                if (data == null)
                {
                    data = new ActivosDeterioroDetallaData
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
        /// Método para guardar un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Activos_Deterioros_Guardar(int CodEmpresa, string usuario, ActivosDeterioroData data)
        {
            var result = new ErrorDto
            {
                Code        = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    EXEC spActivos_AdicionRetiro
                         @Placa,
                         'D',
                         @Justificacion,
                         @Descripcion,
                         @Fecha,
                         @Monto,
                         1,
                         @Usuario,
                         '',
                         '',
                         '',
                         ''";

                var linea = connection.Query<int>(sql, new
                {
                    Placa         = data.num_placa,
                    Justificacion = data.motivo_id,
                    Descripcion   = data.descripcion,
                    Fecha         = data.fecha,
                    Monto         = Math.Abs(data.monto),
                    Usuario       = usuario
                }).FirstOrDefault();

                result.Code = linea;

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"Deterioro (Placa: {data.num_placa})  Deterioro Id:{data.id_addret}_ {data.motivo_desc} ",
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
        /// Método para consultar el histórico de deterioros de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosHistoricoData>> Activos_Deterioros_Historico_Consultar(int CodEmpresa, string placa)
        {
            const string sql = @"
                SELECT X.id_AddRet,
                       X.fecha,
                       X.MONTO,
                       X.DESCRIPCION,
                       RTRIM(J.cod_justificacion) + '..' + J.descripcion AS Justifica,
                       A.nombre,
                       P.cod_proveedor,
                       P.descripcion                                     AS Proveedor,
                       'Revaluación'                                     AS TipoMov
                FROM   Activos_retiro_adicion X
                       INNER JOIN Activos_Principal      A ON X.num_placa = A.num_placa
                       INNER JOIN Activos_justificaciones J ON X.cod_justificacion = J.cod_justificacion
                       LEFT JOIN  Activos_proveedores     P ON X.compra_proveedor = P.cod_proveedor
                WHERE  X.num_placa = @placa
                AND    X.Tipo      = 'D'";

            return ExecuteListQuery<ActivosHistoricoData>(CodEmpresa, sql, new { placa });
        }


        /// <summary>
        /// Método para consultar el nombre de un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Deterioros_ActivosNombre_Consultar(int CodEmpresa, string placa)
        {
            const string sql = @"
                SELECT nombre
                FROM   Activos_Principal
                WHERE  num_placa = @placa";

            return ExecuteSingleQuery(CodEmpresa, sql, string.Empty, new { placa });
        }


        /// <summary>
        /// Método para eliminar un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="placa"></param>
        /// <param name="Id_AddRet"></param>
        /// <returns></returns>
        public ErrorDto Activos_Deterioros_Eliminar(int CodEmpresa, string usuario, string placa, int Id_AddRet)
        {
            var result = new ErrorDto
            {
                Code        = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    DELETE Activos_retiro_adicion
                    WHERE  num_placa = @placa
                    AND    Id_AddRet = @Id_AddRet";

                connection.Execute(sql, new { placa, Id_AddRet });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"Deterioro, Placa: {placa}) Id: {Id_AddRet} ",
                    Movimiento        = "Elimina - WEB",
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
        /// Método para consultar el periodo actual
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