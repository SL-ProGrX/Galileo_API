using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Org.BouncyCastle.Ocsp;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXTipoCambioDefinicionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20;

        public FrmCntXTipoCambioDefinicionDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXTipoCambioDefinicionDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtiene la lista de divisas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Obtener(int codEmpresa, int codConta)
        {
            string query = @"select rtrim(cod_divisa) as item, rtrim(descripcion) as descripcion
                From CntX_Divisas where cod_contabilidad = @codConta and divisa_local = 0";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Obtiene la lista de tipos de cambio para una divisa especifica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDivisa"></param>
        /// <param name="lineas"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXTipoCambioData>> CntXTipoCambio_Obtener(int codEmpresa, int codConta, string codDivisa, int lineas)
        {
            string query = $@"SELECT TOP {lineas} ID_Cambio,TC_Compra,TC_Venta,Inicio,Corte,Variacion, cod_divisa 
                FROM CntX_Divisas_Tipo_Cambio where cod_divisa = @codDivisa 
                and COD_CONTABILIDAD = @codConta 
                order by id_Cambio desc";
            return DbHelper.ExecuteListQuery<CntXTipoCambioData>(
                _portalDb, codEmpresa, query, new { codDivisa, codConta });
        }

        /// <summary>
        /// Guarda un tipo de cambio (creacion o actualizacion)
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXTipoCambio_Guardar(int codEmpresa, int codConta, string usuario, CntXTipoCambioData request)
        {
            const string sqlExists = @"
                select isnull(count(*), 0) as Total
                  from CntX_Divisas_Tipo_Cambio
                 where id_cambio = @IdCambio
                   and cod_divisa = @CodDivisa
                   and COD_CONTABILIDAD = @CodConta;";

            int total = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sqlExists,
                0,
                new
                {
                    IdCambio = request.id_cambio,
                    CodDivisa = request.cod_divisa,
                    CodConta = codConta
                }
            ).Result;

            if (total == 0) // Insertar
            {
                const string sqlGetNextId = @"
                    select isnull(max(id_cambio), 0) + 1 as Ultimo
                      from CntX_Divisas_Tipo_Cambio
                     where cod_divisa = @CodDivisa
                       and COD_CONTABILIDAD = @CodConta;";

                int nuevoId = DbHelper.ExecuteSingleQuery(
                    _portalDb,
                    codEmpresa,
                    sqlGetNextId,
                    0,
                    new
                    {
                        CodDivisa = request.cod_divisa,
                        CodConta = codConta
                    }
                ).Result;

                const string sqlInsert = @"
                insert into CntX_Divisas_Tipo_Cambio
                (
                    ID_Cambio,
                    COD_CONTABILIDAD,
                    cod_divisa,
                    usuario,
                    fecha,
                    tc_Compra,
                    tc_venta,
                    Inicio,
                    Corte,
                    variacion
                )
                values
                (
                    @IdCambio,
                    @CodConta,
                    @CodDivisa,
                    @Usuario,
                    getdate(),
                    @TcCompra,
                    @TcVenta,
                    @Inicio,
                    @Corte,
                    @Variacion
                );";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        IdCambio = nuevoId,
                        CodConta = codConta,
                        CodDivisa = (request.cod_divisa ?? string.Empty).ToUpperInvariant(),
                        Usuario = usuario,
                        TcCompra = request.tc_compra,
                        TcVenta = request.tc_venta,
                        Inicio = request.inicio.Date,
                        Corte = request.corte.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                        Variacion = request.variacion
                    }
                );

                if (respInsert != null && respInsert.Code < 0)
                    return respInsert;

                const string sqlSpUpdate = @"
                exec spCntX_DivisasTC_Update
                     @CodConta,
                     @CodDivisa,
                     @FechaCorte,
                     @TcCompra,
                     @TcVenta;";

                var respSp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlSpUpdate,
                    new
                    {
                        CodConta = codConta,
                        CodDivisa = (request.cod_divisa ?? string.Empty).ToUpperInvariant(),
                        FechaCorte = request.corte.Date,
                        TcCompra = request.tc_compra,
                        TcVenta = request.tc_venta
                    }
                );

                if (respSp != null && respSp.Code < 0)
                    return respSp;

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    $"Tipo Cambio : ID-{nuevoId} Divisa : {request.cod_divisa} Conta.{codConta}",
                    "Registra - WEB"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Tipo de cambio registrado satisfactoriamente."
                };
            }
            else // Actualizar
            {
                const string sqlUpdate = @"
                    update CntX_Divisas_Tipo_Cambio
                       set tc_Compra = @TcCompra,
                           tc_Venta = @TcVenta,
                           inicio = @Inicio,
                           corte = @Corte,
                           variacion = @Variacion
                     where COD_CONTABILIDAD = @CodConta
                       and cod_divisa = @CodDivisa
                       and Id_Cambio = @IdCambio;";

                var respUpdate = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlUpdate,
                    new
                    {
                        TcCompra = request.tc_compra,
                        TcVenta = request.tc_venta,
                        Inicio = request.inicio.Date,
                        Corte = request.corte.Date,
                        Variacion = request.variacion,
                        CodConta = codConta,
                        CodDivisa = (request.cod_divisa ?? string.Empty).ToUpperInvariant(),
                        IdCambio = request.id_cambio
                    }
                );

                if (respUpdate != null && respUpdate.Code < 0)
                    return respUpdate;

                const string sqlSpUpdate = @"
                exec spCntX_DivisasTC_Update
                     @CodConta,
                     @CodDivisa,
                     @FechaCorte,
                     @TcCompra,
                     @TcVenta;";

                var respSp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlSpUpdate,
                    new
                    {
                        CodConta = codConta,
                        CodDivisa = (request.cod_divisa ?? string.Empty).ToUpperInvariant(),
                        FechaCorte = request.corte.Date,
                        TcCompra = request.tc_compra,
                        TcVenta = request.tc_venta
                    }
                );

                if (respSp != null && respSp.Code < 0)
                    return respSp;

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    $"Tipo Cambio : ID-{request.id_cambio} Divisa : {request.cod_divisa} Conta.{codConta}",
                    "Modifica - WEB"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Tipo de cambio actualizado satisfactoriamente."
                };
            }
        }

        /// <summary>
        /// Elimina un tipo de cambio especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="codDivisa"></param>
        /// <param name="idCambio"></param>
        /// <returns></returns>
        public ErrorDto CntXTipoCambio_Eliminar(int codEmpresa, int codConta, string usuario, string codDivisa, int idCambio)
        {
            const string sqlDelete = @"
                delete from CntX_Divisas_Tipo_Cambio
                where COD_CONTABILIDAD = @CodConta
                  and cod_divisa = @CodDivisa
                  and ID_Cambio = @IdCambio;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodConta = codConta,
                    CodDivisa = codDivisa,
                    IdCambio = idCambio
                }
            );

            if (respDelete != null && respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa, 
                usuario, 
                $"Tipo Cambio : ID-{idCambio} Divisa : {codDivisa} Conta.{codConta}",
                "Elimina - WEB"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Tipo de cambio eliminado satisfactoriamente."
            };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
