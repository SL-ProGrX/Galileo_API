using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasTrasladosEfectivoBl
    {
        private readonly FrmCajasTrasladosEfectivoDb DbCajasTrasladosEfectivo;

        public FrmCajasTrasladosEfectivoBl(IConfiguration config)
        {
            DbCajasTrasladosEfectivo = new FrmCajasTrasladosEfectivoDb(config);
        }

        public ErrorDto<List<CajasTrasladosEfectivoDto>> Cajas_TrasladosEfectivo_Obtener(int CodEmpresa, string Filtros)
        {
            var filtros = JsonConvert.DeserializeObject<CajasTrasladosEfectivoFiltros>(Filtros) 
                ?? new CajasTrasladosEfectivoFiltros { cod_caja = string.Empty };
            return DbCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TrasladosEfectivo_Catalogo_Obtener(int CodEmpresa, int Index, string IdCaja)
        {
            return DbCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Catalogo_Obtener(CodEmpresa, Index, IdCaja);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TrasladosEfectivo_Movimientos_Obtener(int CodEmpresa, string IdCaja)
        {
            return DbCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Movimientos_Obtener(CodEmpresa, IdCaja);
        }

        public ErrorDto<decimal> Cajas_TrasladosEfectivo_TipoCambio_Obtener(int CodEmpresa, string Divisa)
        {
            return DbCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_TipoCambio_Obtener(CodEmpresa, Divisa);
        }

        public ErrorDto Cajas_TrasladosEfectivo_Resolucion_Aplicar(int CodEmpresa, CajasTeResolucionRequest Request)
        {
            return DbCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Resolucion_Aplicar(CodEmpresa, Request);
        }

        public ErrorDto Cajas_TrasladosEfectivo_Registrar(int CodEmpresa, string Movimiento, CajasTrasladosEfectivoDto Request)
        {
            return DbCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Registrar(CodEmpresa, Movimiento, Request);
        }
    }
}