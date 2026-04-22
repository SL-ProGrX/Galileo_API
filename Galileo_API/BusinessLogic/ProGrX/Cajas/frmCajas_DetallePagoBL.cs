using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic
{
    public class FrmCajasDetallePagoBL
    {
        private readonly FrmCajasDetallePagoDB _db;

        public FrmCajasDetallePagoBL(IConfiguration config)
        {
            _db = new FrmCajasDetallePagoDB(config);
        }

        public ErrorDto<decimal> ObtenerTipoCambio(int codEmpresa, string codDivisa)
        {
            return _db.Cajas_TipoCambio(codEmpresa, codDivisa);
        }

        public ErrorDto Cajas_DesglocePago_Eliminar(int codEmpresa, string codCaja, int codApertura, string ticket, int linea)
        {
            return _db.Cajas_DesglocePago_Eliminar(codEmpresa, codCaja, codApertura, ticket, linea);
        }

        public ErrorDto<CajasDisponibleFondosDto> Cajas_DisponibleFondos(int codEmpresa, string codCaja, int codApertura, string ticket, string codPlan, int codContrato)
        {
            return _db.Cajas_DisponibleFondos(codEmpresa, codCaja, codApertura, ticket, codPlan, codContrato);
        }

        public ErrorDto<List<CajasSaldoFavorDetDto>> Cajas_SaldoFavor_Obtener(int codEmpresa, string clienteid, int referencia, string referencia_texto)
        {
            return _db.Cajas_SaldoFavor_Obtener(codEmpresa, clienteid, referencia, referencia_texto);
        }

        public ErrorDto<CajasDivisaFuncionalDto> Cajas_DivisaFuncional_Obtener(int codEmpresa, string enlace)
        {
            return _db.Cajas_DivisaFuncional_Obtener(codEmpresa, enlace);
        }

        public ErrorDto<List<CajasDepositosCuentasBancariasDto>> Cajas_DepositosCuentasBancariasAut_Obtener(int codEmpresa, string formaPago)
        {
            return _db.Cajas_DepositosCuentasBancariasAut_Obtener(codEmpresa, formaPago);
        }

        public ErrorDto<List<CajasDesglocePagoDto>> Cajas_DesglocePago_Obtener(int codEmpresa, string codCaja, string ticket, int codApertura, int linea)
        {

            return _db.Cajas_DesglocePago_Obtener(codEmpresa, codCaja, ticket, codApertura, linea);
        }

        public ErrorDto Cajas_DesglocePago_Insert(int codEmpresa, CajasDesglocePagoDto dto)
        {
            return _db.Cajas_DesglocePago_Insert(codEmpresa, dto);
        }

        public ErrorDto Cajas_DesglocePago_Update(int codEmpresa, CajasDesglocePagoDto dto)
        {
            return _db.Cajas_DesglocePago_Update(codEmpresa, dto);
        }

        public ErrorDto Cajas_DistribuyeSaldoFavor(int codEmpresa, DistribuyeSaldoFavorDto dto)
        {
            return _db.Cajas_DistribuyeSaldoFavor(codEmpresa, dto);
        }

        public ErrorDto Cajas_DesglocePago_Guardar(int CodEmpresa, CajasDesglocePagoRequest request)
        {
            return _db.Cajas_DesglocePago_Guardar(CodEmpresa, request);
        }

        public ErrorDto<CajasCatalogosDto> Cajas_Catalogos_Obtener(int CodEmpresa, string codCliente, string codCaja,
            int apertura, string? tiquete, string? productoCodigo, int? productoNumero)
        {
            return _db.Cajas_Catalogos_Obtener(CodEmpresa, codCliente, codCaja, apertura, tiquete, productoCodigo, productoNumero);
        }

        public ErrorDto<List<CajasFormaPagoDto>> Cajas_FormasPago_Obtener(int CodEmpresa, string codCaja)
        {
            return _db.Cajas_FormasPago_Obtener(CodEmpresa, codCaja);
        }

        public ErrorDto<List<CajasTiqueteDto>> Cajas_Tiquete_Obtener(int CodEmpresa, string codCaja, string tiquete, int apertura)
        {
            return _db.Cajas_Tiquete_Obtener(CodEmpresa, codCaja, tiquete, apertura);
        }
    }
}