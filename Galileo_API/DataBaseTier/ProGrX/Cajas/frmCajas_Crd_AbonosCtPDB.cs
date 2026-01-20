using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCrdAbonosCtPDb
    {
        private readonly PortalDB _portalDb;

        public FrmCajasCrdAbonosCtPDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCajasCrdAbonosCtPDb(PortalDB portalDB) => _portalDb = portalDB;

        public ErrorDto<CajasCrdAbonosCtPData> CajasCrdAbonosCtP_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"select R.id_solicitud,R.saldo, R.saldo - isnull(V.amortiza,0) As Saldo_mes,R.proceso, isnull(R.cod_Divisa,'COL') as 'divisa'
                    ,R.interesv,R.int,R.plazo,R.interesc,R.amortiza,R.fecult,R.Prideduc, isnull(C.Impuesto,0) as 'IVA_Aplica'
                    , R.opex,R.cuota,R.codigo,R.cedula,R.cuotas_planilla,R.cuotas_directas, datediff(m,R.fechaforp,dbo.MyGetdate()) as 'Meses'
                    , S.nombre,C.descripcion,C.retencion,C.poliza,R.fechaforp,C.PORC_CARGO_CANCELACION,C.ANTICIPO_MESES,R.Base_Calculo
                    , dbo.fxCrdPlanPagosDiasActivo(@OperacionId) as 'DiasActivo', dbo.fxCrdOperacionTagReg(R.id_solicitud,'S15') as 'AutPagoAnt'
                    , C.descripcion as 'LineaDesc',Ofi.descripcion as 'OficinaDesc',Pre.Descripcion as 'RecursoDesc',dbo.MyGetdate() as 'FechaServer'
                    , dbo.fxCajas_Valida_Auxiliar(@CodCaja,'CRD',R.Codigo) as 'Caja_Valida_Concepto'
                    , dbo.fxCrd_Operacion_Control(R.id_solicitud) as 'Control'
                    , dbo.fxCrd_IVA_Porc() as 'IVA_Porc'
                    from reg_creditos R inner join Catalogo C on R.codigo = C.codigo 
                    inner join Socios S on R.cedula = S.cedula
                    left join Sif_Oficinas Ofi on R.cod_Oficina_R = Ofi.cod_Oficina
                    left join CATALOGO_GRUPOS Pre on R.cod_grupo = Pre.cod_grupo
                    left join vista_morosidad V on R.id_solicitud = V.id_solicitud
                    where R.estado = 'A' and R.saldo > 0
                    and R.ID_SOLICITUD = @OperacionId";

                var op = conn.QueryFirstOrDefault<CajasCrdAbonosCtPData>(query, new { CodCaja, OperacionId }) ?? new CajasCrdAbonosCtPData();

                op.Saldo_mes = op.Saldo_mes < 0 ? 0 : op.Saldo_mes;
                if (op.Saldo_mes == 0)
                {
                    var updateSQl = "update reg_creditos set saldo_mes = saldo where id_solicitud = @id_solicitud";
                    conn.Execute(updateSQl, new { Saldo_mes = op.Saldo_mes, id_solicitud = op.id_solicitud });

                    op.Saldo_mes = op.saldo;
                }

                return op;
            });
        }

        public ErrorDto<List<CajasCrdAbonosCtPData>> CajasCrdAbonosCtP_Operaciones_Obtener(int CodEmpresa)
        {
            const string query = @"SELECT R.id_solicitud,R.Codigo,S.Cedula,S.Nombre,C.Descripcion 
                from REG_CREDITOS R inner join SOCIOS S on R.cedula = S.cedula 
                inner join Catalogo C on R.codigo = C.codigo 
                WHERE R.estado = 'A' ORDER BY R.cedula";

            return DbHelper.ExecuteListQuery<CajasCrdAbonosCtPData>(_portalDb, CodEmpresa, query);
        }

        public ErrorDto<List<CajasCrdOperacionTransacData>> CajasCrdAbonosCtP_OperacionTransac_Obtener(int CodEmpresa, int IdSolicitud)
        {
            const string sql = @"select * from CRD_OPERACION_TRANSAC 
                where estado = 'A' and id_solicitud = @IdSolicitud 
                and Fecha_Inicio < GETDATE() order by ID_SEQ asc";
            return DbHelper.ExecuteListQuery<CajasCrdOperacionTransacData>(_portalDb, CodEmpresa, sql, new {IdSolicitud});
        }
    }
}
