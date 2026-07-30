using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRastreoMovimientosDb
    {
        private readonly PortalDB _portalDB;

        public FrmCntXRastreoMovimientosDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Busca los movimientos
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="filtros">Filtros capturados en la pantalla.</param>
        /// <returns>Movimientos que cumplen con los filtros.</returns>
        public ErrorDto<List<RastreoMovimientosTablaDto>> Buscar(int codEmpresa,RastreoMovimientosFiltroDto filtros)
        {
            var sql = new StringBuilder();
            var where = new StringBuilder();

            ConstruirSelect(sql);
            ConstruirFiltros(where, filtros);

            if (where.Length > 0)
                sql.Append(" WHERE ").Append(where);

            sql.Append(" ORDER BY D.fecha_asiento, D.cod_cuenta ");

            return DbHelper.ExecuteListQuery<RastreoMovimientosTablaDto>(
                _portalDB,
                codEmpresa,
                sql.ToString(),
                ObtenerParametros(filtros)
            );
        }

        /// <summary>
        /// Agrega al SQL las tablas y columnas requeridas por el rastreo.
        /// </summary>
        /// <param name="sql">Constructor del comando SQL.</param>
        private static void ConstruirSelect(StringBuilder sql)
        {
            sql.Append(@"
                SELECT
                    C.cod_cuenta_mask AS cuenta,
                    C.descripcion,
                    D.fecha_asiento AS fecha,
                    D.tipo_asiento,
                    D.num_asiento,
                    D.monto_debito AS debitos,
                    D.monto_credito AS creditos,
                    E.nombre AS empresa,
                    D.documento,
                    D.detalle
                FROM CntX_Asientos A
                INNER JOIN CntX_Asientos_detalle D
                    ON A.COD_CONTABILIDAD = D.COD_CONTABILIDAD
                    AND A.tipo_asiento = D.tipo_asiento
                    AND A.num_asiento = D.num_asiento
                INNER JOIN CNTX_CONTABILIDADES E
                    ON D.COD_CONTABILIDAD = E.COD_CONTABILIDAD
                INNER JOIN CntX_Cuentas C
                    ON D.COD_CONTABILIDAD = C.COD_CONTABILIDAD
                    AND D.cod_cuenta = C.cod_cuenta
            ");
        }

        /// <summary>
        /// Agrega los filtros seleccionados por el usuario.
        /// </summary>
        /// <param name="where">Constructor de condiciones SQL.</param>
        /// <param name="filtros">Filtros capturados en la pantalla.</param>
        private static void ConstruirFiltros(
            StringBuilder where,
            RastreoMovimientosFiltroDto filtros
        )
        {
            AgregarFiltro(where,
                "D.fecha_asiento BETWEEN @fechaInicio AND @fechaCorte",
                filtros.fechaInicio.HasValue && filtros.fechaCorte.HasValue);

            AgregarFiltro(where,
                "C.cod_cuenta_mask BETWEEN @cuentaInicio AND @cuentaCorte",
                !string.IsNullOrEmpty(filtros.cuentaInicio) &&
                !string.IsNullOrEmpty(filtros.cuentaCorte));

            AgregarFiltro(
                where,
                "D.documento LIKE @documento",
                !string.IsNullOrWhiteSpace(filtros.documento));

            AgregarFiltro(
                where,
                "D.detalle LIKE @detalle",
                !string.IsNullOrWhiteSpace(filtros.detalle));

            if (filtros.codigo.HasValue)
            {
                AgregarFiltro(
                    where,
                    filtros.tipo == "Consolidacion"
                        ? @"E.COD_CONTABILIDAD IN (
                                SELECT COD_CONTABILIDAD
                                FROM CNTX_CONSOLIDA_DEFINICION_DET
                                WHERE cod_consolida = @codigo
                            )"
                        : "E.COD_CONTABILIDAD = @codigo",
                    true);
            }

            ConstruirFiltroParametro(where, filtros);
        }

        /// <summary>
        /// Agrega el filtro opcional por monto y tipo de movimiento.
        /// </summary>
        /// <param name="where">Constructor de condiciones SQL.</param>
        /// <param name="filtros">Filtros capturados en la pantalla.</param>
        private static void ConstruirFiltroParametro(
            StringBuilder where,
            RastreoMovimientosFiltroDto filtros
        )
        {
            if (!filtros.parametro.HasValue || filtros.parametro.Value <= 0)
                return;

            var operador = ObtenerOperador(filtros.signo);

            if (filtros.movimiento == "Ambos")
            {
                AppendAnd(where);
                where.Append($@"
                            (
                                D.monto_debito {operador} @parametro
                                OR D.monto_credito {operador} @parametro
                            )");
                return;
            }

            var campo = ObtenerCampoMovimiento(filtros.movimiento);

            AppendAnd(where);
            where.Append($"{campo} {operador} @parametro");
        }

        /// <summary>
        /// Obtiene la columna de monto correspondiente al movimiento.
        /// </summary>
        /// <param name="movimiento">Tipo de movimiento seleccionado.</param>
        /// <returns>Nombre seguro de la columna SQL.</returns>
        private static string ObtenerCampoMovimiento(string movimiento) => movimiento switch
                {
                    "Creditos" => "D.monto_credito",
                    "Debitos" => "D.monto_debito",
                    _ => "D.monto_debito"
                };

        /// <summary>
        /// Obtiene el operador SQL permitido para comparar montos.
        /// </summary>
        /// <param name="signo">Signo seleccionado.</param>
        /// <returns>Operador SQL validado.</returns>
        private static string ObtenerOperador(string signo) =>
            signo switch
            {
                "=" => "=",
                ">" => ">",
                "<" => "<",
                _ => "="
            };

        /// <summary>
        /// Agrega el conector AND cuando ya existen condiciones.
        /// </summary>
        /// <param name="where">Constructor de condiciones SQL.</param>
        private static void AppendAnd(StringBuilder where)
        {
            if (where.Length > 0)
                where.Append(" AND ");
        }

        /// <summary>
        /// Agrega una condición opcional al bloque WHERE.
        /// </summary>
        /// <param name="where">Constructor de condiciones SQL.</param>
        /// <param name="condicion">Condición SQL parametrizada.</param>
        /// <param name="aplicar">Indica si debe agregarse la condición.</param>
        private static void AgregarFiltro(StringBuilder where, string condicion, bool aplicar)
        {
            if (!aplicar) return;

            if (where.Length > 0)
                where.Append(" AND ");

            where.Append(condicion);
        }

        /// <summary>
        /// Construye los parámetros utilizados por la consulta de movimientos.
        /// </summary>
        /// <param name="filtros">Filtros capturados en la pantalla.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object ObtenerParametros(RastreoMovimientosFiltroDto filtros)
        {
            return new
            {
                filtros.codigo,
                fechaInicio = filtros.fechaInicio?.Date,
                fechaCorte = filtros.fechaCorte?.Date.AddDays(1).AddSeconds(-1),
                filtros.cuentaInicio,
                filtros.cuentaCorte,
                parametro = filtros.parametro,
                documento = $"%{filtros.documento}%",
                detalle = $"%{filtros.detalle}%"
            };
        }


        /// <summary>
        /// Busca las contabilidades
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tipo">Tipo de contabilidad solicitado.</param>
        /// <returns>Contabilidades o consolidaciones disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Contabilidades_Buscar(int codEmpresa,string tipo)
        {
            var sql = new StringBuilder();

            if (tipo == "Individual")
            {
                sql.Append(@"
                        SELECT 
                            CAST(COD_CONTABILIDAD AS VARCHAR) AS item,
                            RTRIM(NOMBRE) AS descripcion
                        FROM CNTX_CONTABILIDADES
                        ORDER BY COD_CONTABILIDAD
                    ");
            }
            else
            {
                sql.Append(@"
                        SELECT 
                            CAST(cod_consolida AS VARCHAR) AS item,
                            RTRIM(descripcion) AS descripcion
                        FROM CNTX_CONSOLIDA_DEFINICION
                        ORDER BY cod_consolida
                    ");
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql.ToString()
            );
        }

        /// <summary>
        /// Busca las cuentas asociadas a la contabilidad o consolidación indicada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tipo">Tipo de contabilidad seleccionado.</param>
        /// <param name="codigo">Código de contabilidad o consolidación.</param>
        /// <returns>Lista de cuentas disponibles para seleccionar.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Buscar(
            int codEmpresa,
            string tipo,
            int codigo)
        {
            var sql = tipo == "Consolidacion"
                ? @"
                    SELECT
                        RTRIM(C.cod_cuenta_mask) AS item,
                        RTRIM(C.descripcion) AS descripcion
                    FROM CntX_Cuentas C
                    WHERE C.COD_CONTABILIDAD = (
                        SELECT TOP 1 COD_CONTABILIDAD
                        FROM CNTX_CONSOLIDA_DEFINICION_DET
                        WHERE cod_consolida = @codigo
                        ORDER BY COD_CONTABILIDAD
                    )
                    ORDER BY C.cod_cuenta_mask"
                : @"
                    SELECT
                        RTRIM(cod_cuenta_mask) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM CntX_Cuentas
                    WHERE COD_CONTABILIDAD = @codigo
                    ORDER BY cod_cuenta_mask";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql,
                new { codigo }
            );
        }
    }
}

