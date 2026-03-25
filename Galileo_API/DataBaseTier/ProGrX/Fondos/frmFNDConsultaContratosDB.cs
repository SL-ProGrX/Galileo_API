using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;

namespace Galileo_API.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndConsultaContratosDB
    {
        private readonly int vModulo = 18; // Modulo de Fondos
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDb;

        public FrmFndConsultaContratosDB(IConfiguration? config)
        {
            _Security_MainDB = new MSecurityMainDb(config!);
            _portalDb = new PortalDB(config!);
        }

        /// <summary>
        /// Consulta los socios disponibles para el formulario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdSociosData>> FND_ConsultaContratosSocios_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = $@"Select cedula,cedular,nombre from SOCIOS";

                return conn.Query<CrConsultaCrdSociosData>(query).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los contratos de un socio según su cédula y la opción seleccionada
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="vUsuario"></param>
        /// <param name="opcion"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaContratosData>> FND_ConsultaContratos_Contratos_Obtener(int CodEmpresa, string vCedula, string vUsuario ,string opcion)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = @"
                        Select  S.Nombre,
                                O.Descripcion,
                                P.Descripcion as DPlan,
                                F.Cod_Operadora,
                                F.Cod_plan,
                                F.cod_Contrato,
                                F.Estado,
                                F.Liq_Fecha,
                                F.Fecha_Inicio,
                                F.Monto,
                                F.Plazo,
                                F.Renueva,
                                F.Inc_Anual,
                                F.Inc_Tipo,
                                F.Aportes,
                                F.Rendimiento,
                                F.Operacion,
                                F.Monto_Transito
                        From Socios S
                        inner join Fnd_Contratos F on S.Cedula = F.Cedula
                        inner join Fnd_operadoras O on F.cod_operadora = O.cod_operadora
                        inner join Fnd_planes P on F.Cod_plan = P.Cod_plan
                        Where S.cedula = @cedula
                        AND dbo.fxFndColaboradorVisualiza(F.COD_OPERADORA, F.COD_PLAN, F.cedula,S.EstadoActual, @usuario) = 1
                        AND F.estado = 
                            CASE 
                                WHEN @opcion = '1' THEN 'A'
                                WHEN @opcion = '2' THEN 'L'
                                WHEN @opcion = '3' THEN 'I'
                                WHEN @opcion = '4' THEN 'B'
                                WHEN @opcion = '5' THEN 'C'
                                ELSE 'A'
                            END
                        ORDER BY 
                            CASE WHEN @opcion = '2' THEN F.Liq_Fecha END DESC,
                            F.Fecha_Inicio DESC,
                            F.cod_Plan,
                            F.cod_Contrato";

                return conn.Query<FndConsultaContratosData>(query, new { cedula = vCedula, usuario = vUsuario, opcion }).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los subcontratos de un contrato específico
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaSubContratosData>> FND_ConsultaContratos_SubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, string cod_contrato) 
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var qry = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                string cod_operadora = conn.QueryFirstOrDefault<string>(qry, new { cedula = vCedula }) ?? "";

                const string query = $@"select * from fnd_subCuentas where cod_operadora = @operadora and cod_plan = @cod_plan  and cod_contrato = @cod_contrato";

                return conn.Query<FndConsultaSubContratosData>(query,
                        new
                        {
                            operadora = cod_operadora,
                            cod_plan = cod_plan,
                            cod_contrato = cod_contrato
                        }).ToList();
            });
        }

        /// <summary>
        /// Método para obtener las liquidaciones de un socio según su cédula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaLiquidacionesData>> FND_ConsultaContratos_Liquidaciones_Obtener(int CodEmpresa, string vCedula)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = $@"select C.cod_plan,P.descripcion,C.cod_contrato,L.consec,L.fecha,L.usuario,L.aportes_liq+L.rendi_liq as 'Monto'
                                   ,L.traspaso_tesoreria,L.Traspaso_usuario,L.Solicitud_Tesoreria,isnull(L.Estado,'P') as 'Estado'
                                    from fnd_contratos C inner join fnd_liquidacion L on C.cod_operadora = L.cod_operadora
                                    and C.cod_plan = L.cod_plan and C.cod_Contrato = L.cod_contrato
                                    inner join fnd_planes P on C.cod_plan = P.cod_plan and P.cod_operadora = C.cod_operadora
                                    Where C.cedula = @cedula order by L.consec desc";

                return conn.Query<FndConsultaLiquidacionesData>(query, new { cedula = vCedula }).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los movimientos de un contrato específico
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="contrato"></param>
        /// <param name="cod_plan"></param>
        /// <param name="chkTodas"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaMovimientosData>> FND_ConsultaContratos_Movimiento_Obtener(
            int CodEmpresa, 
            string vCedula,
            FndConsultaMovimientosParams filtros)
        {

            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                string? vFechaIni = MProGrXAuxiliarDB.validaFechaGlobal(filtros.fechaInicio, "yyyy-MM-dd HH:mm:ss");
                string? vFechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(filtros.fechaCorte, "yyyy-MM-dd HH:mm:ss");

                // obtengo la operadora del socio
                var qry = @"select cod_operadora from fnd_contratos where cedula = @cedula";
                string cod_operadora = conn.QueryFirstOrDefault<string>(qry, new { cedula = vCedula }) ?? "";

                string query = @"
                        Select  D.cod_fnd_Detalle,
                                D.Monto,
                                D.Fecha_Proceso,
                                D.Fecha,
                                isnull(Doc.descripcion,'') as descripcion_mov,
                                D.nCon,
                                D.Fecha_Acredita,
                                D.cod_contrato,
                                D.Cod_plan,
                                P.descripcion
                        from fnd_contratos_detalle D
                        inner join fnd_planes P 
                            on D.cod_plan = P.cod_plan
                        inner join fnd_contratos C 
                            on D.cod_plan = C.cod_plan 
                           and D.cod_contrato = C.cod_contrato
                        left join SIF_Documentos Doc 
                            on D.Tcon = Doc.Tipo_Documento
                        where D.cod_operadora = @operadora
                          and C.cedula = @cedula
                          and (
                                @cod_plan is null
                                or ltrim(rtrim(@cod_plan)) = ''
                                or D.cod_plan = @cod_plan
                              )
                          and (
                                @contrato is null
                                or ltrim(rtrim(@contrato)) = ''
                                or D.Cod_Contrato = @contrato
                              )
                          and (
                                @chkTodas = 1
                                or D.Fecha between cast(@fechaDesde as datetime) and cast(@fechaHasta as datetime)
                              )
                        order by D.Fecha desc";

                return conn.Query<FndConsultaMovimientosData>(query, new
                {
                    operadora = cod_operadora,
                    cedula = vCedula,
                    cod_plan = filtros.plan,
                    contrato = filtros.contrato,
                    chkTodas = filtros.chkTodas,
                    fechaDesde = vFechaIni,
                    fechaHasta = vFechaCorte
                }).ToList();
            });

        }

        /// <summary>
        /// Método para obtener los planes asociados a la operadora del socio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_ConsultaContratos_Planes_Obtener(int CodEmpresa, string vCedula)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var qry  = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                string cod_operadora = conn.QueryFirstOrDefault<string>(qry, new { cedula = vCedula }) ?? "";

                const string query = $@"select cod_plan as item,descripcion from fnd_Planes where Cod_operadora= @operadora";

                return conn.Query<DropDownListaGenericaModel>(query, new { operadora = cod_operadora }).ToList();
            });
        }

        /// <summary>
        /// Método para reversar una liquidación
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="boleta"></param>
        /// <returns></returns>
        public ErrorDto FND_ConsultaContratos_Reversar(int CodEmpresa, string usuario, string boleta)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                var query = $@"exec spFndReversaLiq @boleta, @usuario";
                conn.Execute(query, new { boleta = boleta, usuario = usuario });

                //Bitacora
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Reversión de la Liquidación No.: {boleta}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Liquidación reversada correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
