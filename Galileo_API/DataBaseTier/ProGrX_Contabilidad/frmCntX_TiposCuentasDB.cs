using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXTiposCuentasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXTiposCuentasDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCntXTiposCuentasDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtiene los tipos de cuentas contables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXTiposCuentasData>> CntXTiposCuentas_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select tipo_cuenta,descripcion,clasificacion,prioridad from CntX_Tipos_Cuentas
                where COD_CONTABILIDAD = @codConta order by prioridad,descripcion";
            return DbHelper.ExecuteListQuery<CntXTiposCuentasData>(_portalDb, codEmpresa, query, new {codConta});
        }

        /// <summary>
        /// Guardar el tipo de cuenta contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXTiposCuentas_Guardar(int codEmpresa, int codConta, string usuario, CntXTiposCuentasData request)
        {
            const string sqlExists = @"
            select isnull(count(*),0) as Total from CntX_Tipos_Cuentas
            where tipo_cuenta = @TipoCuenta
              and COD_CONTABILIDAD = @CodConta;";

            int total = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sqlExists,
                0,
                new
                {
                    TipoCuenta = request.tipo_cuenta,
                    CodConta = codConta
                }
            ).Result;

            if (total == 0) //Insertar
            {
                const string sqlInsert = @"
                insert into CntX_Tipos_Cuentas
                (tipo_cuenta, COD_CONTABILIDAD, descripcion, clasificacion, prioridad)
                values
                (@TipoCuenta, @CodConta, @Descripcion, @Clasificacion, @Prioridad);";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        TipoCuenta = (request.tipo_cuenta ?? string.Empty).ToUpperInvariant(),
                        CodConta = codConta,
                        Descripcion = (request.descripcion ?? string.Empty).ToUpperInvariant(),
                        Clasificacion = request.clasificacion,
                        Prioridad = request.prioridad
                    }
                );

                if (respInsert != null && respInsert.Code < 0)
                    return respInsert;

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Cuenta : {request.descripcion} Conta.{codConta}",
                    Movimiento = "Registra - WEB",
                    Modulo = 20
                });

                return new ErrorDto { Code = 0, Description = "Tipo de cuenta registrado satisfactoriamente." };
            }
            else //Actualizar
            {
                const string sqlUpdate = @"
                update CntX_Tipos_Cuentas
                   set descripcion = @Descripcion,
                       clasificacion = @Clasificacion,
                       prioridad = @Prioridad
                 where COD_CONTABILIDAD = @CodContabilidad
                   and tipo_cuenta = @TipoCuenta;";

                var respUpdate = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlUpdate,
                    new
                    {
                        Descripcion = (request.descripcion ?? string.Empty).ToUpperInvariant(),
                        Clasificacion = request.clasificacion,
                        Prioridad = request.prioridad,
                        CodContabilidad = codConta,
                        TipoCuenta = request.tipo_cuenta
                    }
                );

                if (respUpdate != null && respUpdate.Code < 0)
                        return respUpdate;

               return new ErrorDto { Code = 0, Description = "Tipo de cuenta actualizado satisfactoriamente." };
            } 
        }

        /// <summary>
        /// Eliminar el tipo de cuenta contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoCuenta"></param>
        /// <returns></returns>
        public ErrorDto CntXTiposCuentas_Eliminar(int codEmpresa, int codConta, string usuario, string tipoCuenta)
        {
            const string sqlDelete = @"delete from CntX_Tipos_Cuentas
                where tipo_cuenta = @TipoCuenta and COD_CONTABILIDAD = @CodConta;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    TipoCuenta = tipoCuenta,
                    CodConta = codConta
                }
            );

            if (respDelete != null && respDelete.Code < 0)
                return respDelete;

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Tipo Cuenta : {tipoCuenta} Conta.{codConta}",
                Movimiento = "Elimina - WEB",
                Modulo = 20
            });

            return new ErrorDto { Code = 0, Description = "Tipo de cuenta eliminado satisfactoriamente." };
        }
    }
}
