using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesBancosCargadoBL
    {
        private readonly FrmTesBancosCargadoDB _Db;

        public FrmTesBancosCargadoBL(IConfiguration config)
        {
            _Db = new FrmTesBancosCargadoDB(config);
        }

        public ErrorDto<List<DropDownListaBancosCargados>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return _Db.Tes_Bancos_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<TesBancoCargadoConceptos>> Tes_BancosCargadoConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            return _Db.Tes_BancosCargadoConceptos_Obtener(CodEmpresa, concepto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroUnidades_Obtener(int CodEmpresa)
        {
            return _Db.Tes_BancosCargadoCentroUnidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroCostos_Obtener(int CodEmpresa, int contabilidad, string? unidad = null)
        {
            return _Db.Tes_BancosCargadoCentroCostos_Obtener(CodEmpresa, contabilidad, unidad);
        }

        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, string filtros)
        {
            FiltrosLazyLoadData Jfiltros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return _Db.Tes_AutoRegistroLista_Obtener(CodEmpresa, Jfiltros);
        }

        public ErrorDto TES_BancosCargados_Aplicar(int CodEmpresa, string cod_banco, string usuario, List<TesCargadoExcelDto> file)
        {
            return _Db.TES_BancosCargados_Aplicar(CodEmpresa, cod_banco, usuario, file);
        }

        public ErrorDto<List<TeslistaRegistroBancosDto>> TES_ListaRegistroBancos_Obtener(int CodEmpresa, string filtros)
        {
            return _Db.TES_ListaRegistroBancos_Obtener(CodEmpresa, filtros);
        }


        public async Task<ErrorDto> TES_RegistrosBancosCargados_Aplicar(int CodEmpresa, string registroLista)
        {
            return await _Db.TES_RegistrosBancosCargados_Aplicar(CodEmpresa, registroLista);
        }

        public ErrorDto TES_RegistrosBancosCargados_Elimina(int CodEmpresa, string registroLista)
        {
            return _Db.TES_RegistrosBancosCargados_Elimina(CodEmpresa, registroLista);
        }

        /// <summary>
        /// Obtiene los movimientos del banco para el tab Detalle de Movimientos.
        /// </summary>
        public ErrorDto<List<TeslistaRegistroBancosDto>> TES_ListaDetalleMovimientos_Obtener(int CodEmpresa, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<TesFiltrosDetalleMovimientoDto>(filtros) ?? new TesFiltrosDetalleMovimientoDto();
            return _Db.TES_ListaDetalleMovimientos_Obtener(CodEmpresa, filtro);
        }

        /// <summary>
        /// Excluye líneas del Tab Detalle Movimientos, revirtiendo Saldo a Favor y Depósito Trámite si aplican.
        /// </summary>
        public ErrorDto TES_BancosCargado_DetalleExcluir(int CodEmpresa, TesBancosCargadoDetalleExcluirModel data)
        {
            return _Db.TES_BancosCargado_DetalleExcluir(CodEmpresa, data);
        }

        /// <summary>
        /// Registra la transacción y asientos para líneas del Tab Detalle Movimientos.
        /// </summary>
        public ErrorDto TES_BancosCargado_DetalleRegistrar(int CodEmpresa, TesBancosCargadoDetalleRegistrarModel data)
        {
            return _Db.TES_BancosCargado_DetalleRegistrar(CodEmpresa, data);
        }

        /// <summary>
        /// Reclasifica el COD_CONCEPTO en TES_TRANSACCIONES para una lista de solicitudes bancarias.
        /// </summary>
        public ErrorDto TES_BancosCargado_ReclasificaConcepto(int CodEmpresa, TesBancosCargadoReclasificaConceptoModel data)
        {
            return _Db.TES_BancosCargado_ReclasificaConcepto(CodEmpresa, data);
        }

        /// <summary>
        /// Obtiene los depósitos para el tab Revisión de Movimientos.
        /// </summary>
        public ErrorDto<List<TesBancosCargadoRevMovDto>>
            TES_BancosCargado_RevMov_Obtener(
                int CodEmpresa,
                TesBancosCargadoRevMovRequest request)
        {
            return _Db.TES_BancosCargado_RevMov_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los movimientos bancarios candidatos para conciliación.
        /// </summary>
        public ErrorDto<List<TesBancosCargadoRevMovConciliaDto>>
            TES_BancosCargado_RevMovConcilia_Obtener(
                int CodEmpresa,
                TesBancosCargadoRevMovConciliaRequest request)
        {
            return _Db.TES_BancosCargado_RevMovConcilia_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Solicita la conciliación de solicitudes con una solicitud destino.
        /// </summary>
        public ErrorDto TES_BancosCargado_RevMovConcilia_Aplicar(
            int CodEmpresa,
             string usuario,
            TesBancosCargadoRevMovConciliaAplicarRequest request)
        {
            return _Db.TES_BancosCargado_RevMovConcilia_Aplicar(
                CodEmpresa,
                usuario,
                request);
        }

    }
}



