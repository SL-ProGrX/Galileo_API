using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdCuentasDb
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdCuentasDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmAfCdCuentasDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        public ErrorDto<AfCdCuentaData?> AfCdCuenta_Obtener(int codEmpresa, int operacion)
        {
            const string sql = @"EXEC spAFI_CD_Cuenta_Load @Operacion;";

            return DbHelper.ExecuteSingleQuery<AfCdCuentaData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new
                {
                    Operacion = operacion
                }
            );
        }

        public ErrorDto<List<AfCdActividadData>> AfCdActividades_Lista_Obtener(
            int codEmpresa, string tipo, int totalAsoc, int operacion, string comite)
        {
            const string query = @"EXEC spAFI_CD_Actividades_List @Tipo, @TotalAsoc, @Operacion, @Comite;";

            return DbHelper.ExecuteListQuery<AfCdActividadData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Tipo = tipo,
                    TotalAsoc = totalAsoc,
                    Operacion = operacion,
                    Comite = comite
                }
            );
        }

        public ErrorDto<List<AfCdCuentaAdjuntosData>> AfCdCuenta_Adjuntos_Obtener(int codEmpresa, int operacion)
        {
            const string query = @"exec spAFI_CD_Cuenta_Adjuntos @Operacion;";

            return DbHelper.ExecuteListQuery<AfCdCuentaAdjuntosData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Operacion = operacion
                }
            );
        }

        public ErrorDto<List<AfCdCuentaBitacoraData>> AfCdCuenta_Bitacora_Obtener(int codEmpresa, int operacion)
        {
            const string query = @"exec spAFI_CD_Cuenta_Bitacora @Operacion;";

            return DbHelper.ExecuteListQuery<AfCdCuentaBitacoraData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Operacion = operacion
                }
            );
        }

        public ErrorDto<List<AfCdCuentaData>> AfCdCuentas_Lista_Obtener(int codEmpresa)
        {
            string query = @"select NOperacion, Cod_Comite, Cedula, Saldo from afi_cd_Cuentas";
            return DbHelper.ExecuteListQuery<AfCdCuentaData>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            string query = @"select COD_COMITE as item, DESCRIPCION from AFI_CD_COMITES where ACTIVO = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdCatalogo_Lista_Obtener(int codEmpresa, string origen)
        {
            string query = "";
            switch (origen)
            {
                case "cboBanco":
                    query = "select id_banco as item, descripcion from AFI_CD_vBancos";
                    break;
                case "cboEmite":
                    query = "select CodTipoCuenta as item, NombreTipoCuenta as descripcion from AFI_CD_TIPO_CUENTA where Activo = 1";
                    break;
                case "cboActividadTipo":
                    query = "select CodTipoActividad as item, NombreTipoActividad as descripcion from AFI_CD_TIPO_ACTIVIDAD where Activo = 1";
                    break;
                case "cboAutorizacion":
                    query = "select CodTipoAprobacion as item, NombreTipoAprobacion as descripcion from AFI_CD_TIPO_APROBACION where Activo = 1";
                    break;
            }
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<AfCdCuentaBancariaData>> AfCdCuentasBancarias_Obtener(int codEmpresa, string cedula, int idBanco)
        {
            string query = @"exec spSys_Cuentas_Bancarias @cedula, @idBanco";
            return DbHelper.ExecuteListQuery<AfCdCuentaBancariaData>(
                _portalDb, codEmpresa, query, new { cedula, idBanco });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdMiembros_Obtener(int codEmpresa, int codComite)
        {
            string query = @"select N.cedula as item, S.nombre as descripcion 
                from afi_cd_comites C left join afi_cd_nombramientos N on C.cod_comite = N.cod_comite 
                inner join socios S on S.cedula = N.cedula 
                where N.cod_comite = @Comite and N.APL_DESEMBOLSOS = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, codEmpresa, query, new { Comite = codComite });
        }
    }
}
