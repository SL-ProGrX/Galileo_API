using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXCatalogoCuentasDB
    {
        /// <summary>
        /// Guarda o actualiza una prorrata de cuenta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa usada para resolver la conexión.</param>
        /// <param name="request">Datos de la cuenta, unidad, centro de costo y porcentaje.</param>
        /// <returns>Resultado de éxito o error de la operación.</returns>
        public ErrorDto<bool> CntXCatalogoProrrataGuardar(int codEmpresa, CntXCatalogoProrrataGuardarRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sqlExiste = @"
                    select count(1)
                    from CNTX_CUENTAS_PRORRATA
                    where cod_contabilidad = @CodContabilidad
                      and cod_cuenta = @Cuenta
                      and cod_unidad = @CodUnidad
                      and cod_centro_costo = @CodCentroCosto";

                int existe = conn.ExecuteScalar<int>(sqlExiste, request);

                if (existe == 0)
                {
                    const string sqlInsert = @"
                        insert into CNTX_CUENTAS_PRORRATA
                            (cod_contabilidad, cod_cuenta, cod_unidad, cod_centro_costo, porcentaje, registro_usuario, registro_fecha)
                        values
                            (@CodContabilidad, @Cuenta, @CodUnidad, @CodCentroCosto, @Porcentaje, @Usuario, dbo.MyGetdate())";
                    conn.Execute(sqlInsert, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Prorrateo: Conta.{request.CodContabilidad}, Cta: {request.Cuenta}, Unidad: {request.CodUnidad}, Centro: {request.CodCentroCosto}", MovimientoRegistraWeb);
                }
                else
                {
                    const string sqlUpdate = @"
                        update CNTX_CUENTAS_PRORRATA
                        set porcentaje = @Porcentaje
                        where cod_contabilidad = @CodContabilidad
                          and cod_cuenta = @Cuenta
                          and cod_unidad = @CodUnidad
                          and cod_centro_costo = @CodCentroCosto";
                    conn.Execute(sqlUpdate, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Prorrateo: Conta.{request.CodContabilidad}, Cta: {request.Cuenta}, Unidad: {request.CodUnidad}, Centro: {request.CodCentroCosto}", MovimientoModificaWeb);
                }

                return true;
            });
        }

        /// <summary>
        /// Elimina una prorrata de cuenta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa usada para resolver la conexión.</param>
        /// <param name="request">Identificación de la cuenta, unidad y centro de costo.</param>
        /// <returns>Resultado de éxito o error de la operación.</returns>
        public ErrorDto<bool> CntXCatalogoProrrataEliminar(int codEmpresa, CntXCatalogoProrrataGuardarRequest request)
        {
            const string sql = @"
                delete CNTX_CUENTAS_PRORRATA
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta = @Cuenta
                  and cod_unidad = @CodUnidad
                  and cod_centro_costo = @CodCentroCosto";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.Execute(sql, request) > 0);

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Prorrateo: Conta.{request.CodContabilidad}, Cta: {request.Cuenta}, Unidad: {request.CodUnidad}, Centro: {request.CodCentroCosto}", MovimientoEliminaWeb);
            }

            return result;
        }
    }
}
