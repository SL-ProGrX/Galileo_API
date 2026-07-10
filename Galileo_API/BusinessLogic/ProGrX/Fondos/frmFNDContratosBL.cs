using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndContratosBL
    {
        private readonly FrmFndContratosDB _Db;

        public FrmFndContratosBL(IConfiguration? config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndContratosDB(config);
        }

        #region General
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_Listas_Obtener(int CodEmpresa, string lista)
        {
            return _Db.Fnd_Contratos_Listas_Obtener(CodEmpresa, lista);
        }

        public ErrorDto<ContratosModels> Fnd_Contratos_Obtener(int CodEmpresa, int operadora, string cod_plan, int contrato, string usuario)
        {
            return _Db.Fnd_Contratos_Obtener(CodEmpresa, operadora, cod_plan, contrato, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_PlanLista_Obtener(int CodEmpresa, int operadora, string usuario)
        {
            return _Db.Fnd_Contratos_PlanLista_Obtener(CodEmpresa, operadora, usuario);
        }

        public ErrorDto<ContratosPlanModels> Fnd_Contratos_Plan_Obtener(int CodEmpresa, int operadora, string plan)
        {
            return _Db.Fnd_Contratos_Plan_Obtener(CodEmpresa, operadora, plan);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_InversionPlazos_Obtener(int CodEmpresa, string codigo)
        {
            return _Db.Fnd_Contratos_InversionPlazos_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<FndContratosListaData> Fnd_Contratos_Buscar(int CodEmpresa, int operadora, string plan, string strFiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(strFiltros)!;
            return _Db.Fnd_Contratos_Buscar(CodEmpresa, operadora, plan, filtros);
        }

        public ErrorDto<string> Fnd_Contratos_Email_Enviar(int CodEmpresa, int operadora, string codigo, int contrato, string usuario)
        {
            return _Db.Fnd_Contratos_Email_Enviar(CodEmpresa, operadora, codigo, contrato, usuario);
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un contrato FND según el estado isNew del modelo.
        /// Retorna el código de contrato resultante en caso de éxito.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que registra el cambio.</param>
        /// <param name="vCambios">Valores anteriores del contrato para registrar cambios.</param>
        /// <param name="contrato">Datos actuales del contrato a guardar.</param>
        /// <returns>Resultado de la operación con el código de contrato resultante.</returns>
        public ErrorDto<long> Fnd_Contratos_Guardar(int CodEmpresa, string usuario, FndCambios vCambios, ContratosModels contrato)
        {
            var result = _Db.Fnd_Contratos_Guardar(CodEmpresa, usuario, vCambios, contrato);
            if (result.Code != 0)
            {
                return new ErrorDto<long> { Code = result.Code, Description = result.Description };
            }

            return new ErrorDto<long> { Code = 0, Result = contrato.cod_contrato };
        }

        public ErrorDto Fnd_Contratos_Borrar(int CodEmpresa, int operadora, string codigo, int contrato, string usuario)
        {
            return _Db.Fnd_Contratos_Borrar(CodEmpresa, operadora, codigo, contrato, usuario);
        }

        public ErrorDto<int> Fnd_Contratos_FrecuenciaMeses_Obtener(int CodEmpresa, string CuponFrecuencia)
        {
            return _Db.Fnd_Contratos_FrecuenciaMeses_Obtener(CodEmpresa, CuponFrecuencia);
        }

        public ErrorDto<decimal> fxTasaRef(FndContratoTasaRefParams param)
        {
            return _Db.fxTasaRef(param);
        }

        public ErrorDto<FndSociosListaData> Fnd_ContratosSocios_Obtener(int CodEmpresa, string strFiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(strFiltros)!;
            return _Db.Fnd_ContratosSocios_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_spFnd_Cupon_Frecuencia(int CodEmpresa, string plazo_id, string plan)
        {
            return _Db.Fnd_Contratos_spFnd_Cupon_Frecuencia(CodEmpresa, plazo_id, plan);
        }

        public ErrorDto<int> Fnd_Contratos_spFnd_Inversion_Plazos_Dias(int CodEmpresa, int plazo_inversion, string cboPlazo)
        {
            return _Db.Fnd_Contratos_spFnd_Inversion_Plazos_Dias(CodEmpresa, plazo_inversion, cboPlazo);
        }
        #endregion

        #region Complementario
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_CuentasBancarias_Obtener(int CodEmpresa, string cedula, int cod_banco)
        {
            return _Db.Fnd_Contratos_CuentasBancarias_Obtener(CodEmpresa, cedula, cod_banco);
        }

        #endregion

        #region Destinos

        public ErrorDto<List<FndContratoDestinoData>> Fnd_Contratos_Destinos_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _Db.Fnd_Contratos_Destinos_Obtener(CodEmpresa, pOperadora, pPlan, pContrato);
        }

        public ErrorDto Fnd_Contratos_Destinos_Guardar(int CodEmpresa, FndContratoDestinoData destino)
        {
            return _Db.Fnd_Contratos_Destinos_Guardar(CodEmpresa, destino);
        }

        #endregion

        #region Beneficiarios

        public ErrorDto<List<FndContratoBeneficiariosData>> Fnd_Contratos_Beneficiarios_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato, string cedula)
        {
            return _Db.Fnd_Contratos_Beneficiarios_Obtener(CodEmpresa, pOperadora, pPlan, pContrato, cedula);
        }

        #endregion

        #region SubCuentas

        public ErrorDto<List<FndContratoSubCuentasData>> Fnd_Contratos_SubCuentas_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato, string cedula)
        {
            return _Db.Fnd_Contratos_SubCuentas_Obtener(CodEmpresa, pOperadora, pPlan, pContrato, cedula);
        }

        public ErrorDto<int> fxSubCuentaContrato(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _Db.fxSubCuentaContrato(CodEmpresa, pOperadora, pPlan, pContrato);
        }

        public ErrorDto Fnd_Contratos_SubCuentas_Guardar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            return _Db.Fnd_Contratos_SubCuentas_Guardar(CodEmpresa, usuario, subCuenta);
        }

        #endregion

        #region Retiros

        public ErrorDto<FndContratosLiquidacionesListaData> Fnd_Contratos_Retiros_Obtener(int CodEmpresa, int operadora, string plan, int contrato, string strFiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(strFiltros)!;
            return _Db.Fnd_Contratos_Retiros_Obtener(CodEmpresa, operadora, plan, contrato, filtros);
        }

        #endregion

        #region Cupones
        public ErrorDto<List<FndContratosCuponesData>> Fnd_Contratos_Cupones_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _Db.Fnd_Contratos_Cupones_Obtener(CodEmpresa, pOperadora, pPlan, pContrato);
        }
        #endregion

        #region Bitacora

        public ErrorDto<List<FndContratoBitacoraData>> Fnd_Contratos_Bitacora_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _Db.Fnd_Contratos_Bitacora_Obtener(CodEmpresa, pOperadora, pPlan, pContrato);    
        }

        #endregion

        #region TP

        public ErrorDto<FndContratoTasaPreferencial> Fnd_Contratos_TP_Obtener(int CodEmpresa, int operadora, string plan, int contrato, string cedula)
        {
           return _Db.Fnd_Contratos_TP_Obtener(CodEmpresa, operadora, plan, contrato, cedula);
        }

        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Solicita(int CodEmpresa, FndContratoTasaPreferencial solicitud)
        {
            return _Db.Fnd_Contratos_TP_Solicita(CodEmpresa, solicitud);
        }

        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Estado(int CodEmpresa, int gestion_id)
        {
            return _Db.Fnd_Contratos_TP_Estado(CodEmpresa, gestion_id);
        }

        #endregion

    }
}
