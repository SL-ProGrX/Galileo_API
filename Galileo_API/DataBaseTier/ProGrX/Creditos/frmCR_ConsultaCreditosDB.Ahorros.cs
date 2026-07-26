using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region Ahorros

        /// <summary>
        /// Consulta los movimientos de ahorro de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaContratosData>> CR_ContratosConsulta_Obtener(int codEmpresa, string cedula, string usuario)
        {
            return EjecutarStoredProcedureList<CrConsultaContratosData>(
                codEmpresa,
                "spFndContratosConsulta",
                new { Cedula = cedula, Usuario = usuario });
        }

        /// <summary>
        /// Consulta los movimientos de ahorro de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosMovimientosData>> CR_Contratos_Movimientos_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return DbHelper.ExecuteListQuery<CrContratosMovimientosData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT 
                    Det.fecha,
                    Det.Fecha_Proceso,
                    Det.Monto,
                    ISNULL(Doc.Descripcion, '') AS DocDesc,
                    Det.nCon,
                    ISNULL(Con.Descripcion, '') AS ConDesc,
                    Det.Usuario,
                    Det.Detalle_01
                FROM fnd_contratos_detalle AS Det
                LEFT JOIN SIF_Documentos AS Doc 
                    ON Det.Tcon = Doc.Tipo_Documento
                LEFT JOIN SIF_Conceptos AS Con 
                    ON Det.Cod_Concepto = Con.Cod_Concepto
                WHERE Det.cod_operadora = @CodOperadora
                  AND Det.cod_plan = @CodPlan
                  AND Det.cod_contrato = @CodContrato
                ORDER BY Det.Fecha DESC, Det.COD_fnd_detalle DESC;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
        }

        /// <summary>
        /// Consulta los cupones de un contrato de ahorro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosCuponesData>> CR_Contratos_Cupones_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return DbHelper.ExecuteListQuery<CrContratosCuponesData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT 
                    Cupon_Id,
                    Fecha_Vence,
                    Monto_Base,
                    Tasa_Aplicada,
                    Cupon_Monto,
                    Rendimiento,
                    Principal,
                    Dias,
                    Estado_Desc,
                    Consec,
                    ISR_PORC,
                    ISR_MNT_GRAVABLE,
                    ISR_MONTO,
                    TOTAL_GIRAR,
                    Tesoreria_Id,
                    Tes_Documento,
                    Bancos_Estado,
                    IBAN
                FROM vFnd_Contratos_Cupones
                WHERE cod_operadora = @CodOperadora
                  AND cod_plan = @CodPlan
                  AND cod_contrato = @CodContrato
                ORDER BY Fecha_Vence;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
        }

        /// <summary>
        /// Consulta la bitacora de los contratos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosBitacoraData>> CR_Contratos_Bitacora_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return DbHelper.ExecuteListQuery<CrContratosBitacoraData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT 
                    C.ID_BITACORA,
                    C.COD_OPERADORA,
                    C.COD_PLAN,
                    C.COD_CONTRATO,
                    C.USUARIO,
                    C.FECHA,
                    C.MOVIMIENTO,
                    C.DETALLE,
                    C.REVISADO_USUARIO,
                    C.REVISADO_FECHA,
                    S.cedula,
                    S.nombre,
                    M.Descripcion AS MovimientoDesc,
                    CASE 
                        WHEN C.revisado_fecha IS NULL THEN 0 
                        ELSE 1 
                    END AS Revisado
                FROM fnd_contratos_cambios AS C
                INNER JOIN fnd_contratos AS X 
                    ON C.cod_operadora = X.cod_operadora
                   AND C.cod_plan = X.cod_plan
                   AND C.cod_contrato = X.cod_contrato
                INNER JOIN Socios AS S 
                    ON X.cedula = S.cedula
                INNER JOIN US_MOVIMIENTOS_BE AS M 
                    ON C.Movimiento = M.Movimiento
                   AND M.modulo = 18
                WHERE C.cod_operadora = @CodOperadora
                  AND C.cod_plan = @CodPlan
                  AND C.cod_contrato = @CodContrato
                ORDER BY C.fecha DESC;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
        }

        /// <summary>
        /// Consulta los cierres de contratos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosCierresData>> CR_Contratos_Cierres_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return DbHelper.ExecuteListQuery<CrContratosCierresData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT TOP 36
                    A.Anio,
                    A.Mes,
                    A.Aportes,
                    A.Rendimientos,
                    (A.Aportes + A.Rendimientos) AS Total,
                    A.Monto_Transito,
                    A.Sobre_Giro,
                    A.Rend_Corte,
                    A.Ind_Deduccion,
                    A.Tipo_Deduc,
                    A.Porc_Deduc,
                    A.Monto,
                    A.Inversion,
                    A.Cashback_Pts_Corte,
                    A.Cashback_Pts_Otorgados,
                    A.Cashback_Pts_Redimidos,
                    A.Cod_Plan,
                    A.Cod_Contrato
                FROM FND_PER_CERRADOS AS A
                WHERE A.Cod_Operadora = @CodOperadora
                  AND A.Cod_Plan = @CodPlan
                  AND A.Cod_Contrato = @CodContrato
                ORDER BY A.Anio DESC, A.Mes DESC;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
        }

        /// <summary>
        /// Obtiene si la sesion esta activa o no
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<CajasSesionDto> Cajas_Sesion_ObtenerActiva(int codEmpresa, string usuario, string identificacion)
        {
            var result = DbHelper.ExecuteSingleQuery<CajasSesionDto>(
                CreatePortalDb(),
                codEmpresa,
                @"SELECT TOP 1 *
                  FROM CAJAS_SESION
                  WHERE cod_usuario = @Usuario
                    AND estado = 1
                    AND identificacion = @Identificacion",
                null,
                new { Usuario = usuario, Identificacion = identificacion });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CajasSesionDto>(result.Description ?? "Error al consultar sesión activa.", result.Code.GetValueOrDefault(-1), null);
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse<CajasSesionDto>("No se encontró sesión activa.", -2, null);
        }


        #endregion
    }
}

