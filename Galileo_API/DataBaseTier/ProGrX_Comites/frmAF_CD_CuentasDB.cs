using Dapper;
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

        public ErrorDto<AfCdCuentaData?> AfCdCuentas_Scroll_Obtener(int codEmpresa, int operacion, int scrollCode)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
                select top 1 R.NOperacion
                from Afi_CD_Cuentas R
                where (
                        (@scrollCode = 1 and R.NOperacion > @numOperacion)
                     or (@scrollCode <> 1 and R.NOperacion < @numOperacion)
                      )
                order by
                    case when @scrollCode = 1 then R.NOperacion end asc,
                    case when @scrollCode <> 1 then R.NOperacion end desc;";

                var operacionDestino = conn.QueryFirstOrDefault<int?>(query, new
                {
                    numOperacion = operacion,
                    scrollCode
                });

                var numeroObjetivo = operacionDestino ?? operacion;

                return AfCdCuenta_Obtener(codEmpresa, numeroObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<AfCdCuentaData?>(ex.Message);
            }
        }

        public ErrorDto<List<AfCdActividadData>> AfCdActividades_Lista_Obtener(
            int codEmpresa, string tipo, int totalAsoc, int operacion, int comite)
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
            string query = @"select NOperacion, Cod_Comite, Cedula, Saldo from afi_cd_Cuentas order by NOperacion desc";
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
            string query = @"exec spSys_Cuentas_Bancarias @cedula, @idBanco, 1";
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

        public ErrorDto<List<AfCdCuentaData>> AfCdLiquidacionesPendientes_Obtener(int codEmpresa, int codComite)
        {
            string query = @"
            select 
                A.noperacion as Noperacion,
                C.notas as Notas,
                sum(A.monto) as Monto,
                C.estado as Estado,
                C.tesoreria_nsolicitud as TesoreriaNSolicitud,
                C.liquida_fecha as LiquidaFecha
            from afi_cd_cuentas C
            inner join afi_cd_cuentas_actividades A 
                on C.noperacion = A.noperacion
            where C.cod_comite = @Comite
              and C.PROCESO = 'T'
            group by 
                A.noperacion,
                C.notas,
                C.estado,
                C.tesoreria_nsolicitud,
                C.liquida_fecha";

            return DbHelper.ExecuteListQuery<AfCdCuentaData>(
                _portalDb, codEmpresa, query, new { Comite = codComite });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdCargos_Lista_Obtener(int codEmpresa)
        {
            string query = @"Select CODIGO as item, DESCRIPCION from AFI_CD_CARGOS where ESTADO = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }


    }
}
