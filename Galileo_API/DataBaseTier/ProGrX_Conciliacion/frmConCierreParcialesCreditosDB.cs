using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Conciliacion;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Conciliacion
{
    public sealed class FrmConCierreParcialesCreditosDB
    {
        private readonly PortalDB _portalDb;

        public FrmConCierreParcialesCreditosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el ultimo cierre parcial procesado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <returns>Ultimo registro de corte parcial.</returns>
        public ErrorDto<ConCierreParcialesCreditosUltimoCorteData?>
            ConCierreParcialesCreditos_UltimoCorte_Obtener(int codEmpresa)
        {
            const string sql = """
                -- @CodEmpresa: codigo de la empresa
                SELECT TOP 1
                    Corte,
                    Registro_Usuario,
                    Registro_Fecha
                FROM CRD_CIERRE_PARCIAL_CORTES
                WHERE Linea IN (
                    SELECT MAX(Linea)
                    FROM CRD_CIERRE_PARCIAL_CORTES
                );
                """;

            return DbHelper.ExecuteSingleQuery<ConCierreParcialesCreditosUltimoCorteData?>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Ejecuta el cierre parcial de cartera al corte indicado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="request">Fecha de corte y usuario.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto ConCierreParcialesCreditos_CierreParcial_Ejecutar(
            int codEmpresa,
            ConCierreParcialesCreditosCierreParcialRequest request)
        {
            const string sql = """
                -- @Fecha: fecha de corte
                -- @Usuario: usuario que ejecuta el proceso
                -- @Analisis: 1 para generar cubo de analisis
                exec spCrdCierreParcial
                    @Fecha,
                    @Usuario,
                    @Analisis;
                """;

            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                connection.Execute(
                    sql,
                    new
                    {
                        Fecha = request.Fecha_Corte,
                        request.Usuario,
                        Analisis = (short)1,
                    },
                    commandTimeout: 0);

                return true;
            });

            if (resp.Code < 0)
            {
                return new ErrorDto
                {
                    Code = resp.Code,
                    Description = resp.Description,
                };
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Ejecuta la proyeccion de cartera y retorna el resultado para exportacion.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="request">Fecha de inicio y cantidad de meses.</param>
        /// <returns>Filas proyectadas.</returns>
        public ErrorDto<List<Dictionary<string, object?>>>
            ConCierreParcialesCreditos_ProyeccionCartera_Ejecutar(
                int codEmpresa,
                ConCierreParcialesCreditosProyeccionRequest request)
        {
            const string sql = """
                -- @FechaInicio: fecha de inicio de la proyeccion
                -- @Periodos: cantidad de meses a proyectar
                -- @Tipo: M = mensual
                exec spCrdProyectaCartera
                    @FechaInicio,
                    @Periodos,
                    @Tipo;
                """;

            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                var registros = connection.Query(
                    sql,
                    new
                    {
                        FechaInicio = request.Fecha_Inicio,
                        Periodos = request.Meses,
                        Tipo = "M",
                    },
                    commandTimeout: 0);

                return registros
                    .Select(ConvertirFila)
                    .ToList();
            });
        }

        /// <summary>
        /// Ejecuta el proceso de producto acumulado al corte indicado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="request">Fecha de corte.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto ConCierreParcialesCreditos_ProductoAcumulado_Ejecutar(
            int codEmpresa,
            ConCierreParcialesCreditosProductoAcumuladoRequest request)
        {
            const string sql = """
                -- @Fecha: fecha de corte
                exec spSIFAuxProdAcumPP @Fecha;
                """;

            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                connection.Execute(
                    sql,
                    new { Fecha = request.Fecha_Corte },
                    commandTimeout: 0);

                return true;
            });

            if (resp.Code < 0)
            {
                return new ErrorDto
                {
                    Code = resp.Code,
                    Description = resp.Description,
                };
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Convierte una fila dinamica de Dapper en un diccionario serializable.
        /// </summary>
        /// <param name="registro">Fila retornada por el procedimiento.</param>
        /// <returns>Diccionario con los valores de la fila.</returns>
        private static Dictionary<string, object?> ConvertirFila(dynamic registro)
        {
            var fila = (IDictionary<string, object>)registro;

            return fila.ToDictionary(
                columna => columna.Key,
                columna => columna.Value is DBNull ? null : columna.Value);
        }
    }
}
