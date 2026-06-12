using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasDatosAsociadosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCajasDatosAsociadosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los creditos asociados a una persona.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="cedula">Identificacion de la persona.</param>
        /// <returns>Lista de creditos asociados.</returns>
        public ErrorDto<List<CajasCreditoDto>> Cajas_Consulta_Creditos(
            int codEmpresa,
            string cedula)
        {
            const string sql = "exec spCajas_Consulta_Creditos @cedula";
            return DbHelper.ExecuteListQuery<CajasCreditoDto>(_portalDb, codEmpresa, sql, new { cedula });
        }

        /// <summary>
        /// Obtiene los fondos asociados a una persona.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="cedula">Identificacion de la persona.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <returns>Lista de fondos asociados.</returns>
        public ErrorDto<List<CajasFondosDto>> Cajas_Consulta_Fondos(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            const string sql = "exec spCajas_Consulta_Fondos @cedula, @usuario";
            return DbHelper.ExecuteListQuery<CajasFondosDto>(_portalDb, codEmpresa, sql, new { cedula, usuario });
        }

        /// <summary>
        /// Obtiene las cuentas por cobrar asociadas a una persona.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="cedula">Identificacion de la persona.</param>
        /// <returns>Lista de cuentas por cobrar asociadas.</returns>
        public ErrorDto<List<CajasCxcDto>> Cajas_Consulta_CxC(
            int codEmpresa,
            string cedula)
        {
            const string sql = "exec spCxC_PersonasCuentas @cedula, 'A'";
            return DbHelper.ExecuteListQuery<CajasCxcDto>(_portalDb, codEmpresa, sql, new { cedula });
        }

        /// <summary>
        /// Obtiene los servicios asociados a una persona.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="cedula">Identificacion de la persona.</param>
        /// <returns>Lista de servicios asociados.</returns>
        public ErrorDto<List<CajasServiciosDto>> Cajas_Consulta_Servicios(
            int codEmpresa,
            string cedula)
        {
            const string sql = "exec spCajas_Consulta_Servicios @cedula";
            return DbHelper.ExecuteListQuery<CajasServiciosDto>(_portalDb, codEmpresa, sql, new { cedula });
        }

        /// <summary>
        /// Obtiene los saldos a favor asociados a una persona.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="cedula">Identificacion de la persona.</param>
        /// <param name="liquidados">Indica si se consultan saldos liquidados o pendientes.</param>
        /// <returns>Lista de saldos a favor asociados.</returns>
        public ErrorDto<List<CajasSaldoFavorDto>> Cajas_Consulta_SaldosFavor(
            int codEmpresa,
            string cedula,
            bool liquidados)
        {
            const string sql = @"
                SELECT
                    LINEA AS linea,
                    DOC_TIPO AS documento,
                    REGISTRO_FECHA AS fecha,
                    MONTO AS monto,
                    SALDO AS saldo,
                    'Tes. Id.: ' + CAST(DOC_TRANSAC_ID AS varchar)
                        + ' ¦ Caja .: ' + COD_CAJA
                        + '  Ap.Id.: ' + CAST(COD_APERTURA AS varchar)
                        AS referencia
                FROM CAJAS_SALDO_FAVOR
                WHERE cedula = @cedula
                  AND (
                        (@liq = 1 AND saldo <= 0)
                     OR (@liq = 0 AND saldo > 0)
                  )
                ORDER BY REGISTRO_FECHA DESC";

            return DbHelper.ExecuteListQuery<CajasSaldoFavorDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { cedula, liq = liquidados ? 1 : 0 });
        }

        /// <summary>
        /// Obtiene los recibos multiples asociados a una persona.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="cedula">Identificacion de la persona.</param>
        /// <returns>Lista de recibos multiples asociados.</returns>
        public ErrorDto<List<CajasReciboMultipleDto>> Cajas_Consulta_RecibosMultiples(
            int codEmpresa,
            string cedula)
        {
            const string sql = @"
                SELECT
                    CAJA_AM_ID AS recibo,
                    MONTO AS monto,
                    REGISTRO_FECHA AS fecha,
                    COD_CAJA AS caja,
                    COD_APERTURA AS apertura,
                    REGISTRO_USUARIO AS usuario
                FROM CAJAS_AM_MAIN
                WHERE cedula = @cedula
                ORDER BY REGISTRO_FECHA DESC";

            return DbHelper.ExecuteListQuery<CajasReciboMultipleDto>(_portalDb, codEmpresa, sql, new { cedula });
        }

        /// <summary>
        /// Obtiene la cedula y nombre asociados a una operacion de credito.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="operacion">Numero de operacion de credito.</param>
        /// <returns>Datos basicos de la persona asociada a la operacion.</returns>
        public ErrorDto<CajasDatosPersonaDto?> Cajas_DatosPersona_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            const string sql = @"
                SELECT TOP 1
                    s.cedula AS cedula,
                    s.nombre AS nombre
                FROM reg_Creditos r
                INNER JOIN socios s ON s.cedula = r.cedula
                WHERE r.id_solicitud = @operacion";

            return DbHelper.ExecuteSingleQuery<CajasDatosPersonaDto>(
                _portalDb,
                codEmpresa,
                sql,
                default,
                new { operacion });
        }

        /// <summary>
        /// Obtiene nombre, indicadores judiciales/incobrables y acceso restringido de una persona.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="cedula">Identificacion de la persona.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <returns>Datos de validacion de la persona.</returns>
        public ErrorDto<CajasDatosPersonaDto> Cajas_DatosPersona_Validar(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"
                    SELECT TOP 1
                        cedula AS cedula,
                        nombre AS nombre,
                        CAST(dbo.fxCBR_Cobro_Judicial_Indica(Cedula) AS bit) AS cobroJudicial,
                        CAST(dbo.fxCbr_Incobrables_Indicador(Cedula) AS bit) AS incobrable
                    FROM Socios
                    WHERE cedula = @cedula";

                var datos = conn.QueryFirstOrDefault<CajasDatosPersonaDto>(sql, new { cedula })
                    ?? new CajasDatosPersonaDto { cedula = cedula, nombre = string.Empty };

                const string sqlRa = "exec spSYS_RA_Consulta_Status @cedula, @usuario";
                var acceso = conn.QueryFirstOrDefault<CajasRaStatusDto>(
                    sqlRa,
                    new { cedula, usuario });

                datos.expedienteRestringido = acceso != null
                    && acceso.PERSONA_ID > 0
                    && acceso.AUTORIZACION_ID == 0;

                if (datos.expedienteRestringido)
                {
                    datos.mensaje = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorizacion para Consultar!";
                }
                else if (datos.cobroJudicial && datos.incobrable)
                {
                    datos.mensaje = "Presenta Creditos en Cobro Judicial e Incobrables";
                }
                else if (datos.cobroJudicial)
                {
                    datos.mensaje = "Presenta Creditos en Cobro Judicial";
                }
                else if (datos.incobrable)
                {
                    datos.mensaje = "Presenta Registro de Incobrables";
                }

                return datos;
            });
        }

        private sealed class CajasRaStatusDto
        {
            public int PERSONA_ID { get; set; }
            public int AUTORIZACION_ID { get; set; }
        }
    }
}
