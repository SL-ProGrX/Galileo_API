using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosExploradorDB
    {
        private readonly PortalDB _portalDB;

        public FrmActivosExploradorDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

       
        private ErrorDto<List<T>> EjecutarLista<T>(int codEmpresa,Func<SqlConnection, List<T>> query)
        {
            string connString = _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<T>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<T>()
            };

            try
            {
                using var cn = new SqlConnection(connString);
                response.Result = query(cn);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<T>();
            }

            return response;
        }

   
        /// <summary>
        /// Obtiene los departamentos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Departamentos(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn => cn.Query<DropDownListaGenericaModel>(@"
                    SELECT
                        cod_departamento AS item,
                        descripcion
                    FROM Activos_Departamentos
                    ORDER BY cod_departamento
                ").ToList()
            );
        }
        /// <summary>
        /// Obtiene las secciones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codDepartamento"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Secciones(
            int codEmpresa,
            string codDepartamento)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn => cn.Query<DropDownListaGenericaModel>(@"
                    SELECT cod_seccion AS item, descripcion
                    FROM Activos_Secciones
                    WHERE cod_departamento = @codDepartamento
                    ORDER BY cod_seccion", new { codDepartamento }).ToList()
            );
        }

        /// <summary>
        /// Obtiene los tipos de activos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposActivo(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn => cn.Query<DropDownListaGenericaModel>(@"
                    SELECT TIPO_ACTIVO AS item, descripcion
                    FROM Activos_TIPO_ACTIVO
                    ORDER BY TIPO_ACTIVO
                ").ToList()
            );
        }

        /// <summary>
        /// Obtiene las justificaciones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Justificaciones(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn => cn.Query<DropDownListaGenericaModel>(@"
                    SELECT COD_JUSTIFICACION AS item, descripcion
                    FROM Activos_JUSTIFICACIONES
                    ORDER BY COD_JUSTIFICACION").ToList()
            );
        }

        /// <summary>
        /// Obtiene las ubicaciones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Ubicaciones(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn => cn.Query<DropDownListaGenericaModel>(@"
                    SELECT RTRIM(COD_LOCALIZA) AS item,RTRIM(descripcion) AS descripcion
                    FROM ACTIVOS_LOCALIZACIONES
                    WHERE Activa = 1
                    ORDER BY descripcion
                ").ToList()
            );
        }

        /// <summary>
        /// Obtiene las responsables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Responsables(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn => cn.Query<DropDownListaGenericaModel>(@"
                    SELECT RTRIM(Identificacion) AS item, RTRIM(Nombre) AS descripcion
                    FROM Activos_Personas
                    ORDER BY Nombre
                ").ToList()
            );
        }

        /// <summary>
        /// Obtiene los proveedores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Proveedores(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn => cn.Query<DropDownListaGenericaModel>(@"
                    SELECT RTRIM(cod_proveedor) AS item, RTRIM(descripcion) AS descripcion
                    FROM Activos_proveedores
                    ORDER BY descripcion
                ").ToList()
            );
        }

        /// <summary>
        /// Obtiene los periodos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>

        public ErrorDto<List<PeriodoExploradorDto>> Periodos(int codEmpresa,string estado)
        {
            return EjecutarLista<PeriodoExploradorDto>(
                codEmpresa,
                cn => cn.Query<PeriodoExploradorDto>(@"
                    SELECT
                        anio,
                        mes,
                        EOMONTH(DATEFROMPARTS(anio, mes, 1)) AS fecha_periodo,
                        UPPER(
                            DATENAME(MONTH, DATEFROMPARTS(anio, mes, 1))
                            + ' DE '
                            + CAST(anio AS VARCHAR)
                        ) AS periodo
                    FROM Activos_Periodos
                    WHERE estado = @estado
                    ORDER BY anio DESC, mes DESC
                ", new { estado }).ToList()
            );
        }

      
        /// <summary>
        /// Obtiene la fecha del servidor
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto FechaServidor_Obtener(int codEmpresa)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var cn = new SqlConnection(connString);

                const string sql = "SELECT dbo.MyGetdate()";

                var fechaServidor = cn.QuerySingle<DateTime>(sql);

                response.Description =
                    fechaServidor.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene la lista de activos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivoExploradorDto>> Listar( int codEmpresa,ActivosExploradorFiltrosDto f)
        {
            return EjecutarLista<ActivoExploradorDto>(codEmpresa,
                cn =>
                {
                    var fuente = f.tipoVisualizacion switch
                    {
                        "A" => @"
                            SELECT A.Num_Placa, A.Placa_Alterna, A.Nombre,
                                   A.Fecha_Adquisicion, A.Fecha_Instalacion,
                                   A.Tipo_Activo, A.TipoActivo AS Tipo_Activo_Desc,
                                   A.Valor_Historico, CAST(0 AS decimal(18,2)) AS Valor_Desecho,
                                   A.Estado, A.Identificacion, A.Responsable,
                                   A.cod_Departamento, A.Departamento,
                                   A.cod_Seccion, A.Seccion,
                                   A.cod_Localiza, A.Localizacion,
                                   A.cod_Proveedor, A.Proveedor,
                                   A.Vida_Util, A.Descripcion, A.Modelo, A.Marca,
                                   A.Num_Serie, A.Otras_Senas,
                                   ISNULL(A.Depreciacion_Ac,0) - ISNULL(A.Depreciacion_Mes,0) AS depreciacion_anterior,
                                   ISNULL(A.Depreciacion_Mes,0) AS depreciacion_mes,
                                   ISNULL(A.Depreciacion_Ac,0) AS depreciacion_acumulada,
                                   ISNULL(A.Valor_Libros,0) AS valor_libros,
                                   A.Depreciacion_Periodo AS corte
                            FROM dbo.vActivos_depreciacion_actual A
                            WHERE A.Estado <> 'R'",
                        "C" => @"
                            SELECT A.Num_Placa, A.Placa_Alterna, A.Nombre,
                                   P.Fecha_Adquisicion, P.Fecha_Instalacion,
                                   A.Tipo_Activo, A.TipoActivo AS Tipo_Activo_Desc,
                                   A.Valor_Historico, ISNULL(P.Valor_Desecho,0) AS Valor_Desecho,
                                   P.Estado, A.Identificacion,
                                   A.Responsable_Nombre AS Responsable,
                                   A.cod_Departamento, A.Responsable_Departamento AS Departamento,
                                   A.cod_Seccion, A.Responsable_Seccion AS Seccion,
                                   P.cod_Localiza, A.Localizacion,
                                   P.cod_Proveedor, A.Proveedor,
                                   A.Vida_Util, P.Descripcion, P.Modelo, P.Marca,
                                   P.Num_Serie, P.Otras_Senas,
                                   ISNULL(A.Depreciacion_Ac_Consolidado,0) - ISNULL(A.Depreciacion_Mes_Consolidado,0) AS depreciacion_anterior,
                                   ISNULL(A.Depreciacion_Mes_Consolidado,0) AS depreciacion_mes,
                                   ISNULL(A.Depreciacion_Ac_Consolidado,0) AS depreciacion_acumulada,
                                   ISNULL(A.Valor_Libros_Consolidado,0) AS valor_libros,
                                   DATEFROMPARTS(A.Anio,A.Mes,1) AS corte
                            FROM dbo.vActivos_AuxiliarConsolidado A
                            INNER JOIN dbo.Activos_Principal P ON A.Num_Placa = P.Num_Placa
                            WHERE A.Anio = YEAR(@fechaPeriodo) AND A.Mes = MONTH(@fechaPeriodo)",
                        _ => @"
                            SELECT Num_Placa, Placa_Alterna, Nombre,
                                   Fecha_Adquisicion, Fecha_Instalacion,
                                   Tipo_Activo, Tipo_Activo_Desc,
                                   Valor_Historico, Valor_Desecho, Estado,
                                   Identificacion, Responsable,
                                   cod_Departamento, Departamento,
                                   cod_Seccion, Seccion,
                                   cod_Localiza, Localizacion,
                                   cod_Proveedor, Proveedor,
                                   Vida_Util, Descripcion, Modelo, Marca, Num_Serie, Otras_Senas,
                                   CAST(NULL AS decimal(18,2)) AS depreciacion_anterior,
                                   CAST(NULL AS decimal(18,2)) AS depreciacion_mes,
                                   CAST(NULL AS decimal(18,2)) AS depreciacion_acumulada,
                                   ISNULL(Valor_Libros_Periodo,0) AS valor_libros,
                                   CAST(NULL AS datetime) AS corte
                            FROM dbo.vActivos_General"
                    };

                    var sql = new StringBuilder($@"
                        SELECT TOP (@lineas) X.*
                        FROM ({fuente}) X
                        WHERE 1 = 1");

                    var param = new DynamicParameters();
                    param.Add("lineas", Math.Clamp(f.lineas ?? 1000, 1, 100000));
                    param.Add("fechaPeriodo", f.fecha_periodo ?? DateTime.Today);

                    if (!string.IsNullOrWhiteSpace(f.nombre))
                    {
                        sql.Append(" AND Nombre LIKE @nombre");
                        param.Add("nombre", $"%{f.nombre}%");
                    }
                    if (!string.IsNullOrWhiteSpace(f.descripcion))
                    {
                        sql.Append(" AND Descripcion LIKE @descripcion");
                        param.Add("descripcion", $"%{f.descripcion}%");
                    }
                    if (!string.IsNullOrWhiteSpace(f.modelo))
                    {
                        sql.Append(" AND Modelo LIKE @modelo");
                        param.Add("modelo", $"%{f.modelo}%");
                    }
                    if (!string.IsNullOrWhiteSpace(f.serie))
                    {
                        sql.Append(" AND Num_Serie LIKE @serie");
                        param.Add("serie", $"%{f.serie}%");
                    }
                    if (!string.IsNullOrWhiteSpace(f.marca))
                    {
                        sql.Append(" AND Marca LIKE @marca");
                        param.Add("marca", $"%{f.marca}%");
                    }
                    if (f.fecha_adq_activa && f.fecha_adq_desde.HasValue)
                    {
                        sql.Append(" AND Fecha_Adquisicion >= @fechaAdqDesde");
                        param.Add("fechaAdqDesde", f.fecha_adq_desde.Value.Date);
                    }
                    if (f.fecha_adq_activa && f.fecha_adq_hasta.HasValue)
                    {
                        sql.Append(" AND Fecha_Adquisicion < @fechaAdqHasta");
                        param.Add("fechaAdqHasta", f.fecha_adq_hasta.Value.Date.AddDays(1));
                    }
                    if (f.fecha_inst_activa && f.fecha_inst_desde.HasValue)
                    {
                        sql.Append(" AND Fecha_Instalacion >= @fechaInstDesde");
                        param.Add("fechaInstDesde", f.fecha_inst_desde.Value.Date);
                    }
                    if (f.fecha_inst_activa && f.fecha_inst_hasta.HasValue)
                    {
                        sql.Append(" AND Fecha_Instalacion < @fechaInstHasta");
                        param.Add("fechaInstHasta", f.fecha_inst_hasta.Value.Date.AddDays(1));
                    }
                    if (!string.IsNullOrWhiteSpace(f.tipo_activo)
                        && !f.tipo_activo.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                    {
                        sql.Append(" AND Tipo_Activo = @tipoActivo");
                        param.Add("tipoActivo", f.tipo_activo);
                    }
                    if (!string.IsNullOrWhiteSpace(f.departamento)
                        && !f.departamento.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                    {
                        sql.Append(" AND cod_Departamento = @departamento");
                        param.Add("departamento", f.departamento);
                    }

                    if (!string.IsNullOrWhiteSpace(f.seccion)
                        && !f.seccion.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                    {
                        sql.Append(" AND cod_Seccion = @seccion");
                        param.Add("seccion", f.seccion);
                    }

                    var ubicacion = !string.IsNullOrWhiteSpace(f.localiza) ? f.localiza : f.ubicacion;
                    if (!string.IsNullOrWhiteSpace(ubicacion)
                        && !ubicacion.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                    {
                        sql.Append(" AND cod_Localiza = @ubicacion");
                        param.Add("ubicacion", ubicacion);
                    }

                    if (!string.IsNullOrWhiteSpace(f.responsable_codigo))
                    {
                        sql.Append(" AND Identificacion = @responsable");
                        param.Add("responsable", f.responsable_codigo);
                    }

                    if (!string.IsNullOrWhiteSpace(f.proveedor_codigo))
                    {
                        sql.Append(" AND cod_Proveedor = @proveedor");
                        param.Add("proveedor", f.proveedor_codigo);
                    }

                    if (string.IsNullOrWhiteSpace(f.estado)
                        || f.estado.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(f.tipoVisualizacion, "C", StringComparison.OrdinalIgnoreCase))
                            sql.Append(" AND Estado = 'A'");
                    }
                    else
                    {
                        if (f.estado.Equals("D", StringComparison.OrdinalIgnoreCase))
                            sql.Append(" AND Estado = 'A' AND ISNULL(valor_libros,0) = 0");
                        else
                        {
                            sql.Append(" AND Estado = @estado");
                            param.Add("estado", f.estado);
                        }
                    }

                    var placaTipo = f.placa_tipo ?? f.tipoPlaca;
                    var placaInicio = f.placa_inicio ?? f.placaDesde;
                    var placaFin = f.placa_fin ?? f.placaHasta;
                    if (!string.IsNullOrWhiteSpace(placaInicio))
                    {
                        var campoPlaca = placaTipo.Equals("Alterna", StringComparison.OrdinalIgnoreCase)
                            ? "Placa_Alterna"
                            : "Num_Placa";
                        sql.Append($" AND {campoPlaca} >= @placaInicio");
                        param.Add("placaInicio", placaInicio);
                        if (!string.IsNullOrWhiteSpace(placaFin))
                        {
                            sql.Append($" AND {campoPlaca} <= @placaFin");
                            param.Add("placaFin", placaFin);
                        }
                    }

                    sql.Append(" ORDER BY Num_Placa");

                    return cn.Query<ActivoExploradorDto>(
                        sql.ToString(),
                        param
                    ).ToList();
                }
            );
        }


        /// <summary>
        /// Lista asientos por período (año/mes de fechaPeriodo)
        /// </summary>
        /// <param name="codEmpresa"
        /// <param name="fechaPeriodo"
        ///  <returns></returns>
        public ErrorDto<List<ActivosExploradorAsientoDto>> Asientos(int codEmpresa,DateTime fechaPeriodo)
        {
            return EjecutarLista<ActivosExploradorAsientoDto>(
                codEmpresa,
                cn => cn.Query<ActivosExploradorAsientoDto>(@"
            SELECT
                RTRIM(num_asiento)     AS num_asiento,
                RTRIM(tipo_asiento)    AS tipo_asiento,
                fecha_asiento          AS fecha_asiento,
                RTRIM(descripcion)     AS descripcion,
                ISNULL(debe,0)         AS debe,
                ISNULL(haber,0)        AS haber,
                RTRIM(aplicado)        AS aplicado,
                RTRIM(notas)           AS notas
            FROM dbo.vActivos_Asientos
            WHERE anio = YEAR(@fechaPeriodo)
              AND mes  = MONTH(@fechaPeriodo)
            ORDER BY fecha_asiento DESC, num_asiento DESC
        ", new { fechaPeriodo }).ToList()
            );
        }

        /// <summary>
        /// Lista detalle de un asiento por período
        /// </summary>
        /// <param name="codEmpresa"
        /// <param name="numAsiento"
        /// <param name="fechaPeriodo"
        ///  <returns></returns>
        public ErrorDto<List<ActivosExploradorAsientoDetalleDto>> AsientoDetalle(int codEmpresa,string numAsiento,DateTime fechaPeriodo)
        {
            return EjecutarLista<ActivosExploradorAsientoDetalleDto>(
                codEmpresa,
                cn => cn.Query<ActivosExploradorAsientoDetalleDto>(@"
            SELECT
                RTRIM(cuenta)       AS cuenta,
                RTRIM(descripcion)  AS descripcion,
                ISNULL(debito,0)    AS debito,
                ISNULL(credito,0)   AS credito,
                RTRIM(detalle)      AS detalle,
                RTRIM(referencia)   AS referencia,
                RTRIM(num_documento)AS num_documento
            FROM dbo.vActivos_Asientos_Detalle
            WHERE anio = YEAR(@fechaPeriodo)
              AND mes  = MONTH(@fechaPeriodo)
              AND RTRIM(num_asiento) = RTRIM(@numAsiento)
            ORDER BY cuenta
        ", new { numAsiento, fechaPeriodo }).ToList()
            );
        }

        /// <summary>
        /// Lista adiciones/retiros (modificaciones) por período
        /// </summary>
        /// <param name="codEmpresa"
        /// <param name="fechaPeriodo"
        ///  <returns></returns>
        public ErrorDto<List<ActivosExploradorModificacionDto>> AdicionesRetiros(int codEmpresa,DateTime fechaPeriodo)
        {
            return EjecutarLista<ActivosExploradorModificacionDto>(
                codEmpresa,
                cn => cn.Query<ActivosExploradorModificacionDto>(@"
            SELECT
                ISNULL(id_addret,0)     AS id_addret,
                RTRIM(nombre)          AS nombre,
                RTRIM(num_placa)       AS num_placa,
                RTRIM(tipo)            AS tipo,
                RTRIM(justificacion)   AS justificacion,
                fecha                  AS fecha,
                ISNULL(monto,0)        AS monto,
                RTRIM(descripcion)     AS descripcion
            FROM dbo.vActivos_Modificaciones
            WHERE anio = YEAR(@fechaPeriodo)
              AND mes  = MONTH(@fechaPeriodo)
            ORDER BY fecha DESC, num_placa
        ", new { fechaPeriodo }).ToList()
            );
        }

    }
}
