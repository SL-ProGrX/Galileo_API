using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic
{
    public class FrmAfComisionesBL
    {
        private readonly FrmAfComisionesDB _db;
        public FrmAfComisionesBL(IConfiguration config)
        {
            _db = new FrmAfComisionesDB(config);
        }

        #region Remesa

        public ErrorDto<TablasListaGenericaModel> AF_ComisionesRemesa_Obtener(int CodEmpresa, bool exporta, string jFiltro)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jFiltro);
            if (filtros == null)
            {
                throw new ArgumentNullException(nameof(jFiltro), "Deserialized filtros is null.");
            }
            return _db.AF_ComisionesRemesa_Obtener(CodEmpresa, exporta, filtros);
        }

        public ErrorDto<decimal> AF_ComisionesRemesa_Total(int CodEmpresa, int cod_comision)
        {
            return _db.AF_ComisionesRemesa_Total(CodEmpresa, cod_comision);
        }

        public ErrorDto AF_ComisionesRemesa_Guardar(int CodEmpresa, string usuario, AfComisionDto comision)
        {
            return _db.AF_ComisionesRemesa_Guardar(CodEmpresa, usuario, comision);
        }

        public ErrorDto AF_ComisionesRemesa_Borrar(int CodEmpresa, string usuario, int cod_comision)
        {
            return _db.AF_ComisionesRemesa_Borrar(CodEmpresa, usuario, cod_comision);
        }

        #endregion

        #region Generacion

        public ErrorDto<List<AfComisionDto>> AF_ComisionesGenera_Obtener(int CodEmpresa)
        {
            return _db.AF_ComisionesGenera_Obtener(CodEmpresa);
        }


        public ErrorDto<List<AfComisionPromotorData>> AF_ComisionesGenera_Buscar(int CodEmpresa, string tipo)
        {
            return _db.AF_ComisionesGenera_Buscar(CodEmpresa, tipo);
        }

        public ErrorDto AF_ComisionesGenera_Generar(int CodEmpresa, string usuario, int comision, List<AfComisionPromotorData> promotor)
        {
            return _db.AF_ComisionesGenera_Generar(CodEmpresa, usuario, comision, promotor);
        }

        #endregion

        #region Pago

        public ErrorDto<List<AfComisionDto>> AF_ComisionesPago_Obtener(int CodEmpresa)
        {
            return _db.AF_ComisionesPago_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_ComisionesPagoBanco_Obtener(int CodEmpresa, int comision)
        {
            return _db.AF_ComisionesPagoBanco_Obtener(CodEmpresa, comision);
        }

        public ErrorDto<List<AfComisionPagoData>> AF_ComisionesPago_Buscar(int CodEmpresa, int comision, int banco)
        {
            return _db.AF_ComisionesPago_Buscar(CodEmpresa, comision, banco);
        }

        public ErrorDto AF_ComisionesPago_Generar(int CodEmpresa, string usuario, int comision, List<AfComisionPagoData> pagos)
        {
            return _db.AF_ComisionesPago_Generar(CodEmpresa, usuario, comision, pagos);
        }

        #endregion

        #region Reportes

        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepBancos_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            return _db.Af_Comisiones_RepBancos_Obtener(CodEmpresa, chkRepRemesas, cod_comision);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepPromotores_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            return _db.Af_Comisiones_RepPromotores_Obtener(CodEmpresa, chkRepRemesas, cod_comision);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepRemesa_Obtener(int CodEmpresa)
        {
            return _db.Af_Comisiones_RepRemesa_Obtener(CodEmpresa);
        }
        
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepUsuario_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            return _db.Af_Comisiones_RepUsuario_Obtener(CodEmpresa, chkRepRemesas, cod_comision);
        }

        #endregion
    }
}