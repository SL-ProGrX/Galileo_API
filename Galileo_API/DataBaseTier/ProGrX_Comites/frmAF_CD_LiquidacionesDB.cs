using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdLiquidacionesDb
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdLiquidacionesDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmAfCdLiquidacionesDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            string query = @"select C.COD_COMITE as item,CM.DESCRIPCION from AFI_CD_CUENTAS C 
            inner join AFI_CD_COMITES CM on c.COD_COMITE = CM.COD_COMITE and C.ESTADO ='T' ";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<string?> AfCdComite_Descripcion_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select DESCRIPCION from AFI_CD_COMITES where COD_COMITE = @Comite";
            return DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, query, null,
                new { Comite = codComite });
        }

        public ErrorDto<int> AfCdLiquidaciones_Pendientes_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select count(COD_COMITE)as Cuenta 
            from AFI_CD_CUENTAS where estado='T' and COD_COMITE= @Comite";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0,
                new { Comite = codComite });
        }

        public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Lista_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select 
                C.NOPERACION as noperacion,
                C.ACTIVA_FECHA as activa_fecha,
                DATEDIFF(DAY, C.ACTIVA_FECHA, GETDATE()) as dias_pendientes,
                ISNULL(CA.MONTO, 0) as monto,
                A.DESCRIPCION as actividad,
                case C.ESTADO
                    when 'T' then 'Trasladado'
                    when 'A' then 'Activo'
                    else 'Liquidado'
                end as estado,
                case C.TIPO
                    when 'T' then 'Transferencia'
                    else 'Cheque'
                end as desembolso,
                C.REGISTRO_USUARIO as registro_usuario,
                Tes.FECHA_EMISION as fecha_emision
            from dbo.AFI_CD_CUENTAS C
            inner join AFI_CD_CUENTAS_ACTIVIDADES CA 
                on C.NOPERACION = CA.NOPERACION
            inner join AFI_CD_ACTIVIDADES A 
                on CA.COD_ACTIVIDAD = A.COD_ACTIVIDAD
            left join TES_TRANSACCIONES Tes 
                on C.TESORERIA_NSOLICITUD = Tes.NSOLICITUD
            where C.COD_COMITE = @Comite
              and C.ESTADO = 'T'
              and C.PROCESO = 'T'";
            return DbHelper.ExecuteListQuery<AfCdOperacionData>(_portalDb, codEmpresa, query, 
                new { Comite = codComite });
        }

        public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Detallar_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select 
                C.NOPERACION as noperacion,
                datediff(day, C.REGISTRO_FECHA, getdate()) as dias_pendientes,
                sum(CA.MONTO) as monto
            from dbo.AFI_CD_CUENTAS C
            inner join AFI_CD_CUENTAS_ACTIVIDADES CA 
                on C.NOPERACION = CA.NOPERACION
            where C.COD_COMITE = @Comite
              and C.ESTADO = 'T'
              and C.PROCESO = 'D'
            group by C.NOPERACION, C.REGISTRO_FECHA
            order by C.NOPERACION desc";

            return DbHelper.ExecuteListQuery<AfCdOperacionData>(
                _portalDb,
                codEmpresa,
                query,
                new { Comite = codComite }
            );
        }

        public ErrorDto<List<AfCdOperacionHistoricoData>> AfCdOperaciones_Historico_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select 
                C.NOPERACION as operacion,
                C.NOTAS as notas,
                C.LIQUIDA_FECHA as liquida_fecha,
                C.ACTIVA_FECHA as activa_fecha,
                Tes.FECHA_EMISION as fecha_emision,
                CA.MONTO as monto,
                A.DESCRIPCION as actividad,
                C.TESORERIA_FECHA as tesoreria_fecha,
                C.TESORERIA_NSOLICITUD as tesoreria_nsolicitud,
                case C.ESTADO 
                    when 'T' then 'Trasladado' 
                    when 'A' then 'Activo' 
                    else 'Liquidado' 
                end as estado,
                case C.APRUEBA 
                    when 'J' then 'Junta Directiva' 
                    when 'O' then 'Oficina Regional' 
                    else 'Director Zona' 
                end as aprueba,
                case C.TIPO 
                    when 'T' then 'Transferencia' 
                    else 'Cheque' 
                end as desembolso,
                C.REGISTRO_FECHA as registro_fecha,
                C.REGISTRO_USUARIO as registro_usuario,
                Tes.Beneficiario as tesoreria_beneficiario,
                Tes.Codigo as tesoreria_codigo
            from dbo.AFI_CD_CUENTAS C
            inner join AFI_CD_CUENTAS_ACTIVIDADES CA 
                on C.NOPERACION = CA.NOPERACION
            inner join AFI_CD_ACTIVIDADES A 
                on CA.COD_ACTIVIDAD = A.COD_ACTIVIDAD
            left join TES_TRANSACCIONES Tes 
                on C.TESORERIA_NSOLICITUD = Tes.NSOLICITUD
            where C.COD_COMITE = @Comite
            order by C.REGISTRO_FECHA desc";

            return DbHelper.ExecuteListQuery<AfCdOperacionHistoricoData>(
                _portalDb, codEmpresa, query,
                new { Comite = codComite }
            );
        }

        public ErrorDto<List<AfCdFacturaData>> AfCdFacturas_Obtener(int codEmpresa, int operacion)
        {
            string query = @"select 
                DEPOSITO as deposito,
                NDOCUMENTO as ndocumento,
                FECHA_DOCUMENTO as fecha_documento,
                DETALLE as detalle,
                MONTO as monto
            from AFI_CD_DETALLE_LIQUIDACION
            where NOPERACION = @Operacion";

            return DbHelper.ExecuteListQuery<AfCdFacturaData>(
                _portalDb,
                codEmpresa,
                query,
                new { Operacion = operacion }
            );
        }
    }
}
