using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasAbonosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private readonly MCajas _mCajas;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MRecibos _mRecibos;
        private readonly MAfilicacionDB _mAfiliacion;
        private readonly int vModulo = 31;

        public FrmCxCCuentasAbonosDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config))
        {
        }

        public FrmCxCCuentasAbonosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            DBBitacora = dbBitacora;
        }

        public ErrorDto<CxCCuentasAbonosData> CxCCuentas_ConsultaOperacion_Obtener(int codEmpresa, string codCaja, int operacionId)
        {
            const string query = @"select R.Operacion,R.saldo,R.proceso,R.Tasa_Corriente,R.interesc,R.amortiza,
                    dbo.fxSIFFechaProcesoConvert(isnull(R.Fecha_UltMov,dbo.MyGetdate())) as 'Fecha_UltMov'
                    , R.cuota,R.cod_concepto,R.cedula,datediff(m,R.Activa_Fecha,dbo.MyGetdate()) as 'Meses'
                    , S.nombre,R.Activa_Fecha,R.Autoriza_Usuario 
                    , C.descripcion as 'ConceptoDesc',Ofi.descripcion as 'OficinaDesc', GETDATE() as 'FechaServer' 
                    , dbo.fxCajas_Valida_Auxiliar(@codCaja,'CxC',C.cod_Concepto) as 'Caja_Valida_Concepto'
                    , dbo.fxCxC_Operacion_Facturas_Pending(R.Operacion) as 'Facturas' 
                    from CxC_Cuentas R inner join CxC_Conceptos C on R.cod_concepto = C.cod_concepto 
                    inner join CxC_Personas S on R.cedula = S.cedula 
                    left join Sif_Oficinas Ofi on R.cod_Oficina = Ofi.cod_Oficina 
                    left join vCxC_CuentasMora V on R.Operacion = V.Operacion 
                    where R.estado = 'A' and R.saldo > 0 and R.Operacion = @operacionId";

            var result = DbHelper.ExecuteSingleQuery<CxCCuentasAbonosData>(_portalDb, codEmpresa, query, new CxCCuentasAbonosData(), new { codCaja, operacionId });

            if (result.Result == null)
            {
                result.Result = new CxCCuentasAbonosData();
            }
            return result!;
        }

        public ErrorDto<List<CxCCuotasActivasData>> CxCCuentas_CuotasActivas_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"
                SELECT * , CASE WHEN Dias_Mora > 0 THEN 'En Mora' ELSE 'Al Día' END AS estado_desc 
                FROM CxC_Cuentas_Mov
                WHERE estado = 'A' AND Operacion = @operacionId
                ORDER BY Linea;";

            return DbHelper.ExecuteListQuery<CxCCuotasActivasData>(_portalDb, codEmpresa, query, new { operacionId });
        }

        public ErrorDto<List<CxCOperacionesActivasData>> CxCCuentas_OperacionesActivas_Obtener(int codEmpresa)
        {
            const string query = @"
                Select R.Operacion,R.COD_CONCEPTO,S.Cedula,S.Nombre,C.Descripcion 
                FROM CxC_Cuentas R 
                    INNER JOIN CxC_Personas S ON R.cedula = S.cedula 
                    INNER JOIN CxC_Conceptos C ON R.COD_CONCEPTO = C.COD_CONCEPTO 
                WHERE R.ESTADO = 'A'";

            return DbHelper.ExecuteListQuery<CxCOperacionesActivasData>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_TipoDoc_Obtener(int codEmpresa, string caja)
        {
            const string query = @"select rTrim(C.tipo_documento) as item, rtrim(D.Descripcion) as descripcion
                from SIF_DOCUMENTOS D inner join CAJAS_DOCUMENTOS C on D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO  
                Where C.cod_caja = @caja and D.Tipo_Movimiento in('A','C') 
                order by C.tipo_documento";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { caja });
        }
    }
}
