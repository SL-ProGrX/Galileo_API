using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXCierresDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20;

        public FrmCntXCierresDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXCierresDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtener los cierres fiscales
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXCierreData>> CntXCierres_Obtener(int codEmpresa, int codConta)
        {
            string query = @"select id_cierre,inicio_anio,inicio_mes,corte_anio,corte_mes,descripcion
                ,cuenta_ganper,cuenta_utilidad,cuenta_imprenta,impuesto_renta,activo
                from CntX_Cierres 
                where COD_CONTABILIDAD = @codConta
                Order by inicio_anio desc,inicio_mes desc";
            return DbHelper.ExecuteListQuery<CntXCierreData>(_portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Guardar cierre fiscal
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXCierres_Guardar(int codEmpresa, int codConta, string usuario, CntXCierreData request)
        {
            if (request == null)
                return new ErrorDto { Code = -2, Description = "La informaci&oacute;n especificada no es v&aacute;lida, verifiquela..." };

            if (!FxVerificaCuentas(codEmpresa, codConta, request.cuenta_utilidad))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Cuenta puente no es v&aacute;lida, verifiquela..."
                };
            }

            if (!FxVerificaCuentas(codEmpresa, codConta, request.cuenta_ganper))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Cuenta ganancias/p&eacute;rdidas no es v&aacute;lida, verifiquela..."
                };
            }

            if (!FxVerificaCuentas(codEmpresa, codConta, request.cuenta_imprenta))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Cuenta impuesto no es v&aacute;lida, verifiquela..."
                };
            }

            string descripcion = (request.descripcion ?? string.Empty).Trim().ToUpperInvariant();

            string cuentaGanper = (request.cuenta_ganper ?? string.Empty).Trim();
            string cuentaUtilidad = (request.cuenta_utilidad ?? string.Empty).Trim();
            string cuentaImprenta = (request.cuenta_imprenta ?? string.Empty).Trim();

            decimal impuestoRenta = request.impuesto_renta;
            int activo = request.activo ? 1 : 0;

            if (request.id_cierre <= 0) // INSERT
            {
                const string sqlNext = @"
                select isnull(max(id_cierre),0) + 1
                from CntX_Cierres
                where cod_contabilidad = @CodConta;";

                int nuevoId = DbHelper.ExecuteSingleQuery(
                    _portalDb, codEmpresa, sqlNext, 0,
                    new { CodConta = codConta }
                ).Result;

                const string sqlInsert = @"
                insert into CntX_Cierres
                (id_cierre, COD_CONTABILIDAD, inicio_anio, inicio_mes, corte_anio, corte_mes,
                 descripcion, cuenta_ganper, cuenta_utilidad, cuenta_imprenta, impuesto_renta, activo)
                values
                (@IdCierre, @CodConta, @InicioAnio, @InicioMes, @CorteAnio, @CorteMes,
                 @Descripcion, @CuentaGanper, @CuentaUtilidad, @CuentaImprenta, @ImpuestoRenta, @Activo);";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb, codEmpresa, sqlInsert,
                    new
                    {
                        IdCierre = nuevoId,
                        CodConta = codConta,
                        InicioAnio = request.inicio_anio,
                        InicioMes = request.inicio_mes,
                        CorteAnio = request.corte_anio,
                        CorteMes = request.corte_mes,
                        Descripcion = descripcion,
                        CuentaGanper = cuentaGanper,
                        CuentaUtilidad = cuentaUtilidad,
                        CuentaImprenta = cuentaImprenta,
                        ImpuestoRenta = impuestoRenta,
                        Activo = activo
                    }
                );

                if (respInsert != null && respInsert.Code < 0) { return respInsert; }

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Cierre Fiscal : {nuevoId} Conta.{codConta}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                return new ErrorDto { Code = 0, Description = $"Cierre fiscal registrado satisfactoriamente. Id: {nuevoId}" };
            }
            else // UPDATE
            {
                const string sqlUpdate = @"
                update CntX_Cierres set
                    descripcion = @Descripcion,
                    inicio_anio = @InicioAnio,
                    inicio_mes = @InicioMes,
                    corte_anio = @CorteAnio,
                    corte_mes = @CorteMes,
                    cuenta_ganper = @CuentaGanper,
                    cuenta_utilidad = @CuentaUtilidad,
                    cuenta_imprenta = @CuentaImprenta,
                    impuesto_renta = @ImpuestoRenta,
                    activo = @Activo
                where id_cierre = @IdCierre and cod_contabilidad = @CodConta;";

                var respUpdate = DbHelper.ExecuteNonQuery(
                    _portalDb, codEmpresa, sqlUpdate,
                    new
                    {
                        Descripcion = descripcion,
                        InicioAnio = request.inicio_anio,
                        InicioMes = request.inicio_mes,
                        CorteAnio = request.corte_anio,
                        CorteMes = request.corte_mes,
                        CuentaGanper = cuentaGanper,
                        CuentaUtilidad = cuentaUtilidad,
                        CuentaImprenta = cuentaImprenta,
                        ImpuestoRenta = impuestoRenta,
                        Activo = activo,
                        IdCierre = request.id_cierre,
                        CodConta = codConta
                    }
                );

                if (respUpdate != null && respUpdate.Code < 0) { return respUpdate; }

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Cierre Fiscal : {request.id_cierre} Conta.{codConta}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                return new ErrorDto { Code = 0, Description = "Cierre fiscal actualizado satisfactoriamente." };
            }
        }

        /// <summary>
        /// Eliminar cierre fiscal
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="idCierre"></param>
        /// <returns></returns>
        public ErrorDto CntXCierres_Eliminar(int codEmpresa, int codConta, string usuario, int idCierre)
        {
            const string sqlDelete = @"delete CntX_Cierres 
                where COD_CONTABILIDAD = @CodConta and Activo = 1 and id_cierre = @IdCierre;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sqlDelete,
                new
                {
                    IdCierre = idCierre,
                    CodConta = codConta
                }
            );

            if (respDelete != null && respDelete.Code < 0) { return respDelete; }

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Cierre Fiscal : {idCierre} Conta.{codConta}",
                Movimiento = "Elimina - WEB",
                Modulo = 20
            });

            return new ErrorDto { Code = 0, Description = "Cierre fiscal eliminado satisfactoriamente." };
        }

        /// <summary>
        /// Verifica Cuentas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="pCuenta"></param>
        /// <returns></returns>
        private bool FxVerificaCuentas(int codEmpresa, int codConta, string pCuenta)
        {
            string query = @"select isnull(count(*),0) as Total from Cntx_Cuentas where COD_CONTABILIDAD = @codConta
                and cod_cuenta = @pCuenta and acepta_movimientos = 1";
            int total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { codConta, pCuenta }).Result;
            return total == 1;
        }
    }
}
