using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXCentrosCostosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXCentrosCostosDb(IConfiguration config) 
            : this( new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXCentrosCostosDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        public ErrorDto<List<CntXCentroCostosData>> CntXCentrosCostos_Obtener(int codEmpresa, int codConta, bool activo)
        {
            string query = @"select cod_centro_costo,descripcion,activo 
                from CntX_Centro_Costos where COD_CONTABILIDAD = @codConta";
            if (activo)
            {
                query += " and Activo = 1";
            }
            query += " order by cod_centro_costo";
            return DbHelper.ExecuteListQuery<CntXCentroCostosData>(_portalDb, codEmpresa, query, new { codConta });
        }

        public ErrorDto CntXCentrosCostos_Guardar(int codEmpresa, int codConta, string usuario, CntXCentroCostosData request)
        {
            const string sqlExists = @"
            select isnull(count(*),0) as Total from CntX_Centro_Costos 
                where cod_centro_costo = @CodCentroCosto and COD_CONTABILIDAD = @CodConta;";

            int total = DbHelper.ExecuteSingleQuery(
                _portalDb, codEmpresa, sqlExists, 0,
                new
                {
                    CodCentroCosto = request.cod_centro_costo,
                    CodConta = codConta
                }
            ).Result;

            if (total == 0) //Insertar
            {
                const string sqlInsert = @"
                insert into CntX_Centro_Costos(cod_centro_costo,COD_CONTABILIDAD,descripcion,activo)
                values (@CodCentroCosto, @CodConta, @Descripcion, @Activo);";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb, codEmpresa, sqlInsert,
                    new
                    {
                        CodCentroCosto = (request.cod_centro_costo ?? string.Empty).ToUpperInvariant(),
                        CodConta = codConta,
                        Descripcion = (request.descripcion ?? string.Empty).ToUpperInvariant(),
                        Activo = request.activo ? 1 : 0
                    }
                );

                if (respInsert != null && respInsert.Code < 0) { return respInsert; }

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Centro de Costo: {request.descripcion} Conta.{codConta}",
                    Movimiento = "Registra - WEB",
                    Modulo = 20
                });

                return new ErrorDto { Code = 0, Description = "Centro de costo registrado satisfactoriamente." };
            }
            else //Actualizar
            {
                const string sqlUpdate = @"update CntX_Centro_Costos set 
                    descripcion = @Descripcion, activo = @Activo
                    where COD_CONTABILIDAD = @CodContabilidad and cod_centro_costo = @CodCentroCosto;";

                var respUpdate = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlUpdate,
                    new
                    {
                        Descripcion = (request.descripcion ?? string.Empty).ToUpperInvariant(),
                        Activo = request.activo ? 1 : 0,
                        CodContabilidad = codConta,
                        CodCentroCosto = request.cod_centro_costo
                    }
                );

                if (respUpdate != null && respUpdate.Code < 0) { return respUpdate; }

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Centro de Costo: {request.descripcion} Conta.{codConta}",
                    Movimiento = "Modifica - WEB",
                    Modulo = 20
                });

                return new ErrorDto { Code = 0, Description = "Centro de costo actualizado satisfactoriamente." };
            }
        }

        public ErrorDto CntXCentrosCostos_Eliminar(int codEmpresa, int codConta, string usuario, string codCentroCosto)
        {
            const string sqlDelete = @"delete CntX_Centro_Costos
                where COD_CONTABILIDAD = @CodConta and cod_centro_costo = @CodCentroCosto;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sqlDelete,
                new
                {
                    CodCentroCosto = codCentroCosto,
                    CodConta = codConta
                }
            );

            if (respDelete != null && respDelete.Code < 0) { return respDelete; }

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Centro de Costo: {codCentroCosto} Conta.{codConta}",
                Movimiento = "Elimina - WEB",
                Modulo = 20
            });

            return new ErrorDto { Code = 0, Description = "Centro de costo eliminado satisfactoriamente." };
        }

        public ErrorDto<List<CntXCentroCostosUnidadesDto>> CntXCentrosCostos_Unidades_Obtener(
            int codEmpresa, int codConta, string codCentroCosto)
        {
            const string query = @"select C.cod_unidad, C.descripcion, 
                    case when A.cod_unidad is null then cast(0 as bit) else cast(1 as bit) end as existeX 
                 from CntX_Unidades C left join CntX_Unidades_CC A on C.cod_unidad = A.cod_unidad 
                 and C.cod_contabilidad = A.cod_contabilidad and A.cod_centro_costo =  @codCentroCosto 
                 and A.cod_contabilidad = @codConta where C.cod_contabilidad = @codConta 
                 order by ExisteX desc, C.cod_unidad;";

            return DbHelper.ExecuteListQuery<CntXCentroCostosUnidadesDto>(
                _portalDb, codEmpresa, query, new { codCentroCosto, codConta }
            );
        }

        public ErrorDto CntXCentrosCostos_Unidades_Asignar(int codEmpresa, int codConta, string codCentroCosto, string codUnidad, bool itemChecked)
        {
            string sql = itemChecked
                ? @"insert CntX_unidades_cc (cod_unidad, cod_centro_costo, cod_contabilidad)
            values (@CodUnidad, @CodCentroCosto, @CodConta);"
                : @"delete CntX_unidades_cc
            where cod_unidad = @CodUnidad
              and cod_centro_costo = @CodCentroCosto
              and cod_contabilidad = @CodConta;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sql,
                new
                {
                    CodUnidad = codUnidad,
                    CodCentroCosto = codCentroCosto,
                    CodConta = codConta
                }
            );

            if (resp != null && resp.Code < 0)
                return resp;

            return new ErrorDto { Code = 0 };
        }
    }
}
