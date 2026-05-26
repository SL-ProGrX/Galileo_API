using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXConCierreDB
    {
        private readonly PortalDB _portalDb;

        public FrmCntXConCierreDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de definiciones de consolidación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <returns>ErrorDto con la lista de definiciones</returns>
        public ErrorDto<FrmCntXConCierreLista> AF_CntXConCierre_Obtener(int codEmpresa)
        {
            string query = "SELECT Cod_Consolida, Descripcion FROM CNTX_CONSOLIDA_DEFINICION";
            var lista = DbHelper.ExecuteListQuery<FrmCntXConCierreData>(_portalDb, codEmpresa, query);

            return new ErrorDto<FrmCntXConCierreLista>
            {
                Code = 0,
                Description = "OK",
                Result = new FrmCntXConCierreLista
                {
                    Total = lista.Result?.Count ?? 0,
                    Lista = lista.Result ?? []
                }
            };
        }        

        /// <summary>
        /// Obtiene la lista de contabilidades y niveles para una consolidación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codConsolida">Código de consolidación</param>
        /// <returns>ErrorDto con la lista de definiciones</returns>
        public ErrorDto<FrmCntXConCierreDefinicionLista> AF_CntXConCierre_ObtenerDefinicion(int codEmpresa, int codConsolida)
        {
            string query = @"SELECT COD_CONTABILIDAD, nivel 
                             FROM CNTX_CONSOLIDA_DEFINICION 
                             WHERE cod_consolida = @CodConsolida";
            var lista = DbHelper.ExecuteListQuery<FrmCntXConCierreDefinicionData>(_portalDb, codEmpresa, query, new { CodConsolida = codConsolida });

            return new ErrorDto<FrmCntXConCierreDefinicionLista>
            {
                Code = 0,
                Description = "OK",
                Result = new FrmCntXConCierreDefinicionLista
                {
                    Total = lista.Result?.Count ?? 0,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Valida si existe periodo contable base abierto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="mes">Mes</param>
        /// <param name="anio">Año</param>
        /// <param name="codContabilidad">Código de contabilidad base</param>
        /// <returns>ErrorDto con el resultado de la validación</returns>
        public ErrorDto AF_CntXConCierre_ValidaPeriodoBase(int codEmpresa, int mes, int anio, int codContabilidad)
        {
            string query = @"SELECT ISNULL(COUNT(*),0) AS Existe 
                             FROM CntX_Periodos 
                             WHERE estado = 'P' 
                               AND mes = @Mes
                               AND anio = @Anio
                               AND cod_contabilidad = @CodContabilidad";
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { Mes = mes, Anio = anio, CodContabilidad = codContabilidad }).Result;
            return existe > 0
                ? new ErrorDto { Code = 0, Description = "Periodo base abierto." }
                : new ErrorDto { Code = -1, Description = "No existe periodo base abierto." };
        }

        /// <summary>
        /// Valida si existe periodo contable local abierto para consolidación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="mes">Mes</param>
        /// <param name="anio">Año</param>
        /// <param name="codConsolida">Código de consolidación</param>
        /// <returns>ErrorDto con el resultado de la validación</returns>
        public ErrorDto AF_CntXConCierre_ValidaPeriodoLocal(int codEmpresa, int mes, int anio, int codConsolida)
        {
            string query = @"SELECT ISNULL(COUNT(*),0) AS Existe 
                             FROM CntX_Periodos 
                             WHERE mes = @Mes
                               AND anio = @Anio
                               AND estado = 'P'
                               AND COD_CONTABILIDAD IN (
                                    SELECT COD_CONTABILIDAD 
                                    FROM CNTX_CONSOLIDA_DEFINICION_DET 
                                    WHERE cod_consolida = @CodConsolida
                               )";
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { Mes = mes, Anio = anio, CodConsolida = codConsolida }).Result;
            return existe > 0
                ? new ErrorDto { Code = 0, Description = "Periodo local abierto." }
                : new ErrorDto { Code = -1, Description = "No existe periodo local abierto." };        
        }

        /// <summary>
        /// Obtiene portales y credenciales para una consolidación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codConsolida">Código de consolidación</param>
        /// <returns>ErrorDto con la lista de portales</returns>
        public ErrorDto<FrmCntXConCierrePortalLista> AF_CntXConCierre_ObtenerPortales(int codEmpresa, int codConsolida)
        {
            string query = @"SELECT C.cod_portal, C.COD_CONTABILIDAD, P.por_user, P.por_password, P.por_server, P.por_database
                             FROM CNTX_CONSOLIDA_PORTALES_CON C
                             INNER JOIN CNTX_CONSOLIDA_PORTALES_CON P ON C.cod_portal = P.cod_portal
                             WHERE C.cod_consolida = @CodConsolida";
            var lista = DbHelper.ExecuteListQuery<FrmCntXConCierrePortalData>(_portalDb, codEmpresa, query, new { CodConsolida = codConsolida });
            return new ErrorDto<FrmCntXConCierrePortalLista>
            {
                Code = 0,
                Description = "OK",
                Result = new FrmCntXConCierrePortalLista
                {
                    Total = lista.Result?.Count ?? 0,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Valida si existe periodo contable para una contabilidad, con opción de filtrar por estado abierto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="mes">Mes</param>
        /// <param name="anio">Año</param>
        /// <param name="codContabilidad">Código de contabilidad</param>
        /// <param name="soloAbierto">Si true, filtra por estado 'P'</param>
        /// <returns>ErrorDto con el resultado de la validación</returns>
        public ErrorDto AF_CntXConCierre_ValidaPeriodo(int codEmpresa, int mes, int anio, int codContabilidad, bool soloAbierto)
        {
            string query = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM CntX_Periodos
                             WHERE mes = @Mes AND anio = @Anio AND COD_CONTABILIDAD = @CodContabilidad";
            if (soloAbierto)
                query += " AND estado = 'P'";
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { Mes = mes, Anio = anio, CodContabilidad = codContabilidad }).Result;
            return existe > 0
                ? new ErrorDto { Code = 0, Description = "Existe periodo." }
                : new ErrorDto { Code = -1, Description = "No existe periodo." };
        }

        /// <summary>
        /// Inserta un nuevo periodo contable.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="anio">Año</param>
        /// <param name="mes">Mes</param>
        /// <param name="codContabilidad">Código de contabilidad</param>
        /// <returns>ErrorDto con el resultado de la inserción</returns>
        public ErrorDto AF_CntXConCierre_InsertarPeriodo(int codEmpresa, int anio, int mes, int codContabilidad)
        {
            string query = @"INSERT INTO CntX_Periodos(anio, mes, estado, cod_contabilidad)
                             VALUES (@Anio, @Mes, 'P', @CodContabilidad)";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { Anio = anio, Mes = mes, CodContabilidad = codContabilidad });
        }

        /// <summary>
        /// Inserta movimientos consolidados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codConsolida">Código de consolidación</param>
        /// <param name="codContabilidad">Código de contabilidad</param>
        /// <param name="anio">Año</param>
        /// <param name="mes">Mes</param>
        /// <param name="nivel">Nivel</param>
        /// <returns>ErrorDto con el resultado de la inserción</returns>
        public ErrorDto AF_CntXConCierre_InsertarMovimientos(int codEmpresa, int codConsolida, int codContabilidad, int anio, int mes, int nivel)
        {
            string query = @"INSERT INTO con_movimientos(
                                cod_consolida, COD_CONTABILIDAD, anio, mes, cod_cuenta, saldo_inicial, total_debitos, total_creditos)
                            SELECT @CodConsolida, @CodContabilidad, @Anio, @Mes, M.cod_cuenta,
                                   ISNULL(SUM(M.saldo_inicial),0), ISNULL(SUM(M.total_debitos),0), ISNULL(SUM(M.Total_creditos),0)
                            FROM movimiento_cuentas M
                            INNER JOIN Cuentas X ON M.COD_CONTABILIDAD = X.COD_CONTABILIDAD AND M.cod_cuenta = X.cod_cuenta
                            INNER JOIN CNTX_CONSOLIDA_DEFINICION_DET C ON M.COD_CONTABILIDAD = C.COD_CONTABILIDAD
                            WHERE M.mes = @Mes AND M.anio = @Anio AND C.cod_consolida = @CodConsolida AND X.nivel <= @Nivel
                            GROUP BY M.cod_cuenta";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { CodConsolida = codConsolida, CodContabilidad = codContabilidad, Anio = anio, Mes = mes, Nivel = nivel });
        }

        /// <summary>
        /// Actualiza los saldos de un movimiento consolidado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="req">Datos del movimiento a actualizar</param>
        /// <returns>ErrorDto con el resultado de la actualización</returns>
        public ErrorDto AF_CntXConCierre_ActualizarMovimiento(int codEmpresa, FrmCntXConCierreActualizarMovimientoRequest req)
        {
            string query = @"UPDATE con_movimientos
                             SET saldo_inicial = saldo_inicial + @SI,
                                 total_debitos = total_debitos + @TD,
                                 total_creditos = total_creditos + @TC
                             WHERE cod_consolida = @Cod_Consolida
                               AND COD_CONTABILIDAD = @Cod_Contabilidad
                               AND Anio = @Anio
                               AND mes = @Mes
                               AND cod_cuenta = @Cod_Cuenta";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, req);
        }
    }
}
