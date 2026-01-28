using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXTiposAsientosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXTiposAsientosDb(IConfiguration config)
            : this(
                  new PortalDB(config), 
                  new MSecurityMainDb(config)
            )
        { }

        public FrmCntXTiposAsientosDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtener los tipos de asientos contables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXTiposAsientosData>> CntXTiposAsientos_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select tipo_asiento,descripcion,activo,consecutivo 
                from CntX_Tipos_Asientos where COD_CONTABILIDAD = @codConta order by descripcion";
            return DbHelper.ExecuteListQuery<CntXTiposAsientosData>(_portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Guardar tipo de asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXTiposAsientos_Guardar(int codEmpresa, int codConta, string usuario, CntXTiposAsientosData request)
        {
            const string sqlExists = @"
            select isnull(count(*),0) as Total from CntX_Tipos_Asientos 
                where tipo_asiento = @TipoAsiento and COD_CONTABILIDAD = @CodConta;";

            int total = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sqlExists,
                0,
                new
                {
                    TipoAsiento = request.tipo_asiento,
                    CodConta = codConta
                }
            ).Result;

            if (total == 0) //Insertar
            {
                const string sqlInsert = @"
                insert into CntX_Tipos_Asientos(tipo_asiento,COD_CONTABILIDAD,descripcion,activo,consecutivo) 
                values (@TipoAsiento, @CodConta, @Descripcion, @Activo, @Consecutivo);";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        TipoAsiento = (request.tipo_asiento ?? string.Empty).ToUpperInvariant(),
                        CodConta = codConta,
                        Descripcion = (request.descripcion ?? string.Empty).ToUpperInvariant(),
                        Activo = request.activo ? 1 : 0,
                        Consecutivo = request.consecutivo
                    }
                );

                if (respInsert != null && respInsert.Code < 0)
                    return respInsert;

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo Asiento : {request.descripcion} Conta.{codConta}",
                    Movimiento = "Registra - WEB",
                    Modulo = 20
                });

                return new ErrorDto { Code = 0, Description = "Tipo de asiento registrado satisfactoriamente." };
            }
            else //Actualizar
            {
                const string sqlUpdate = @"update CntX_Tipos_Asientos
                   set descripcion = @Descripcion, activo = @Activo, consecutivo = @Consecutivo
                 where COD_CONTABILIDAD = @CodContabilidad and tipo_asiento = @TipoAsiento;";

                var respUpdate = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlUpdate,
                    new
                    {
                        Descripcion = (request.descripcion ?? string.Empty).ToUpperInvariant(),
                        Activo = request.activo ? 1 : 0,
                        Consecutivo = request.consecutivo,
                        CodContabilidad = codConta,
                        TipoAsiento = request.tipo_asiento
                    }
                );

                if (respUpdate != null && respUpdate.Code < 0)
                    return respUpdate;

                return new ErrorDto { Code = 0, Description = "Tipo de asiento actualizado satisfactoriamente." };
            }
        }

        /// <summary>
        /// Eliminar tipo de asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoAsiento"></param>
        /// <returns></returns>
        public ErrorDto CntXTiposAsientos_Eliminar(int codEmpresa, int codConta, string usuario, string tipoAsiento)
        {
            const string sqlDelete = @"delete from CntX_Tipos_Asientos where 
                tipo_asiento = @TipoAsiento and COD_CONTABILIDAD = @CodConta;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    TipoAsiento = tipoAsiento,
                    CodConta = codConta
                }
            );

            if (respDelete != null && respDelete.Code < 0)
                return respDelete;

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Tipo Asiento : {tipoAsiento} Conta.{codConta}",
                Movimiento = "Elimina - WEB",
                Modulo = 20
            });

            return new ErrorDto { Code = 0, Description = "Tipo de asiento eliminado satisfactoriamente." };
        }

        /// <summary>
        /// Importar tipos de asientos contables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntXTiposAsientos_Importar(int codEmpresa, int codConta, string usuario)
        {
            const string sqlDelete = @"exec spCntX_Tipos_Asientos_Default @CodConta, @Usuario;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Usuario = usuario,
                    CodConta = codConta
                }
            );

            if (respDelete != null && respDelete.Code < 0)
                return respDelete;

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Importación de Tipos de Asientos Default, Conta Id: {codConta}",
                Movimiento = "Aplica - WEB",
                Modulo = 20
            });

            return new ErrorDto { Code = 0, Description = "Importacion realizada satisfactoriamente" };
        }
    }
}
